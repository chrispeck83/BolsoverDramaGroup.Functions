using BolsoverDramaGroup.Functions.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BolsoverDramaGroup.Functions.Factory.Interfaces
{
    public interface ITransactionsFactory
    {
        public Task<List<Transaction>> GetTransactionsAsync(DateTime lastRun);
    }
}
