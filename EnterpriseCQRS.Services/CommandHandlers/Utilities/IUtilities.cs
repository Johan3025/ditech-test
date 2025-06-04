namespace EnterpriseCQRS.Services.CommandHandlers.Utilities
{
    using EnterpriseCQRS.Domain.Responses;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IUtilities<T> where T : class
    {
        Task<GenericResponse<IList<T>>> ExternalServiceUtility(Uri url);
    }
}
