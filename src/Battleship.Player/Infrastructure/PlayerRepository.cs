﻿namespace Battleship.Player.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microservices.Infrastructure.Repository;
    using Models;

    public class PlayerRepository : RepositoryCore, IPlayerRepository
    {
        #region Constructors

        public PlayerRepository(string database) : base(database)
        {
        }

        #endregion

        #region Methods

        public async Task<Guid> CreatePlayer(Player player)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"firstname", player.Firstname},
                {"lastname", player.Lastname},
                {"email", player.Email},
                {"password", player.Password}
            };

            return await this.ExecuteScalarAsync<Guid>(parameters);
        }

        public async Task<IEnumerable<Player>> GetPlayers()
        {
            return await this.ExecuteAsync<Player>();
        }

        public async Task<IEnumerable<Player>> GetDemoPlayers()
        {
            return await this.ExecuteAsync<Player>();
        }

        public async Task<Player> GetPlayer(Guid playerId)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"PlayerId", playerId}
            };

            return await this.ExecuteScalarAsync<Player>(parameters);
        }

        public async Task<bool> PlayerLogout(Guid playerId)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"PlayerId", playerId}
            };

            return await this.ExecuteScalarAsync<bool>(parameters);
        }

        public async Task<Player> PlayerLogin(Player player)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"Email", player.Email},
                {"Password", player.Password}
            };

            return await this.ExecuteScalarAsync<Player>(parameters);
        }

        public async Task<Player> DemoLogin(Guid playerId)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"PlayerId", playerId}
            };

            return await this.ExecuteScalarAsync<Player>(parameters);
        }

        #endregion
    }
}