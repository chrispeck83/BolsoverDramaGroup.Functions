using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BolsoverDramaGroup.Functions.Factory.Extensions;
using BolsoverDramaGroup.Functions.Factory.Interfaces;
using Square.Exceptions;
using Square.Models;

namespace BolsoverDramaGroup.Functions.Factory.SquarePoS
{
    public class SquareTransactionsFactory : SquareFactory, ITransactionsFactory
    {
        private List<Location> Locations { get; set; }

        public SquareTransactionsFactory()
        {
            Locations = Client.LocationsApi.ListLocations().Locations.Where(l => l.Status.Equals("ACTIVE")).ToList();
        }

        public async Task<List<Models.Transaction>> GetTransactionsAsync(DateTime lastRun)
        {
            var transactions = new List<Models.Transaction>();           
            var result = await SearchOrdersAsync(lastRun);
            foreach (var orderEntries in result.OrderEntries)
            {
                var order = Client.OrdersApi.RetrieveOrder(orderEntries.OrderId);
                if (order.Order.Tenders != null)
                {
                    transactions.AddRange(LogSale(order.Order));
                }

                if (order.Order.Refunds != null)
                {
                    transactions.AddRange(LogRefund(order.Order));
                }
            }

            return transactions;
        }

        private async Task<SearchOrdersResponse> SearchOrdersAsync(DateTime startAt) 
        {


            var bodyQueryFilterStateFilterStates = new List<string>();
            bodyQueryFilterStateFilterStates.Add("COMPLETED");
            var bodyQueryFilterStateFilter = new SearchOrdersStateFilter.Builder(
                    bodyQueryFilterStateFilterStates)
                .Build();
            var bodyQueryFilterDateTimeFilterCreatedAt = new TimeRange.Builder()
                .StartAt(startAt.ToRfc3339String())
                .EndAt(DateTime.UtcNow.ToRfc3339String())
                .Build();
            var bodyQueryFilterDateTimeFilter = new SearchOrdersDateTimeFilter.Builder()
                .Build();

            var bodyQueryFilter = new SearchOrdersFilter.Builder()
                .StateFilter(bodyQueryFilterStateFilter)
                .DateTimeFilter(bodyQueryFilterDateTimeFilter)
                .Build();
            var bodyQuerySort = new SearchOrdersSort.Builder("CREATED_AT")
                .Build();
            var bodyQuery = new SearchOrdersQuery.Builder()
                .Filter(bodyQueryFilter)
                .Sort(bodyQuerySort)
                .Build();
            var body = new SearchOrdersRequest.Builder()
                .LocationIds(Locations.Select(l => l.Id).ToList())
                .Query(bodyQuery)
                .ReturnEntries(true)
                .Build();

            try
            {
                var ordersApi = Client.OrdersApi;
                return await ordersApi.SearchOrdersAsync(body);
            }
            catch (ApiException e)
            {
                var errors = e.Errors;
                var statusCode = e.ResponseCode;
                var httpContext = e.HttpContext;
                StringBuilder errorDetails = new StringBuilder();
                errorDetails.AppendLine("ApiException occurred:");
                errorDetails.AppendLine("Headers:");
                foreach (var item in httpContext.Request.Headers)
                {
                    //Display all the headers except Authorization
                    if (item.Key != "Authorization")
                    {
                        errorDetails.AppendLine($"\t{item.Key}: {item.Value}");
                    }
                }
                Console.WriteLine("Status Code: {0}", statusCode);
                foreach (Error error in errors)
                {
                    errorDetails.AppendLine($"Error Category:{error.Category} Code:{error.Code} Detail:{error.Detail}");
                }

                throw new Exception(errorDetails.ToString());
            }
            catch (Exception e)
            {
                throw;
            }
        }


        private IList<Models.Transaction> LogSale(Order order)
        {
            List<Models.Transaction> transactions = new List<Models.Transaction>();
            StringBuilder description = new StringBuilder();
            if (order.LineItems != null)
            {
                foreach (var item in order.LineItems.GroupBy(i => i.Name).Select(i => i.Key))
                {
                    if (item != null)
                    {
                        try
                        {
                            description.Append($"{item} x {order.LineItems.Where(l => l.Name.Equals(item)).Sum(i => int.Parse(i.Quantity))}/");
                        }
                        catch(Exception e)
                        {
                            description.Append($"{item}:{e.Message}/");
                        }
                    }
                }
            }

            foreach (var tender in order.Tenders)
            {
                if (tender.AmountMoney.Amount > 0)
                {
                    transactions.Add(new Models.Transaction(
                        tender.Type.Equals("CARD") ? "Square" : Locations.Where(l => l.Id.Equals(order.LocationId)).FirstOrDefault()?.Name,
                        tender.CreatedAt,
                        tender.AmountMoney.Amount,
                        String.IsNullOrEmpty(tender.Note) ? description.ToString() : $"{tender.Note}: {description}"));

                    if (tender.Type == "CARD" && tender.ProcessingFeeMoney.Amount > 0)
                    {
                        transactions.Add(new Models.Transaction("Square", tender.CreatedAt, -tender.ProcessingFeeMoney.Amount, "Square Processing Fee"));
                    }
                }
            }

            return transactions;
        }

        private IList<Models.Transaction> LogRefund(Order order)
        {
            List<Models.Transaction> transactions = new List<Models.Transaction>();

            foreach (var ret in order.Returns)
            {
                StringBuilder description = new StringBuilder();
                description.Append("Refund: ");
                if (ret.ReturnLineItems != null)
                {
                    foreach (var item in ret.ReturnLineItems.GroupBy(i => i.Name).Select(i => i.Key))
                    {
                        if (item != null)
                        {
                            try
                            {
                                description.Append($"{item} x {ret.ReturnLineItems.Where(l => l.Name.Equals(item)).Sum(i => int.Parse(i.Quantity))}/");
                            }
                            catch(Exception e)
                            {
                                description.Append($"{item}:{e.Message}/");
                            }
                        }
                    }
                }

                var originalOrder = Client.OrdersApi.RetrieveOrder(ret.SourceOrderId);

                foreach (var tender in originalOrder.Order.Tenders.Where(t => order.Refunds.Select(r => r.TenderId).Contains(t.Id)))
                {
                    transactions.Add(new Models.Transaction(
                        tender.Type.Equals("CARD") ? "Square" : Locations.Where(l => l.Id.Equals(originalOrder.Order.LocationId)).FirstOrDefault()?.Name,
                        tender.CreatedAt,
                        -tender.AmountMoney.Amount,
                        String.IsNullOrEmpty(tender.Note) ? description.ToString() : $"{tender.Note}: {description}"));

                    if (tender.Type == "CARD" && tender.ProcessingFeeMoney.Amount > 0)
                    {
                        transactions.Add(new Models.Transaction("Square", tender.CreatedAt, tender.ProcessingFeeMoney.Amount, "Square Processing Fee"));
                    }
                }
            }

            return transactions;
        }
    }
}
