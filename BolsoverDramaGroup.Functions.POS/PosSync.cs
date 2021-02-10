using BolsoverDramaGroup.Functions.Factory.Interfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;

namespace BolsoverDramaGroup.Functions.POS
{
    public class PosSync
    {
        private readonly ITransactionsFactory _transactionsFactory;
        private readonly IStorageFactory _storageFactory;

        public PosSync(ITransactionsFactory transactionsFactory, IStorageFactory storageFactory)
        {
            _transactionsFactory = transactionsFactory;
            _storageFactory = storageFactory;
        }

        [FunctionName("SquareToCsv")]
        [StorageAccount("BolsoverDramaGroup")]
        public async void Run([TimerTrigger("0 0 0 1 * *")] TimerInfo myTimer, ILogger log, ExecutionContext context)
        {
            try
            {
                var transactions = await _transactionsFactory.GetTransactionsAsync(DateTime.Now.AddMonths(-1));
                log.LogInformation("Got transactions");

                if (transactions == null) log.LogError("Transactions is null");
                else if (transactions.Count == 0) log.LogInformation("No transactions");
                else
                {
                    log.LogInformation($"{transactions.Count} transactions found");
                    var errors = await _storageFactory.StoreTransactionsAsync(transactions);
                    log.LogInformation($"Transaction blobs created with {errors.Count} errors");
                    foreach(var error in errors)
                    {
                        log.LogError(error);
                    }
                }
            }
            catch (Exception e)
            {
                log.LogError(e.Message);
                log.LogError(e.StackTrace);
            }
        }
    }
}
