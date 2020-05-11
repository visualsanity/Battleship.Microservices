﻿namespace Battleship.Statistics
{
    using Battleship.Microservices.Core.Messages;
    using Battleship.Microservices.Infrastructure.Messages;
    using Battleship.Statistics.Communication;

    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    public class Startup
    {
        #region Fields

        private readonly IConfiguration configuration;

        private readonly string database = "Database=Battleship.Player;";

        private string sqlConnectionString = string.Empty;

        #endregion

        #region Constructors

        public Startup(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        #endregion

        #region Methods

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMemoryCache();

            this.sqlConnectionString = this.configuration.GetConnectionString("BattleshipGameCN");
            string databaseConnection = $"{this.sqlConnectionString}{this.database}";

            // add message publisher classes
            IConfigurationSection configSection = this.configuration.GetSection("RabbitMQ");
            string host = configSection["Host"];
            string username = configSection["UserName"];
            string password = configSection["Password"];
            string exchange = configSection["Exchange"];
            string queue = configSection["Queue"];
            string rpcQueue = configSection["RPCQueue"];

            IConfigurationSection logPathConfigSection = this.configuration.GetSection("AuditLog");
            string auditPath = logPathConfigSection["AuditPath"];
            string auditQueue = configSection["AuditQueue"];

            services.AddTransient<IMessagePublisher>(sp => new MessagePublisher(host, username, password, exchange, queue));
            services.AddTransient<IRpcClient>(sp => new RpcClient(host, username, password, exchange, rpcQueue, databaseConnection));

            services.AddCors();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
            else
                app.UseHsts();
            app.UseCors(options => options.WithOrigins("http://localhost:4200").AllowAnyMethod().AllowAnyHeader().AllowAnyOrigin());

            app.UseRouting();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }

        #endregion
    }
}