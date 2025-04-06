using CaseForge.Abstractions.Mediator;
using CaseForge.Notifications;
using CaseForge.Responses;

namespace CaseForge.Abstractions.Queries;

public abstract class PagedQueryHandler<TQuery, TResult>(INotificator notificator)
                    : Handler(notificator), IRequestHandler<TQuery, PagedResponse<TResult>>
                      where TQuery : IRequest<PagedResponse<TResult>>
{
    public abstract Task<PagedResponse<TResult>> ExecuteAsync(TQuery request, CancellationToken cancellationToken);
}