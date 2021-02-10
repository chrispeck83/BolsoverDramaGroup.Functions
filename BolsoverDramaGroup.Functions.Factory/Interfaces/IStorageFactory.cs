using BolsoverDramaGroup.Functions.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BolsoverDramaGroup.Functions.Factory.Interfaces
{
    public interface IStorageFactory
    {
        public Task<List<string>> StoreTransactionsAsync(IList<Transaction> tranactions);
    }
}
