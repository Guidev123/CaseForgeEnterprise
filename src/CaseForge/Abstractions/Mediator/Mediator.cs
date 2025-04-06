namespace CaseForge.Abstractions.Mediator;

public class Mediator(Func<Type, object> serviceResolver, IDictionary<Type, Type> handlerDatails) : IMediator
{
    private readonly Func<Type, object> _serviceResolver = serviceResolver;
    private readonly IDictionary<Type, Type> _handlerDatails = handlerDatails;

    public async Task<TResponse> DispatchAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        var requestType = request.GetType();
        if (!_handlerDatails.ContainsKey(requestType))
            throw new InvalidOperationException($"No handler registered for request type {requestType}");

        _handlerDatails.TryGetValue(requestType, out var requestHandlerType);
        if (requestHandlerType is null)
            throw new InvalidOperationException($"No handler registered for request type {requestType}");

        var handler = _serviceResolver(requestHandlerType);
        return await (Task<TResponse>)handler.GetType().GetMethod("ExecuteAsync")!.Invoke(handler, [request, cancellationToken])!;
    }
}
