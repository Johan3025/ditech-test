using EnterpriseCQRS.Data;
using EnterpriseCQRS.Data.Model;
using EnterpriseCQRS.Domain.Commands.ProductCommand;
using Microsoft.Extensions.Logging;
using Moq;
using EnterpriseCQRS.Domain.Responses;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Xunit;
using static EnterpriseCQRS.Services.CommandHandlers.ProductCommandHandler.ProductCommandHandler;
using EnterpriseCQRS.Services.CommandHandlers.Utilities;

namespace EnterpriseCQRS.XUnitTests
{
    public class ProductCommandHandlerTests : SqLiteDbFake
    {
        private readonly CommittedCapacityContext _context;
        private readonly Mock<ILogger<GetTransactionCommandHandler>> _mockLogger;
        private readonly Mock<ILogger<GetRateCommandHandler>> _mockRateLogger;
        private readonly Mock<IUtilities<Transaction>> _mockService;
        private readonly Mock<IUtilities<Rates>> _mockRateService;

        public ProductCommandHandlerTests()
        {
            _context = GetDbContext();
            _mockLogger = new Mock<ILogger<GetTransactionCommandHandler>>();
            _mockRateLogger = new Mock<ILogger<GetRateCommandHandler>>();
            _mockService = new Mock<IUtilities<Transaction>>();
            _mockRateService = new Mock<IUtilities<Rates>>();
        }

        [Fact]
        public async Task IsNotNullResponse_GetTransactions()
        {
            var request = new GetTransactionCommand();

            var transactionsList = new List<Transaction>
            {
                new Transaction { Sku = "T1", Amount = "10", Currency = "USD" },
                new Transaction { Sku = "T2", Amount = "5", Currency = "EUR" }
            };

            _mockService
                .Setup(x => x.ExternalServiceUtility(It.IsAny<Uri>()))
                .ReturnsAsync(new GenericResponse<IList<Transaction>> { Result = transactionsList });

            var handle = new GetTransactionCommandHandler(_context, _mockLogger.Object, _mockService.Object);

            var resultResponseMessage = "Guardado exitoso";

            var responses = await handle.Handle(request, new CancellationToken());

            Assert.Contains(resultResponseMessage, responses.Message);
            Assert.Equal(2, await _context.Transaction.CountAsync());
        }

        [Fact]
        public async Task IsNotNullResponse_GetRates()
        {
            var request = new GetRateCommand();

            var rateList = new List<Rates>
            {
                new Rates { From = "USD", To = "EUR", Rate = "0.5" },
                new Rates { From = "EUR", To = "USD", Rate = "2" }
            };

            _mockRateService
                .Setup(x => x.ExternalServiceUtility(It.IsAny<Uri>()))
                .ReturnsAsync(new GenericResponse<IList<Rates>> { Result = rateList });

            var handle = new GetRateCommandHandler(_context, _mockRateLogger.Object, _mockRateService.Object);

            var resultResponseMessage = "Guardado exitoso";

            var responses = await handle.Handle(request, new CancellationToken());

            Assert.Equal(resultResponseMessage, responses.Message);
            Assert.Equal(2, await _context.Rates.CountAsync());
        }
    }
}
