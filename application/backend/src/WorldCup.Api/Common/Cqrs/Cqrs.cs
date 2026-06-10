using MediatR;

namespace WorldCup.Api.Common.Cqrs;

/// <summary>A state-changing request. Committed by the UnitOfWork behavior.</summary>
public interface ICommand<out TResponse> : IRequest<TResponse> { }

/// <summary>A read-only request. Never committed.</summary>
public interface IQuery<out TResponse> : IRequest<TResponse> { }

public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse> { }

public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse> { }
