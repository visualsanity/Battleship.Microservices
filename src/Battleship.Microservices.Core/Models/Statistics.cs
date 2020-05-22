﻿namespace Battleship.Microservices.Core.Models
{
    using Newtonsoft.Json;

    public class Statistics
    {
        [JsonProperty("firstName")]
        public string Firstname { get; set; }

        [JsonProperty("lastName")]
        public string Lastname { get; set; }

        [JsonIgnore]
        public string Email { get; set; }

        [JsonProperty("winningPercentage")]
        public double WinningPercentage { get; set; }
    }
}