using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace BuildingBlocks.API.Application;

public interface ICommand<T> : IRequest<T>;


