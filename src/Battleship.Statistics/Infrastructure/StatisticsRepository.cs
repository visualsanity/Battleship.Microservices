﻿namespace Battleship.Statistics.Infrastructure
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Battleship.Infrastructure.Core.Models;
    using Battleship.Infrastructure.Core.Repository;

    public class StatisticsRepository : RepositoryCore, IStatisticsRepository
    {
        #region Constructors

        public StatisticsRepository(string databaseName)
            : base(databaseName)
        {
        }

        #endregion

        #region Methods

        public async Task<IEnumerable<Statistics>> GetTopTenPlayers()
        {
            return await this.ExecuteAsync<Statistics>();
        }

        public async Task<Statistics> GetPlayerByEmail(string email)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object> { { "email", email } };

            return await this.ExecuteScalarAsync<Statistics>(parameters);
        }

        public async Task SaveStatistics(Statistics statistics)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                { "fullNAme", statistics.FullName },
                { "email", statistics.Email },
                { "Percentage", statistics.WinningPercentage },
                { "completedGames", statistics.CompletedGames },
            };

            await this.ExecuteAsync(parameters);
        }

        #endregion
    }
}