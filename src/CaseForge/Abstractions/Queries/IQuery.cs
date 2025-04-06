using CaseForge.Abstractions.Mediator;
using CaseForge.Responses;

namespace CaseForge.Abstractions.Queries;

public interface IQuery<TResult> : IRequest<Response<TResult>>
{   }
