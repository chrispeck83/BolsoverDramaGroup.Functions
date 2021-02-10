using System;
using System.Collections.Generic;
using System.Text;

namespace BolsoverDramaGroup.Functions.Square.Interfaces
{
    interface ITransactionsFactory
    {
        public ICollection<Transaction> GetTransactions(SquareClient client);
    }
}
