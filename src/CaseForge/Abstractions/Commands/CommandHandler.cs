using CaseForge.Abstractions.Mediator;
using CaseForge.Notifications;
using CaseForge.Responses;

namespace CaseForge.Abstractions.Commands;

public abstract class CommandHandler<TCommand, TResult>(INotificator notificator) : Handler(notificator), IRequestHandler<TCommand, Response<TResult>>
                      where TCommand : IRequest<Response<TResult>>

{
    public abstract Task<Response<TResult>> ExecuteAsync(TCommand request, CancellationToken cancellationToken);
}