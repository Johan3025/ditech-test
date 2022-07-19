using EnterpriseCQRS.Data;
using EnterpriseCQRS.Data.Model;
using EnterpriseCQRS.Domain.Commands.ProductCommand;
using EnterpriseCQRS.Domain.Responses;
using EnterpriseCQRS.Services.CommandHandlers.Utilities;
using MediatR;
using Microsoft.EntityFrameworkCore;
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

            public GetTransactionCommandHandler(CommittedCapacityContext context)
            {
                _context = context;
            }

            public async Task<GenericResponse<IList<Transaction>>> Handle(GetTransactionCommand request, CancellationToken cancellationToken)
            {
                var url = new Uri("http://quiet-stone-2094.herokuapp.com/transactions.json");
                var response = new GenericResponse<IList<Transaction>>();
                var transactions = new Utilities<Transaction>();

                _context.Database.ExecuteSqlRaw("DELETE FROM [Transaction]");
                var responses = await transactions.test(url);
                await _context.Transaction.AddRangeAsync(responses.Result, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                response.Message = "Guardado exitoso";
                //response.Result = responses.Result;
                return response;
            }
        }

        public class GetRateCommandHandler : IRequestHandler<GetRateCommand, GenericResponse<IList<Rates>>>
        {
            private readonly CommittedCapacityContext _context;

            public GetRateCommandHandler(CommittedCapacityContext context)
            {
                _context = context;
            }

            public async Task<GenericResponse<IList<Rates>>> Handle(GetRateCommand request, CancellationToken cancellationToken)
            {
                var url = new Uri("http://quiet-stone-2094.herokuapp.com/rates.json");
                var response = new GenericResponse<IList<Rates>>();
                var rates = new Utilities<Rates>();

                _context.Database.ExecuteSqlRaw("DELETE FROM [Rates]");
                var responses = await rates.test(url);
                await _context.Rates.AddRangeAsync(responses.Result, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                response.Message = "Guardado exitoso";
                //response.Result = responses.Result;
                return response;
            }
        }

        public class CalculateTransactionCommandHandler : IRequestHandler<CalculateTransactionCommand, GenericResponse<IList<test>>>
        {
            private readonly CommittedCapacityContext _context;

            public CalculateTransactionCommandHandler(CommittedCapacityContext context)
            {
                _context = context;
            }

            public async Task<GenericResponse<IList<test>>> Handle(CalculateTransactionCommand request, CancellationToken cancellationToken)
            {
                var response = new GenericResponse<IList<test>>();
                var transactions = await _context.Transaction.Where(x => x.Sku.Equals("C4740")).ToListAsync();
                var rates = await _context.Rates.ToListAsync();
                var lista = new List<test>();
                var rate = new List<Rates>();
                decimal acumulador = default;

                var listjerarquia = new List<Hierarchy>();
                var row = new Hierarchy
                {
                    Currency = "EUR",
                    Process = 1,
                    CurrencyLink = null
                };

                listjerarquia.Add(row);

                CreateHierarchyList(rates, listjerarquia);

                RetrieveTotals(transactions, rates, lista, listjerarquia, acumulador);

                response.Message = "Guardado exitoso";
                response.Result = lista;
                return response;
            }

            private static void RetrieveTotals(List<Transaction> transactions, List<Rates> rates, List<test> lista, List<Hierarchy> listjerarquia, decimal acumulador)
            {
                foreach (var transaction in transactions)
                {
                    var totales = new test();

                    if (transaction.Currency == listjerarquia[0].Currency)
                    {
                        totales.Id = transaction.Id;
                        totales.Sku = transaction.Sku;
                        totales.Amount = transaction.Amount;
                        totales.Currency = transaction.Currency;
                        totales.CurrencyChange = transaction.Currency;
                        totales.Convertion = Math.Round(decimal.Parse(transaction.Amount), 2, MidpointRounding.ToEven);
                        acumulador += totales.Convertion;
                        lista.Add(totales);
                        continue;
                    }

                    if (transaction.Currency == listjerarquia[1].Currency)
                    {
                        var operador = decimal.Parse(rates.Where(x => x.From == listjerarquia[1].Currency && x.To == listjerarquia[0].Currency).Select(x => x.Rate).FirstOrDefault());
                        var resultado = decimal.Parse(transaction.Amount) * operador;
                        totales.Id = transaction.Id;
                        totales.Sku = transaction.Sku;
                        totales.Amount = transaction.Amount;
                        totales.Currency = transaction.Currency;
                        totales.CurrencyChange = "EUR";
                        totales.Convertion = Math.Round(resultado, 1, MidpointRounding.ToEven);
                        acumulador += totales.Convertion;
                        lista.Add(totales);
                        continue;
                    }

                    var count = listjerarquia.Where(x => x.Process == 2).Count();

                    if (count > 1)
                    {
                        var listacurrency = listjerarquia.Where(x => x.Process == 2).Select(x => x.Currency).ToList();

                        if (transaction.Currency == listacurrency[0] || transaction.Currency == listacurrency[1])
                        {
                            if (transaction.Currency == listjerarquia[2].Currency)
                            {
                                var operador = decimal.Parse(rates.Where(x => x.From == listjerarquia[2].Currency && x.To == listjerarquia[0].Currency).Select(x => x.Rate).FirstOrDefault());
                                var resultado = decimal.Parse(transaction.Amount) * operador;
                                totales.Id = transaction.Id;
                                totales.Sku = transaction.Sku;
                                totales.Amount = transaction.Amount;
                                totales.Currency = transaction.Currency;
                                totales.CurrencyChange = "EUR";
                                totales.Convertion = Math.Round(resultado, 1, MidpointRounding.ToEven);
                                acumulador += totales.Convertion;
                                lista.Add(totales);
                                continue;
                            }
                        }
                    }

                    if (transaction.Currency == listjerarquia[2].Currency)
                    {
                        var listaOperador = new List<decimal>();
                        listaOperador.Add(decimal.Parse(rates.Where(x => x.From == listjerarquia[2].Currency && x.To == listjerarquia[1].Currency).Select(x => x.Rate).FirstOrDefault()));
                        listaOperador.Add(decimal.Parse(rates.Where(x => x.From == listjerarquia[1].Currency && x.To == listjerarquia[0].Currency).Select(x => x.Rate).FirstOrDefault()));
                        var resultado = decimal.Parse(transaction.Amount) * listaOperador[0];
                        resultado *= listaOperador[1];
                        totales.Id = transaction.Id;
                        totales.Sku = transaction.Sku;
                        totales.Amount = transaction.Amount;
                        totales.Currency = transaction.Currency;
                        totales.CurrencyChange = "EUR";
                        totales.Convertion = Math.Round(resultado, 1, MidpointRounding.ToEven);
                        acumulador += totales.Convertion;
                        lista.Add(totales);
                        continue;
                    }

                    if (transaction.Currency == listjerarquia[3].Currency)
                    {
                        var listaOperador = new List<decimal>();
                        decimal resultado = default;
                        var listacurrency = listjerarquia.Where(x => x.Process == 2).Count();

                        if (listacurrency > 1 )
                        {
                            listaOperador.Add(decimal.Parse(rates.Where(x => x.From == listjerarquia[3].Currency && x.To == listjerarquia[3].CurrencyLink).Select(x => x.Rate).FirstOrDefault()));
                            var tostis = listjerarquia.Where(x => x.Currency == listjerarquia[3].CurrencyLink).Select(x => x.Currency).FirstOrDefault();
                            var tostis2 = listjerarquia.Where(x => x.Currency == listjerarquia[3].CurrencyLink).Select(x => x.CurrencyLink).FirstOrDefault();
                            listaOperador.Add(decimal.Parse(rates.Where(x => x.From == tostis && x.To == tostis2).Select(x => x.Rate).FirstOrDefault()));
                            resultado = decimal.Parse(transaction.Amount) * listaOperador[0];
                            resultado *= listaOperador[1];
                        }
                        else
                        {
                            listaOperador.Add(decimal.Parse(rates.Where(x => x.From == listjerarquia[3].Currency && x.To == listjerarquia[2].Currency).Select(x => x.Rate).FirstOrDefault()));
                            listaOperador.Add(decimal.Parse(rates.Where(x => x.From == listjerarquia[2].Currency && x.To == listjerarquia[1].Currency).Select(x => x.Rate).FirstOrDefault()));
                            listaOperador.Add(decimal.Parse(rates.Where(x => x.From == listjerarquia[1].Currency && x.To == listjerarquia[0].Currency).Select(x => x.Rate).FirstOrDefault()));
                            resultado = decimal.Parse(transaction.Amount) * listaOperador[0];
                            resultado *= listaOperador[1];
                            resultado *= listaOperador[2];
                        }

                        totales.Id = transaction.Id;
                        totales.Sku = transaction.Sku;
                        totales.Amount = transaction.Amount;
                        totales.Currency = transaction.Currency;
                        totales.CurrencyChange = "EUR";
                        totales.Convertion = Math.Round(resultado, 1, MidpointRounding.ToEven);
                        acumulador += totales.Convertion;
                        lista.Add(totales);
                        continue;
                    }
                }
            }

            private static void CreateHierarchyList(List<Rates> rates, List<Hierarchy> listjerarquia)
            {
                int contador = listjerarquia.Count();

                for (int i = 0; i < contador; i++)
                {
                    var result = rates.Where(x => x.To.Equals(listjerarquia[i].Currency))
                                        .Select(x => new Hierarchy { Currency = x.From, CurrencyLink = x.To, Process = listjerarquia[i].Process + 1 }).ToList();

                    if (result.Count > 0)
                    {

                        NewMethod(listjerarquia, result);
                    }
                }


                contador = listjerarquia.Count();

                for (int i = 1; i < contador; i++)
                {
                    var count = listjerarquia.Where(x => x.Process == 2).Count();
                    var result = new List<Hierarchy>();

                    if (i == 1)
                    {
                        result = rates.Where(x => x.To.Equals(listjerarquia[i].Currency)
                                             && x.From != listjerarquia[0].Currency
                                             && x.From != listjerarquia[2].Currency)
                                            .Select(x => new Hierarchy
                                            {
                                                Currency = x.From,
                                                CurrencyLink = x.To,
                                                Process = listjerarquia[i].Process + 1
                                            })
                                            .ToList();

                    }

                    if (i == 2)
                    {
                        result = rates.Where(x => x.To.Equals(listjerarquia[i].Currency)
                                             && x.From != listjerarquia[0].Currency
                                             && x.From != listjerarquia[1].Currency)
                                            .Select(x => new Hierarchy
                                            {
                                                Currency = x.From,
                                                CurrencyLink = x.To,
                                                Process = listjerarquia[i].Process + 1
                                            })
                                            .ToList();

                    }


                    if (result.Count > 0)
                    {
                        NewMethod(listjerarquia, result);
                    }
                }

                contador = listjerarquia.Count();

                for (int i = 2; i < contador; i++)
                {
                    var result = rates.Where(x => x.To.Equals(listjerarquia[i].Currency)
                                             && x.From != listjerarquia[0].Currency
                                             && x.From != listjerarquia[1].Currency
                                             && x.From != listjerarquia[2].Currency)
                                            .Select(x => new Hierarchy
                                            {
                                                Currency = x.From,
                                                CurrencyLink = x.To,
                                                Process = listjerarquia[i].Process + 1
                                            })
                                            .ToList();

                    if (result.Count > 0)
                    {
                        NewMethod(listjerarquia, result);
                    }
                }
            }

            private static void NewMethod(List<Hierarchy> listjerarquia, List<Hierarchy> second)
            {
                foreach (var item in second)
                {
                    listjerarquia.Add(item);
                }
            }
        }
    }
}
