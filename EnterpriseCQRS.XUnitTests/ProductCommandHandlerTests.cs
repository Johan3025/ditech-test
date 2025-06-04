using EnterpriseCQRS.Data;
using EnterpriseCQRS.Domain.Commands.ProductCommand;
using Microsoft.Extensions.Logging;
using Moq;
using EnterpriseCQRS.Data.Model;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using static EnterpriseCQRS.Services.CommandHandlers.ProductCommandHandler.ProductCommandHandler;

namespace EnterpriseCQRS.XUnitTests
{
    public class ProductCommandHandlerTests : SqLiteDbFake
    {
        private readonly CommittedCapacityContext _context;
        private readonly Mock<ILogger<GetTransactionCommandHandler>> _mockLogger;
        private readonly Mock<ILogger<GetRateCommandHandler>> _mockRateLogger;

        public ProductCommandHandlerTests()
        {
            _context = GetDbContext();
            _mockLogger = new Mock<ILogger<GetTransactionCommandHandler>>();
            _mockRateLogger = new Mock<ILogger<GetRateCommandHandler>>();
        }

        [Fact]
        public async Task IsNotNullResponse_GetTransactions()
        {
            var request = new GetTransactionCommand();

            var handle = new GetTransactionCommandHandler(_context, _mockLogger.Object);

            var resultResponseMessage = "Guardado exitoso";

            var responses = await handle.Handle(request, new CancellationToken());

            Assert.Contains(resultResponseMessage, responses.Message);
        }

        [Fact]
        public async Task IsNotNullResponse_GetRates()
        {
            var request = new GetRateCommand();

            var handle = new GetRateCommandHandler(_context, _mockRateLogger.Object);

            var resultResponseMessage = "Guardado exitoso";

            var responses = await handle.Handle(request, new CancellationToken());

            Assert.Equal(resultResponseMessage, responses.Message);
        }

        [Fact]
        public async Task CalculateTransactions_MissingRate_DoesNotThrow()
        {
            var transaction = new Transaction { Sku = "SKU1", Amount = "10", Currency = "USD" };
            _context.Transaction.Add(transaction);
            _context.Rates.Add(new Rates { From = "USD", To = "EUR", Rate = "invalid" });
            await _context.SaveChangesAsync();

            var command = new CalculateTransactionCommand { Sku = transaction.Sku };
            var handler = new CalculateTransactionCommandHandler(_context);

            var response = await handler.Handle(command, new CancellationToken());

            Assert.NotNull(response);
            Assert.NotNull(response.Result);
        }
    }
}
