using MediatR;

namespace EnterpriseCQRS.Domain.Commands
{
    public abstract class BaseCommand<TResult> : IRequest<TResult>
    {
    }
} 
