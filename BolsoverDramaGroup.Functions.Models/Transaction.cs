using System;

namespace BolsoverDramaGroup.Functions.Models
{
    public class Transaction
    {
        private readonly DateTime _transactionDate;
        private readonly decimal _amount;
        private readonly string _description;

        public string Cashier { get; }

        public string CsvLineText
        {
            get
            {
                return $"{_transactionDate:dd/MM/yyyy},{_amount:F},{_description??"No description"}";
            }
        }

        public Transaction(string cashier, string dateTimeString, long? amount, string description)
        {
            Cashier = cashier;
            _transactionDate = DateTime.Parse(dateTimeString);
            _amount = (decimal)(amount / 100.00);
            _description = description.TrimEnd('/');
        }
    }
}
