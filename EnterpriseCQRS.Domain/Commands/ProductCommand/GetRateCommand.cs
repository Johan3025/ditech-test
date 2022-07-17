using EnterpriseCQRS.Data.Model;
using EnterpriseCQRS.Domain.Responses;
using System.Collections.Generic;

namespace EnterpriseCQRS.Domain.Commands.ProductCommand
{
    public class GetRateCommand : BaseCommand<GenericResponse<IList<Rates>>>
    {
    }
}
