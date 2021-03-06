﻿namespace Battleship.Game.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Battleship.Core.Messages;
    using Battleship.Game.Board;
    using Battleship.Game.Handlers;
    using Battleship.Game.Infrastructure;
    using Battleship.Game.Models;
    using Battleship.Infrastructure.Core.Components;
    using Battleship.Infrastructure.Core.Messages;
    using Battleship.Infrastructure.Core.Models;

    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Localization;
    using Microsoft.Extensions.Primitives;

    using Newtonsoft.Json;

    /// <summary>
    ///     The game board generation
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class BoardController : ContextBase, IBoardController
    {
        #region Fields

        private readonly IGameRepository gameRepository;

        private readonly IGridGenerator gridGenerator;

        private readonly JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings
                                                                             {
                                                                                 TypeNameHandling = TypeNameHandling.All,
                                                                                 DefaultValueHandling = DefaultValueHandling.Ignore
                                                                             };

        private readonly IStringLocalizer<BoardController> localizer;

        private readonly sbyte marker = 0x01;

        private readonly IMessagePublisher messagePublisher;

        #endregion

        #region Constructors

        public BoardController(IGameRepository gameRepository, IMessagePublisher messagePublisher, IStringLocalizer<BoardController> localizer)
            : base(messagePublisher)
        {
            this.gridGenerator = GridGenerator.Instance();
            this.gameRepository = gameRepository;
            this.messagePublisher = messagePublisher;
            this.localizer = localizer;
        }

        #endregion

        #region Methods

        [HttpGet]
        [Route("GenerateBoard")]
        public async Task<ActionResult<GamingGrid>> GetGamingGrid()
        {
            try
            {
                GamingGrid result = await Task.Run(
                                        () =>
                                            {
                                                GamingGrid gamingGrid = new GamingGrid
                                                                            {
                                                                                X = this.GetXAxis(),
                                                                                Y = this.GetYAxis()
                                                                            };
                                                return gamingGrid;
                                            });

                return result;
            }
            catch (Exception)
            {
                return this.BadRequest();
            }
        }

        [HttpPost]
        [Route("UserInput")]
        public async Task<ActionResult> UserInput([FromBody] PlayerCommand playerCommand)
        {
            try
            {
                string sessionToken = this.GetAuthorizationToken(this.HttpContext);
                string coordinates = string.Empty;
                Guid playerId = Guid.Empty;
                StatusCodeResult playerStatus = this.ValidatePlayerContext(sessionToken, playerCommand, ref coordinates, ref playerId);

                if (playerStatus == null)
                {
                    // get into a object
                    Dictionary<Coordinate, Segment> shipCoordinates = JsonConvert.DeserializeObject<KeyValuePair<Coordinate, Segment>[]>(coordinates, this.jsonSerializerSettings)
                       .ToDictionary(kv => kv.Key, kv => kv.Value);

                    // Count of ship lengths coordinates along any length - tp check game completed
                    int sum = shipCoordinates.Count(q => q.Key.X != this.marker);

                    KeyValuePair<Coordinate, Segment> shipCoordinate =
                        shipCoordinates.FirstOrDefault(q => q.Key.X == playerCommand.Coordinate.X && q.Key.Y == playerCommand.Coordinate.Y);

                    playerCommand.ScoreCard.IsHit = false;
                    if (shipCoordinate.Value != null)
                    {
                        // Meta
                        shipCoordinate.Value.Ship.ShipSegmentHit = this.marker;
                        int numberOfSegmentsHit = shipCoordinates.Count(
                            q => q.Value.Ship.ShipIndex == shipCoordinate.Value.Ship.ShipIndex && q.Value.Ship.ShipSegmentHit == this.marker);
                        int totalNumberOfShipsHit = shipCoordinates.Count(q => q.Value.Ship.ShipSegmentHit == this.marker);

                        // Save
                        string updateShipCoordinates = JsonConvert.SerializeObject(shipCoordinates.ToArray(), Formatting.Indented, this.jsonSerializerSettings);
                        await this.gameRepository.UpdateShipCoordinates(updateShipCoordinates, sessionToken);

                        // Display
                        playerCommand.ScoreCard.Hit++;
                        playerCommand.ScoreCard.IsHit = true;
                        playerCommand.ScoreCard.Message = this.localizer["Boom! You hit a ship!"];
                        if (numberOfSegmentsHit == shipCoordinate.Value.Ship.ShipLength)
                        {
                            playerCommand.ScoreCard.Sunk++;
                            playerCommand.ScoreCard.Message = this.localizer["Ship sunk!"];
                        }

                        if (sum == totalNumberOfShipsHit)
                        {
                            // Get the player via the RPC service
                            PlayerHandler playerHandler = new PlayerHandler(this.messagePublisher);
                            string payload = playerHandler.GetPlayer(playerId);
                            if (!string.IsNullOrEmpty(payload))
                            {
                                Player player = JsonConvert.DeserializeObject<Player>(payload);
                                if (player != null && !player.IsDemoPlayer)
                                {
                                    Statistics statistics = new Statistics
                                                                {
                                                                    FullName = $"{player.Firstname} {player.Lastname}",
                                                                    Email = player.Email,
                                                                    ScoreCard = playerCommand.ScoreCard
                                                                };

                                    string playerStatisticsPayload = JsonConvert.SerializeObject(statistics);
                                    await this.messagePublisher.PublishMessageAsync(playerStatisticsPayload, "Statistics");
                                }

                                playerCommand.ScoreCard.IsCompleted = true;
                                playerCommand.ScoreCard.Message = this.localizer["Game completed!"];
                            }

                            playerHandler.Close();
                        }
                    }
                    else
                    {
                        playerCommand.ScoreCard.Miss++;
                        playerCommand.ScoreCard.Message = this.localizer["Sorry you missed, try again!"];
                    }
                }

                playerCommand.ScoreCard.Total++;
                playerCommand.ScoreCard.SessionToken = sessionToken;

                await this.messagePublisher.PublishMessageAsync(JsonConvert.SerializeObject(playerCommand.ScoreCard), "ScoreCard");
            }
            catch (Exception e)
            {
                this.Log(e);
                return this.StatusCode(StatusCodes.Status500InternalServerError);
            }

            return this.Ok(JsonConvert.SerializeObject(playerCommand.ScoreCard));
        }

        [HttpPost]
        public async Task<ActionResult> SetGameCompleted()
        {
            try
            {
                string sessionToken = this.GetAuthorizationToken(this.HttpContext);
                bool result = await this.gameRepository.SetGameCompleted(sessionToken);
                return this.Ok(result);
            }
            catch (Exception e)
            {
                this.Log(e);
                return this.StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet]
        [Route("StartGame")]
        public async Task<ActionResult> StartGame(int numberOfShips)
        {
            try
            {
                if (numberOfShips == 0) return this.BadRequest();

                string sessionToken = this.GetAuthorizationToken(this.HttpContext);
                if (string.IsNullOrEmpty(sessionToken)) return this.StatusCode(StatusCodes.Status401Unauthorized);

                await this.gameRepository.StartGame(sessionToken, numberOfShips);

                this.Log($"Game started on session: {sessionToken}");

                // Publish the message to the ScoreCard Queue
                ScoreCard scoreCard = new ScoreCard
                                          {
                                              SessionToken = sessionToken,
                                              Message = this.localizer["Let the games begin!"].Value,
                                              Hit = 0,
                                              Miss = 0,
                                              Sunk = 0,
                                              IsCompleted = false,
                                              IsHit = false,
                                              Total = 0
                                          };
                string serializedScoreCard = JsonConvert.SerializeObject(scoreCard);
                await this.messagePublisher.PublishMessageAsync(serializedScoreCard, "ScoreCard");

                return this.StatusCode(StatusCodes.Status200OK);
            }
            catch (Exception e)
            {
                this.Log(e);
                return this.StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet]
        public string Get()
        {
            return "Board API started.";
        }

        private IEnumerable<string> GetXAxis()
        {
            try
            {
                return this.gridGenerator.GetAlphaColumnChars();
            }
            catch (Exception)
            {
                return null;
            }
        }

        private IEnumerable<int> GetYAxis()
        {
            try
            {
                return this.gridGenerator.GetNumericRows();
            }
            catch (Exception)
            {
                return null;
            }
        }

        private string GetAuthorizationToken(HttpContext httpContext)
        {
            string result = string.Empty;
            if (httpContext.Request.Headers.TryGetValue("Authorization", out StringValues values)) result = values.FirstOrDefault();

            return result;
        }

        private StatusCodeResult? ValidatePlayerContext(string sessionToken, PlayerCommand playerCommand, ref string coordinates, ref Guid playerId)
        {
            StatusCodeResult status = null;
            if (string.IsNullOrEmpty(sessionToken))
                status = this.StatusCode(StatusCodes.Status401Unauthorized);
            else
                playerId = this.gameRepository.CheckPlayerStatus(sessionToken);

            if (playerCommand.Coordinate.X == 0 || playerCommand.Coordinate.Y == 0)
                status = this.StatusCode(StatusCodes.Status500InternalServerError);

            coordinates = this.gameRepository.GetShipCoordinates(sessionToken);
            if (coordinates == null)
            {
                this.Log(new NullReferenceException("Coordinates not found"));
                status = this.StatusCode(StatusCodes.Status204NoContent);
            }

            return status;
        }

        #endregion
    }
}