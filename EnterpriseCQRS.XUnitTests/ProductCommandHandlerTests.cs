using EnterpriseCQRS.Data;
using EnterpriseCQRS.Data.Model;
using EnterpriseCQRS.Domain.Commands.ProductCommand;
using EnterpriseCQRS.Domain.Responses;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using static EnterpriseCQRS.Services.CommandHandlers.ProductCommandHandler.ProductCommandHandler;

namespace EnterpriseCQRS.XUnitTests
{
    public class ProductCommandHandlerTests : SqLiteDbFake
    {
        private readonly CommittedCapacityContext _context;

        public ProductCommandHandlerTests()
        {
            _context = GetDbContext();
        }

        [Fact]
        public async Task IsNotNullResponse_GetTransactions()
        {
            GetTransactionCommand request = new GetTransactionCommand();

            GetTransactionCommandHandler handle = new GetTransactionCommandHandler(_context);

            var resultResponseMessage = "Guardado exitoso";

            GenericResponse<IList<Transaction>> responses = await handle.Handle(request, new CancellationToken());

            Assert.Contains(resultResponseMessage, responses.Message);
        }

        [Fact]
        public async Task IsNotNullResponse_GetRates()
        {
            GetRateCommand request = new GetRateCommand();

            GetRateCommandHandler handle = new GetRateCommandHandler(_context);

            var resultResponseMessage = "Guardado exitoso";

            GenericResponse<IList<Rates>> responses = await handle.Handle(request, new CancellationToken());

            Assert.Equal(resultResponseMessage, responses.Message);
        }
    }
}
