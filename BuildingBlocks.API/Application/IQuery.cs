using MediatR;


namespace BuildingBlocks.API.Application;

public interface IQuery<T> : IRequest<T>;

