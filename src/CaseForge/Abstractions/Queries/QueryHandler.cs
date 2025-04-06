using CaseForge.Abstractions.Mediator;
using CaseForge.Notifications;
using CaseForge.Responses;

namespace CaseForge.Abstractions.Queries;

public abstract class QueryHandler<TQuery, TResult>(INotificator notificator)
                    : Handler(notificator),
                      IRequestHandler<TQuery, Response<TResult>>
                      where TQuery : IRequest<Response<TResult>>
{
    public abstract Task<Response<TResult>> ExecuteAsync(TQuery request, CancellationToken cancellationToken);
}
