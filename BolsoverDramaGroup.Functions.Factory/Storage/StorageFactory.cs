using BolsoverDramaGroup.Functions.Factory.Interfaces;
using BolsoverDramaGroup.Functions.Models;
using System;
using System.Collections.Generic;
using System.Text;
using Azure.Storage.Blobs;
using System.Threading.Tasks;
using System.Linq;
using System.IO;

namespace BolsoverDramaGroup.Functions.Factory.Storage
{  
    public class StorageFactory : IStorageFactory
    {
        public BlobContainerClient Container { get; }

        public StorageFactory()
        {
            Container = new BlobContainerClient(
                Environment.GetEnvironmentVariable("CloudStorageConnectionStringFromKeyVault", EnvironmentVariableTarget.Process),
                $"square-reports");
        }

        public async Task<List<string>> StoreTransactionsAsync(IList<Transaction> transactions)
        {
            await Container.CreateIfNotExistsAsync();
            List<string> errors = new List<string>();

            foreach (var cashier in transactions.Select(t => t.Cashier).Distinct())
            {
                BlobClient blob = Container.GetBlobClient($"{DateTime.Now.Year}_{DateTime.Now.Month}_{cashier}.csv");
                StringBuilder blobContents = new StringBuilder();

                foreach (var transaction in transactions.Where(t => t.Cashier.Equals(cashier)))
                {
                    blobContents.AppendLine(transaction.CsvLineText);
                }

                using (MemoryStream ms = new MemoryStream())
                {
                    var sw = new StreamWriter(ms);
                    try
                    {
                        sw.Write(blobContents.ToString());
                        sw.Flush();
                        ms.Seek(0, SeekOrigin.Begin);
                        await blob.UploadAsync(ms);
                    }
                    catch(Exception ex)
                    {
                        errors.Add($"Unable to store transactions for {cashier}: {ex.Message}");
                    }
                    finally
                    {
                        sw.Dispose();
                    }
                }
            }

            return errors;
        }
    }
}
