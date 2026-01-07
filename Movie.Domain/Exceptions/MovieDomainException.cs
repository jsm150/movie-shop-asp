using System;
using System.Collections.Generic;
using System.Text;

namespace Movie.Domain.Exceptions
{
    public class MovieDomainException(string message) : Exception(message)
    {
    }
}
