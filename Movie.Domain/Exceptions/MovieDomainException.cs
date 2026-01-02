using System;
using System.Collections.Generic;
using System.Text;

namespace Movie.Domain.Exceptions
{
    internal class MovieDomainException(string message) : Exception(message)
    {
    }
}
