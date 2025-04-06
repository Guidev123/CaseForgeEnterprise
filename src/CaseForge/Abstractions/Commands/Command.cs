using CaseForge.Messages;

namespace CaseForge.Abstractions.Commands;

public abstract record Command<TResult> : Message, ICommand<TResult>
{
    public Guid Id => Guid.NewGuid();
}