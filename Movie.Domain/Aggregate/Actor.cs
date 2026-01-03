using System;
using System.Collections.Generic;
using System.Text;

namespace Movie.Domain.Aggregate;

public record Actor
{
    public required string Name { get; init; }
    public required DateTime DateOfBirth { get; init; }
    public required string National { get; init; }
    public required string Role { get; init; }
}