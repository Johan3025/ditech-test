using EnterpriseCQRS.Data.Model;
using EnterpriseCQRS.Domain.Responses;
using System.Collections.Generic;

namespace EnterpriseCQRS.Domain.Commands.ProductCommand
{

    public class GetTransactionCommand : BaseCommand<GenericResponse<IList<Transaction>>>
    {
    }
}
