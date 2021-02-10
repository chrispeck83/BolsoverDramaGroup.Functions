using BolsoverDramaGroup.Functions.Factory.Interfaces;
using BolsoverDramaGroup.Functions.Factory.SquarePoS;
using BolsoverDramaGroup.Functions.Factory.Storage;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(BolsoverDramaGroup.Functions.POS.Startup))]
namespace BolsoverDramaGroup.Functions.POS
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            //builder.Services.AddHttpClient();

            //builder.Services.AddSingleton<ITransactionsFactory>((s) =>
            //{
            //    return new SquareTransactionsFactory();
            //});

            //var squareToken = Environment.GetEnvironmentVariable("SquareTokenFromKeyVault", EnvironmentVariableTarget.Process);

            builder.Services.AddSingleton<ITransactionsFactory,SquareTransactionsFactory>();
            builder.Services.AddSingleton<IStorageFactory, StorageFactory>();
            builder.Services.AddLogging();

            //builder.Services.AddSingleton<ILoggerProvider, MyLoggerProvider>();
        }
    }
}
