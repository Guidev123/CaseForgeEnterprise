using CaseForge.Abstractions.Mediator;
using CaseForge.Responses;

namespace CaseForge.Abstractions.Queries;

public interface IPagedQuery<TResult> : IRequest<PagedResponse<TResult>>
{   }