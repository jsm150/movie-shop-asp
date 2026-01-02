using System;
using System.Collections.Generic;
using System.Text;

namespace Movie.Domain.Aggregate;

public record Actor(
    string Name,
    DateTime DateOfBirth,
    string National,
    string Role
);