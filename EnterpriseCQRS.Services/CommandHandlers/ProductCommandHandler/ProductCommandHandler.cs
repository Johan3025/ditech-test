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

                _context.Database.ExecuteSqlRaw("DELETE FROM [Transaction]");
                var responses = await rates.test(url);
                await _context.Rates.AddRangeAsync(responses.Result, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                response.Message = "Guardado exitoso";
                return response;
            }
        }

        public class CalculateTransactionCommandHandler : IRequestHandler<CalculateTransactionCommand, GenericResponse<IList<string>>>
        {
            private readonly CommittedCapacityContext _context;

            public CalculateTransactionCommandHandler(CommittedCapacityContext context)
            {
                _context = context;
            }

            public async Task<GenericResponse<IList<string>>> Handle(CalculateTransactionCommand request, CancellationToken cancellationToken)
            {

                var response = new GenericResponse<IList<string>>();
                var transactions = await _context.Transaction.Where(x => x.Sku.Equals("Y8417")).Take(12).ToListAsync();
                var rates = await _context.Rates.ToListAsync();
                var lista = new List<string>();
                var rate = new List<Rates>();
                double acumulador = default;

                var listjerarquia = new List<Hierarchy>();

                var hierarchyOne = new Hierarchy
                {
                    Currency = "EUR",
                    Rate = "0"
                };

                listjerarquia.Add(hierarchyOne);

                var hierarchyTwo = new Hierarchy
                {
                    Currency = rates.Where(x => x.From.Equals(listjerarquia[0].Currency)).Select(x => x.To).FirstOrDefault()
                };

                hierarchyTwo.Rate = rates.Where(x => x.From.Equals(hierarchyTwo.Currency) && x.To.Equals(listjerarquia[0].Currency)).Select(x => x.Rate).FirstOrDefault();


                listjerarquia.Add(hierarchyTwo);

                var hierarchyThree = new Hierarchy
                {
                    Currency = rates.Where(x => x.From.Equals(listjerarquia[1].Currency) && !x.To.Equals(listjerarquia[0])).Select(x => x.To).FirstOrDefault()
                };

                hierarchyThree.Rate = rates.Where(x => x.From.Equals(hierarchyThree.Currency) && x.To.Equals(listjerarquia[1].Currency)).Select(x => x.Rate).FirstOrDefault();

                listjerarquia.Add(hierarchyThree);

                var hierarchyFour = new Hierarchy
                {
                    Currency = rates.Where(x => x.From.Equals(listjerarquia[2].Currency) && !x.To.Equals(listjerarquia[1])).Select(x => x.To).FirstOrDefault()
                };

                hierarchyFour.Rate = rates.Where(x => x.From.Equals(hierarchyFour.Currency) && x.To.Equals(listjerarquia[2].Currency)).Select(x => x.Rate).FirstOrDefault();

                listjerarquia.Add(hierarchyFour);

                //listjerarquia.Add(rates.Where(x => x.From.Equals(listjerarquia[2]) && !x.To.Equals(listjerarquia[1])).Select(x => x.To).FirstOrDefault());
                //listjerarquia.Add(rates.Where(x => x.From.Equals(listjerarquia[3]) && !x.To.Equals(listjerarquia[2])).Select(x => x.To).FirstOrDefault());


                foreach (var transaction in transactions)
                {
                    //if (transaction.Currency == listjerarquia[0])
                    //{
                    //    lista.Add(transaction.Amount);
                    //    continue;
                    //}

                    if (transaction.Currency == "AUD")
                    {
                        rate = rates.Where(x => x.From.Equals(transaction.Currency)).ToList();
                        testing(lista, transaction, rate, acumulador);

                        rate = rates.Where(x => x.To.Equals(rate.FirstOrDefault().To)).ToList();
                        testing(lista, transaction, rate, acumulador);
                    }
                }

                response.Message = "Guardado exitoso";
                return response;
            }

            private void testing(List<string> lista, Transaction transaction, List<Rates> rate, double acumulador)
            {
                foreach (var item in rate)
                {
                    if (item.To == "EUR")
                    {
                        lista.Add((int.Parse(transaction.Amount) * int.Parse(item.Rate)).ToString());

                    }
                    else
                    {
                        acumulador += double.Parse(item.Rate);
                    }
                }
            }
        }
    }
}
