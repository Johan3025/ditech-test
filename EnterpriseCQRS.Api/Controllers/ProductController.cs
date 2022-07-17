using EnterpriseCQRS.Domain.Commands.ProductCommand;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace EnterpriseCQRS.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        public IMediator _mediator { get; }

        public ProductController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [Route("GetTransaction")]
        public async Task<IActionResult> GetTransaction([FromQuery] GetTransactionCommand query)
        {
            var response = await _mediator.Send(query);

            if (response is null)
            {
                return NotFound();
            }

            return response.Error is null ? Ok(response) : StatusCode(StatusCodes.Status500InternalServerError, response);
        }

        [HttpGet]
        [Route("GetRate")]
        public async Task<IActionResult> GetRate([FromQuery] GetRateCommand query)
        {
            var response = await _mediator.Send(query);

            if (response is null)
            {
                return NotFound();
            }

            return response.Error is null ? Ok(response) : StatusCode(StatusCodes.Status500InternalServerError, response);
        }

        [HttpPost]
        [Route("CalculateTransactions")]
        public async Task<IActionResult> CalculateTransactions([FromForm] CalculateTransactionCommand query)
        {
            var response = await _mediator.Send(query);

            if (response is null)
            {
                return NotFound();
            }

            return response.Error is null ? Ok(response) : StatusCode(StatusCodes.Status500InternalServerError, response);
        }
    }
}
