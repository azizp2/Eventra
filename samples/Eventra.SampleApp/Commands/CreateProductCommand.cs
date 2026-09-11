using MediatR;

namespace Eventra.SampleApp.Commands;

public sealed record CreateProductCommand(string Name) : IRequest<Guid>;