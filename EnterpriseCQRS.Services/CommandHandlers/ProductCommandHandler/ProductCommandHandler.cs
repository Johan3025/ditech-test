using EnterpriseCQRS.Data;
using EnterpriseCQRS.Data.Model;
using EnterpriseCQRS.Domain.Commands.ProductCommand;
using EnterpriseCQRS.Domain.Responses;
using EnterpriseCQRS.Services.CommandHandlers.Utilities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EnterpriseCQRS.Services.CommandHandlers.ProductCommandHandler
{
    public class ProductCommandHandler
    {
        public class GetTransactionCommandHandler : IRequestHandler<GetTransactionCommand, GenericResponse<IList<Transaction>>>
        {
            private readonly CommittedCapacityContext _context;
            private readonly ILogger logger;

            public GetTransactionCommandHandler(CommittedCapacityContext context, ILogger<GetTransactionCommandHandler> logger)
            {
                _context = context;
                this.logger = logger;
            }

            public async Task<GenericResponse<IList<Transaction>>> Handle(GetTransactionCommand request, CancellationToken cancellationToken)
            {
                logger.LogInformation("comienza a ejecutar el handler");
                var url = new Uri("http://quiet-stone-2094.herokuapp.com/transactions.json");
                var response = new GenericResponse<IList<Transaction>>();
                var transactions = new Utilities<Transaction>();

                logger.LogWarning("se realiza proceso de eliminado de info de la tabla");
                _context.Database.ExecuteSqlRaw("DELETE FROM [Transaction]");
                logger.LogWarning("Termino el proceso de eliminado de info de la tabla");

                logger.LogWarning("se realiza proceso de consumir servicio externo");
                var responses = await transactions.ExternalServiceUtility(url);
                logger.LogWarning("Termino el proceso de eliminado de info de la tabla");

                if (responses.Result is null)
                {
                    responses.Message = " el servicio externo no devolvio datos";
                    return responses;
                }

                logger.LogWarning("se realiza guardado de info en la tabla");
                await _context.Transaction.AddRangeAsync(responses.Result, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                logger.LogWarning("termino proceso de  guardado de info en la tabla");
                response.Message = "Guardado exitoso";
                //response.Result = responses.Result;

                return response;
            }
        }

        public class GetRateCommandHandler : IRequestHandler<GetRateCommand, GenericResponse<IList<Rates>>>
        {
            private readonly CommittedCapacityContext _context;
            private readonly ILogger<GetRateCommandHandler> logger;

            public GetRateCommandHandler(CommittedCapacityContext context, ILogger<GetRateCommandHandler> logger)
            {
                _context = context;
                this.logger = logger;
            }

            public async Task<GenericResponse<IList<Rates>>> Handle(GetRateCommand request, CancellationToken cancellationToken)
            {
                logger.LogInformation("comienza a ejecutar el handler");
                var url = new Uri("http://quiet-stone-2094.herokuapp.com/rates.json");
                var response = new GenericResponse<IList<Rates>>();
                var rates = new Utilities<Rates>();

                logger.LogInformation("se realiza proceso de eliminado de info de la tabla");
                _context.Database.ExecuteSqlRaw("DELETE FROM [Rates]");
                logger.LogInformation("Termino el proceso de eliminado de info de la tabla");

                logger.LogInformation("se realiza proceso de consumir servicio externo");
                var responses = await rates.ExternalServiceUtility(url);
                logger.LogInformation("Termino el proceso de eliminado de info de la tabla");

                if (responses.Result is null)
                {
                    responses.Message = " el servicio externo no devolvio datos";
                    return responses;
                }

                logger.LogInformation("se realiza guardado de info en la tabla");
                await _context.Rates.AddRangeAsync(responses.Result, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                logger.LogInformation("termino proceso de  guardado de info en la tabla");

                response.Message = "Guardado exitoso";
                //response.Result = responses.Result;
                return response;
            }
        }

        public class CalculateTransactionCommandHandler : IRequestHandler<CalculateTransactionCommand, GenericResponse<IList<Total>>>
        {
            private readonly CommittedCapacityContext _context;

            public CalculateTransactionCommandHandler(CommittedCapacityContext context)
            {
                _context = context;
            }

            public async Task<GenericResponse<IList<Total>>> Handle(CalculateTransactionCommand request, CancellationToken cancellationToken)
            {
                var response = new GenericResponse<IList<Total>>();
                var transactions = await _context.Transaction.Where(x => x.Sku.Equals(request.Sku)).ToListAsync();
                var rates = await _context.Rates.ToListAsync();
                var totalTransactionList = new List<TotalTransactionList>();
                var totals = new List<Total>();
                var rate = new List<Rates>();
                decimal acumulateTotals = default;

                var HierarchyList = new List<Hierarchy>();
                var row = new Hierarchy
                {
                    Currency = "EUR",
                    Process = 1,
                    CurrencyLink = null
                };

                HierarchyList.Add(row);
                CreateHierarchyList(rates, HierarchyList);

                acumulateTotals = RetrieveTotals(transactions, rates, totalTransactionList, HierarchyList, acumulateTotals);

                var totalsValues = new Total { Totalesporsku = totalTransactionList, TotalSkus = acumulateTotals };
                totals.Add(totalsValues);
                response.Message = "Successfully Processed";
                response.Result = totals;
                return response;
            }

            private static decimal RetrieveTotals(List<Transaction> transactions, List<Rates> rates, List<TotalTransactionList> totalTransactionList, List<Hierarchy> HierarchyList, decimal acumulateTotals)
            {
                foreach (var transaction in transactions)
                {
                    var totals = new TotalTransactionList();

                    if (transaction.Currency == HierarchyList[0].Currency)
                    {
                        totals.Id = transaction.Id;
                        totals.Sku = transaction.Sku;
                        totals.Amount = transaction.Amount;
                        totals.Currency = transaction.Currency;
                        totals.CurrencyChange = transaction.Currency;
                        totals.Convertion = Math.Round(decimal.Parse(transaction.Amount), 2, MidpointRounding.ToEven);
                        acumulateTotals += totals.Convertion;
                        totalTransactionList.Add(totals);
                        continue;
                    }

                    if (transaction.Currency == HierarchyList[1].Currency)
                    {
                        var operatorRate = decimal.Parse(rates.Where(x => x.From == HierarchyList[1].Currency && x.To == HierarchyList[0].Currency)
                                       .Select(x => x.Rate).FirstOrDefault());

                        var result = decimal.Parse(transaction.Amount) * operatorRate;
                        totals.Id = transaction.Id;
                        totals.Sku = transaction.Sku;
                        totals.Amount = transaction.Amount;
                        totals.Currency = transaction.Currency;
                        totals.CurrencyChange = "EUR";
                        totals.Convertion = Math.Round(result, 1, MidpointRounding.ToEven);
                        acumulateTotals += totals.Convertion;
                        totalTransactionList.Add(totals);
                        continue;
                    }

                    var count = HierarchyList.Where(x => x.Process == 2).Count();

                    if (count > 1)
                    {
                        var listacurrency = HierarchyList.Where(x => x.Process == 2).Select(x => x.Currency).ToList();

                        if (transaction.Currency == listacurrency[0] || transaction.Currency == listacurrency[1])
                        {
                            if (transaction.Currency == HierarchyList[2].Currency)
                            {
                                var operatorRate = decimal.Parse(rates.Where(x => x.From == HierarchyList[2].Currency && x.To == HierarchyList[0].Currency).Select(x => x.Rate).FirstOrDefault());
                                var result = decimal.Parse(transaction.Amount) * operatorRate;
                                totals.Id = transaction.Id;
                                totals.Sku = transaction.Sku;
                                totals.Amount = transaction.Amount;
                                totals.Currency = transaction.Currency;
                                totals.CurrencyChange = "EUR";
                                totals.Convertion = Math.Round(result, 1, MidpointRounding.ToEven);
                                acumulateTotals += totals.Convertion;
                                totalTransactionList.Add(totals);
                                continue;
                            }
                        }
                    }

                    if (transaction.Currency == HierarchyList[2].Currency)
                    {
                        var operatorRateList = new List<decimal>
                        {
                            decimal.Parse(rates.Where(x => x.From == HierarchyList[2].Currency && x.To == HierarchyList[1].Currency).Select(x => x.Rate).FirstOrDefault()),
                            decimal.Parse(rates.Where(x => x.From == HierarchyList[1].Currency && x.To == HierarchyList[0].Currency).Select(x => x.Rate).FirstOrDefault())
                        };

                        var result = decimal.Parse(transaction.Amount) * operatorRateList[0];
                        result *= operatorRateList[1];
                        totals.Id = transaction.Id;
                        totals.Sku = transaction.Sku;
                        totals.Amount = transaction.Amount;
                        totals.Currency = transaction.Currency;
                        totals.CurrencyChange = "EUR";
                        totals.Convertion = Math.Round(result, 1, MidpointRounding.ToEven);
                        acumulateTotals += totals.Convertion;
                        totalTransactionList.Add(totals);
                        continue;
                    }

                    if (transaction.Currency == HierarchyList[3].Currency)
                    {
                        var operatorRateList = new List<decimal>();
                        decimal result = default;
                        var currencylist = HierarchyList.Where(x => x.Process == 2).Count();

                        if (currencylist > 1)
                        {
                            operatorRateList.Add(decimal.Parse(rates.Where(x => x.From == HierarchyList[3].Currency && x.To == HierarchyList[3].CurrencyLink).Select(x => x.Rate).FirstOrDefault()));
                            var firstCondition = HierarchyList.Where(x => x.Currency == HierarchyList[3].CurrencyLink).Select(x => x.Currency).FirstOrDefault();
                            var secondCondition = HierarchyList.Where(x => x.Currency == HierarchyList[3].CurrencyLink).Select(x => x.CurrencyLink).FirstOrDefault();
                            operatorRateList.Add(decimal.Parse(rates.Where(x => x.From == firstCondition && x.To == secondCondition).Select(x => x.Rate).FirstOrDefault()));
                            result = decimal.Parse(transaction.Amount) * operatorRateList[0];
                            result *= operatorRateList[1];
                        }
                        else
                        {
                            operatorRateList.Add(decimal.Parse(rates.Where(x => x.From == HierarchyList[3].Currency && x.To == HierarchyList[2].Currency).Select(x => x.Rate).FirstOrDefault()));
                            operatorRateList.Add(decimal.Parse(rates.Where(x => x.From == HierarchyList[2].Currency && x.To == HierarchyList[1].Currency).Select(x => x.Rate).FirstOrDefault()));
                            operatorRateList.Add(decimal.Parse(rates.Where(x => x.From == HierarchyList[1].Currency && x.To == HierarchyList[0].Currency).Select(x => x.Rate).FirstOrDefault()));
                            result = decimal.Parse(transaction.Amount) * operatorRateList[0];
                            result *= operatorRateList[1];
                            result *= operatorRateList[2];
                        }

                        totals.Id = transaction.Id;
                        totals.Sku = transaction.Sku;
                        totals.Amount = transaction.Amount;
                        totals.Currency = transaction.Currency;
                        totals.CurrencyChange = "EUR";
                        totals.Convertion = Math.Round(result, 1, MidpointRounding.ToEven);
                        acumulateTotals += totals.Convertion;
                        totalTransactionList.Add(totals);
                        continue;
                    }
                }

                return acumulateTotals;
            }

            private static void CreateHierarchyList(List<Rates> rates, List<Hierarchy> hierarchyList)
            {
                int counter = hierarchyList.Count;

                for (int i = 0; i < counter; i++)
                {
                    var result = rates.Where(x => x.To.Equals(hierarchyList[i].Currency))
                                        .Select(x => new Hierarchy
                                        { 
                                            Currency = x.From,
                                            CurrencyLink = x.To,
                                            Process = hierarchyList[i].Process + 1 })
                                        .ToList();

                    if (result.Count > 0)
                    {
                        AddValuesHierarchyData(hierarchyList, result);
                    }
                }

                counter = hierarchyList.Count;

                for (int i = 1; i < counter; i++)
                {
                    var count = hierarchyList.Where(x => x.Process == 2).Count();
                    var result = new List<Hierarchy>();

                    if (count > 1)
                    {
                        if (i == 1)
                        {
                            result = rates.Where(x => x.To.Equals(hierarchyList[i].Currency)
                                                 && x.From != hierarchyList[0].Currency
                                                 && x.From != hierarchyList[2].Currency)
                                                .Select(x => new Hierarchy
                                                {
                                                    Currency = x.From,
                                                    CurrencyLink = x.To,
                                                    Process = hierarchyList[i].Process + 1
                                                })
                                                .ToList();
                        }

                        if (i == 2)
                        {
                            result = rates.Where(x => x.To.Equals(hierarchyList[i].Currency)
                                                 && x.From != hierarchyList[0].Currency
                                                 && x.From != hierarchyList[1].Currency)
                                                .Select(x => new Hierarchy
                                                {
                                                    Currency = x.From,
                                                    CurrencyLink = x.To,
                                                    Process = hierarchyList[i].Process + 1
                                                })
                                                .ToList();

                        }
                    }
                    else
                    {
                        result = rates.Where(x => x.To.Equals(hierarchyList[i].Currency)
                                               && x.From != hierarchyList[0].Currency
                                               && x.From != hierarchyList[1].Currency)
                                              .Select(x => new Hierarchy
                                              {
                                                  Currency = x.From,
                                                  CurrencyLink = x.To,
                                                  Process = hierarchyList[i].Process + 1
                                              })
                                              .ToList();
                    }


                    if (result.Count > 0)
                    {
                        AddValuesHierarchyData(hierarchyList, result);
                    }
                }

                counter = hierarchyList.Count;

                for (int i = 2; i < counter; i++)
                {
                    var result = rates.Where(x => x.To.Equals(hierarchyList[i].Currency)
                                             && x.From != hierarchyList[0].Currency
                                             && x.From != hierarchyList[1].Currency
                                             && x.From != hierarchyList[2].Currency)
                                            .Select(x => new Hierarchy
                                            {
                                                Currency = x.From,
                                                CurrencyLink = x.To,
                                                Process = hierarchyList[i].Process + 1
                                            })
                                            .ToList();

                    if (result.Count > 0)
                    {
                        AddValuesHierarchyData(hierarchyList, result);
                    }
                }
            }

            private static void AddValuesHierarchyData(List<Hierarchy> hierarchyList, List<Hierarchy> Results)
            {
                foreach (var result in Results)
                {
                    hierarchyList.Add(result);
                }
            }
        }
    }
}
