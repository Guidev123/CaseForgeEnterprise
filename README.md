# CaseForge - Documentation

Welcome to the documentation for **CaseForge**, a library designed to implement use cases following the Mediator pattern (without relying on the MediatR library), with support for the Result pattern, Notification pattern, and validations using the **FluentValidation** library. This library aims to simplify the creation of commands, queries, and paged queries in a clean and structured way.

Below are practical examples of how to use the library's main features.

---

## Initial Setup

To start using **CaseForge**, download the NuGet package and add the following lines to your application's `Program.cs` file:

```powershell
dotnet add package CaseForge
```

```csharp
// Service registration
builder.Services.AddMediator(typeof(Program));
builder.Services.AddScoped<INotificator, Notificator>();
```

<li>AddMediator: Registers all commands, queries, and handlers located in the assembly of the provided class (in this case, Program). Simply pass a class that resides in the same assembly as your commands and queries.</li>
<li>INotificator: A service responsible for managing notifications/errors using the Notification pattern.</li>

## Using Commands

Commands represent actions that modify the system's state, such as creating or updating something. Here's a functional example of creating an order: 

### Code Example

```csharp
// Command Definition
public record CreateOrderCommand : Command<CreateOrderResponse>
{
    public CreateOrderCommand(Guid customerId, decimal totalAmount)
    {
        CustomerId = customerId;
        TotalAmount = totalAmount;
    }

    public Guid CustomerId { get; private set; }
    public decimal TotalAmount { get; private set; }
}

// Command Response
public record CreateOrderResponse(Guid OrderId);

// Validation with FluentValidation
public sealed class CreateOrderValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("Customer ID cannot be empty.");

        RuleFor(x => x.TotalAmount)
            .GreaterThan(0)
            .WithMessage("Total amount must be greater than zero.");
    }
}

// Command Handler
public sealed class CreateOrderHandler : CommandHandler<CreateOrderCommand, CreateOrderResponse>
{
    private readonly IOrderRepository _orderRepository;

    public CreateOrderHandler(INotificator notificator, IOrderRepository orderRepository)
        : base(notificator)
    {
        _orderRepository = orderRepository;
    }

    public override async Task<Response<CreateOrderResponse>> ExecuteAsync(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        // Execute validation
        if (!ExecuteValidation(new CreateOrderValidator(), request))
            return Response<CreateOrderResponse>.Failure(GetNotifications());

        // Create the order in the repository
        var order = await _orderRepository.CreateOrderAsync(request.MapToOrder());
        if (!order.IsSuccess)
        {
            Notify("Failed to create the order.");
            return Response<CreateOrderResponse>.Failure(GetNotifications());
        }

        // Return success with the response
        return Response<CreateOrderResponse>.Success(new(order.Id));
    }
}

```

<li>Command: Defines the data required for the action (CreateOrderCommand).</li>
<li>Response: Represents the operation's result (CreateOrderResponse).</li>
<li>Validator: Validates input data using FluentValidation</li>
<li>Handler: Contains the execution logic, including validation and result handling with the Result pattern.</li>

## Using Queries

Queries are used to retrieve data without altering the system's state. Here's an example of fetching an order by ID:

### Code Example
```csharp
// Query Definition
public record GetOrderQuery : IQuery<GetOrderResponse>
{
    public Guid OrderId { get; private set; }

    public GetOrderQuery(Guid orderId)
    {
        OrderId = orderId;
    }
}

// Query Response
public record GetOrderResponse(Guid OrderId, string Description);

// Query Handler
public sealed class GetOrderHandler : QueryHandler<GetOrderQuery, GetOrderResponse>
{
    private readonly IOrderRepository _orderRepository;

    public GetOrderHandler(INotificator notificator, IOrderRepository orderRepository)
        : base(notificator)
    {
        _orderRepository = orderRepository;
    }

    public override async Task<Response<GetOrderResponse>> ExecuteAsync(GetOrderQuery request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId);
        if (order is null)
        {
            Notify("Order not found.");
            return Response<GetOrderResponse>.Failure(GetNotifications(), code: 404);
        }

        return Response<GetOrderResponse>.Success(new(order.Id, order.Description));
    }
}
```
<li>Query: Defines the query parameters (GetOrderQuery).</li>
<li>Response: Contains the returned data (GetOrderResponse).</li>
<li>Handler: Executes the query logic and returns the result using the Result pattern.</li>

## Using Paged Queries

Paged Queries are useful for queries that return paginated lists of data. Here's an example of listing a customer's orders:

### Code Example
```csharp
// Paged Query Definition
public record GetAllOrdersQuery : IPagedQuery<GetAllOrdersResponse>
{
    public Guid CustomerId { get; private set; }
    public int PageNumber { get; private set; }
    public int PageSize { get; private set; }

    public GetAllOrdersQuery(Guid customerId, int pageNumber, int pageSize)
    {
        CustomerId = customerId;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }
}

// Paged Query Response
public record GetAllOrdersResponse(List<OrderDTO> Orders);

// Paged Query Handler
public sealed class GetAllOrdersHandler : PagedQueryHandler<GetAllOrdersQuery, GetAllOrdersResponse>
{
    private readonly IOrderRepository _orderRepository;

    public GetAllOrdersHandler(INotificator notificator, IOrderRepository orderRepository)
        : base(notificator)
    {
        _orderRepository = orderRepository;
    }

    public override async Task<PagedResponse<GetAllOrdersResponse>> ExecuteAsync(GetAllOrdersQuery request, CancellationToken cancellationToken)
    {
        var orders = await _orderRepository.GetAllOrdersAsync(request.CustomerId, request.PageNumber, request.PageSize);
        if (orders is null || !orders.Any())
        {
            Notify("No orders found for the specified customer.");
            return PagedResponse<GetAllOrdersResponse>.Failure(GetNotifications());
        }

        return PagedResponse<GetAllOrdersResponse>.Success(
            new(orders.MapToResponse()),
            orders.TotalCount,
            request.PageNumber,
            request.PageSize);
    }
}
```

<li>Paged Query: Defines the parameters for the paginated query (GetAllOrdersQuery).</li>
<li>Response: Contains the returned data as a list (GetAllOrdersResponse).</li>
<li>Handler: Executes the paginated query logic and returns a PagedResponse with pagination details.</li>

# Dispatching a Use Case

Once configured, you can dispatch a command or query directly via IMediator. Here's an example in an API route:

```csharp
app.MapPost("/orders", async (int pageNumber, int pageSize, Guid customerId, IMediator mediator) =>
{
    var result = await mediator.DispatchAsync(new GetAllOrdersQuery(customerId, pageNumber, pageSize));
    return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
});
```
