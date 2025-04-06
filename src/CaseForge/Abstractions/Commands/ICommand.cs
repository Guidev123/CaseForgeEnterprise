using CaseForge.Abstractions.Mediator;
using CaseForge.Responses;

namespace CaseForge.Abstractions.Commands;

public interface ICommand<TResult> : IRequest<Response<TResult>>
{
    Guid Id { get; }
}