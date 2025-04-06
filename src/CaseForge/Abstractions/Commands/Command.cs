namespace CaseForge.Abstractions.Commands;

public abstract record Command<TResult> : ICommand<TResult>
{
    public Guid Id => Guid.NewGuid();
}