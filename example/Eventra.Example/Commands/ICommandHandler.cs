namespace Eventra.Example.Commands;

public interface ICommandHandler<in TCommand, TResult>
{
    Task<TResult> HandleAsync(
        TCommand command,
        CancellationToken cancellationToken = default);
}