using System;
using System.Collections.Generic;
using System.Text;

namespace Theater.Domain.Exceptions;

public class TheaterDomainException(string message) : Exception(message)
{
}
