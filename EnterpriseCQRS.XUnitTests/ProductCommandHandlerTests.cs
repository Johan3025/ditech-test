using EnterpriseCQRS.Data;
using EnterpriseCQRS.Data.Model;
using EnterpriseCQRS.Domain.Commands.ProductCommand;
using EnterpriseCQRS.Domain.Responses;
using EnterpriseCQRS.Services.CommandHandlers.ProductCommandHandler;
using MediatR;
using Moq;
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
        private readonly Mock<ProductCommandHandler> _mockGetTransaction;
        private readonly Mock<GetTransactionCommandHandler> _mockGetTransactionHandle;
        private IMediator _mediator { get; }

        public ProductCommandHandlerTests()
        {
            _context = GetDbContext();
            _mockGetTransaction = new Mock<ProductCommandHandler>();
            _mockGetTransactionHandle = new Mock<GetTransactionCommandHandler>();
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
