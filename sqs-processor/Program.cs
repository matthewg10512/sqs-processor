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
using sqs_processor.Services.context;
using sqs_processor.Services.Factories;
using sqs_processor.Services.Network;
using sqs_processor.Services.Network.Dividends;
using sqs_processor.Services.Network.Earnings;
using sqs_processor.Services.Network.HistoricalPrices;
using sqs_processor.Services.Network.Profile;
using sqs_processor.Services.repos;
using sqs_processor.Services.Utility;

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
                    services.AddScoped<IUtility, Utility>();



                    IConfiguration configuration = hostContext.Configuration;
                    var apiKey = configuration.GetSection("FMPAPIKey").Value;
                    
                    services.AddScoped<IGetSecurityService>(_ => new GetSecurityFromFMP(new WebClientWrapper(), apiKey));

                    services.AddScoped<IGetEarningsService>(_ => new GetEarningsFMP(new WebClientWrapper(), apiKey));

                    services.AddScoped<IGetDividendsServices>(_ => new GetDividendFMPrep(new WebClientWrapper(), apiKey));
                    services.AddScoped<IGetHistoricalPricesService>(_ => new GetHistoricalPriceFMP(new WebClientWrapper(), apiKey));
                    services.AddScoped<IGetSecurityProfile>(_ => new GetSecurityProfileFromFMP(new WebClientWrapper(), apiKey));

                    
                    DbContextOptions<SecuritiesLibraryContext> options = new DbContextOptions<SecuritiesLibraryContext>();
                   
                    services.AddScoped<ISecuritiesRepository, SecuritiesRepository>();
                    var serverVersion = new MySqlServerVersion(new Version(8, 0, 20));
                    string sqlConnection = configuration.GetSection("MYSQLConnection").Value;

                    string endpoint = System.Environment.GetEnvironmentVariable("MYSQLPassword");

                    sqlConnection = sqlConnection.Replace("EnvironmentPassword", endpoint);



                    var contextOptions = new DbContextOptionsBuilder<SecuritiesLibraryContext>()
                   .UseMySql(sqlConnection, serverVersion
                      , builder =>
                      {
                          builder.EnableRetryOnFailure(5, TimeSpan.FromSeconds(5), null);
                      }
                      )
           .Options;


                    services.AddScoped<IContextOptions>(_ => new ContextOptions(contextOptions));

                    services.AddScoped<IUnitofWorkFactory, UnitofWorkFactory>();
                    

                    services.AddScoped<IServiceFactory, ServiceFactory>();

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
