using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using sqs_processor.DbContexts;
using sqs_processor.Services.amazon;
using sqs_processor.Services.Network;
using sqs_processor.Services.Network.Dividends;
using sqs_processor.Services.Network.Earnings;
using sqs_processor.Services.repos;

namespace sqs_processor
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<Worker>();
                    services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
                    services.AddScoped<ISecuritiesRepository, SecuritiesRepository>();
                    services.AddScoped<IWebClientWrapper, WebClientWrapper>();
                    services.AddScoped<IAmazonUtility, AmazonUtility>();
                    

                    IConfiguration configuration = hostContext.Configuration;
                    var apiKey = configuration.GetSection("FMPAPIKey").Value;
                    
                    services.AddScoped<IGetSecurityService>(_ => new GetSecurityFromFMP(new WebClientWrapper(), apiKey));

                    services.AddScoped<IGetEarningsService>(_ => new GetEarningsFMP(new WebClientWrapper(), apiKey));

                    services.AddScoped<IGetDividendsServices>(_ => new GetDividendFMPrep(new WebClientWrapper(), apiKey));

                    services.AddScoped<ISecuritiesRepository, SecuritiesRepository>();
                    var serverVersion = new MySqlServerVersion(new Version(8, 0, 20));
                    string sqlConnection = configuration.GetSection("MYSQLConnection").Value;

                    string endpoint = System.Environment.GetEnvironmentVariable("MYSQLPassword");
                    sqlConnection = sqlConnection.Replace("EnvironmentPassword", endpoint);


                    services.AddDbContextPool<SecuritiesLibraryContext>(options => options.UseMySql(sqlConnection, serverVersion
               , builder =>
               {
                   builder.EnableRetryOnFailure(5, TimeSpan.FromSeconds(5), null);
               }

               ));
                    


                    //Configuration.GetConnectionString("FinancialServices")







                });


    }
}
