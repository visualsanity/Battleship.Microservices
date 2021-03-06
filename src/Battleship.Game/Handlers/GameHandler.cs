﻿namespace Battleship.Game.Handlers
{
    using System;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using Battleship.Core.Messages;
    using Battleship.Game.Infrastructure;
    using Battleship.Infrastructure.Core.Messages;
    using Battleship.Infrastructure.Core.Models;

    using Microsoft.Extensions.Hosting;

    using Newtonsoft.Json;

    using RabbitMQ.Client;
    using RabbitMQ.Client.Events;

    using Serilog;

    public class GameHandler : BackgroundService
    {
        #region Fields

        private readonly IGameRepository gameRepository;

        private readonly IMessagePublisher messagePublisher;

        private IModel channel;

        private IConnection connection;

        #endregion

        #region Constructors

        public GameHandler(IGameRepository gameRepository, IMessagePublisher messagePublisher)
        {
            this.messagePublisher = messagePublisher;
            this.gameRepository = gameRepository;

            this.Initialise();
        }

        #endregion

        #region Methods

        public override void Dispose()
        {
            this.channel.Close();
            this.connection.Close();
            base.Dispose();
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            EventingBasicConsumer consumer = new EventingBasicConsumer(this.channel);
            consumer.Received += (sender, eventArgs) =>
                {
                    string content = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
                    try
                    {
                        if (!string.IsNullOrEmpty(content))
                        {
                            Player player = JsonConvert.DeserializeObject<Player>(content);
                            this.gameRepository.CreatePlayer(player.SessionToken, player.PlayerId);
                            this.channel.BasicAck(eventArgs.DeliveryTag, true);
                        }
                    }
                    catch (Exception exp)
                    {
                        Log.Error(exp, exp.Message);
                        this.channel.BasicAck(eventArgs.DeliveryTag, false);
                    }
                };

            this.channel.BasicConsume(this.messagePublisher.Queue, false, consumer);
            return Task.CompletedTask;
        }

        private void Initialise()
        {
            ConnectionFactory factory = new ConnectionFactory { HostName = this.messagePublisher.Host, UserName = this.messagePublisher.Username, Password = this.messagePublisher.Password };

            // create connection
            this.connection = factory.CreateConnection();

            // create channel
            this.channel = this.connection.CreateModel();

            this.channel.ExchangeDeclare(this.messagePublisher.Exchange, ExchangeType.Direct, true);
            this.channel.QueueDeclare(queue: this.messagePublisher.Queue, durable: false, exclusive: false, autoDelete: false, arguments: null);
            this.channel.QueueBind(this.messagePublisher.Queue, this.messagePublisher.Exchange, this.messagePublisher.Queue);
        }

        #endregion
    }
}