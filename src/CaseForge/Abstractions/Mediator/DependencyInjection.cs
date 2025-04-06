using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;

namespace CaseForge.Abstractions.Mediator;

public static class DependencyInjection
{
    public static void AddMediator(this IServiceCollection services, params Type[] markers)
    {
        var serviceLifeTime = ServiceLifetime.Scoped;
        var handlerInfo = new Dictionary<Type, Type>();

        foreach (var marker in markers)
        {
            var assembly = marker.Assembly;
            var requests = GetClassesImplementingInterface(assembly, typeof(IRequest<>));
            var handlers = GetClassesImplementingInterface(assembly, typeof(IRequestHandler<,>));

            foreach (var request in requests)
            {
                var handler = handlers.FirstOrDefault(h =>
                    h.GetInterfaces().Any(i => i.IsGenericType &&
                        i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>) &&
                        i.GetGenericArguments()[0] == request))
                    ?? throw new InvalidOperationException($"No handler found for request {request.Name}");
                handlerInfo[request] = handler;
                services.TryAdd(new ServiceDescriptor(handler, handler, serviceLifeTime));
            }
        }

        services.AddScoped<IMediator>(x => new Mediator(t => x.GetRequiredService(t), handlerInfo));
    }

    private static List<Type> GetClassesImplementingInterface(Assembly assembly, Type typeToMatch)
    {
         return [.. assembly.ExportedTypes
            .Where(type =>
            {
                var genericInterfaceTypes = type.GetInterfaces().Where(x => x.IsGenericType).ToList();
                var implementRequestType = genericInterfaceTypes.Any(x => x.GetGenericTypeDefinition() == typeToMatch);

                return !type.IsInterface && !type.IsAbstract && implementRequestType;
            })];
    }
}
