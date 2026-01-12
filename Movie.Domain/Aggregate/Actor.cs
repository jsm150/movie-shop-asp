using Movie.Domain.Exceptions;

namespace Movie.Domain.Aggregate;

public record Actor
{
    public required string Name 
    { 
        get; 
        init
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new MovieDomainException("배우 이름은 필수입니다.");
            if (value.Length > 100)
                throw new MovieDomainException("배우 이름은 100자를 초과할 수 없습니다.");
            field = value;
        }
    }
    
    public required DateTime DateOfBirth 
    { 
        get; 
        init
        {
            if (value >= DateTime.UtcNow)
                throw new MovieDomainException("배우의 생년월일은 과거여야 합니다.");
            field = value;
        }
    }
    
    public required string National 
    { 
        get; 
        init
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new MovieDomainException("배우의 국적은 필수입니다.");
            if (value.Length > 100)
                throw new MovieDomainException("배우의 국적은 100자를 초과할 수 없습니다.");
            field = value;
        }
    }
    
    public required string Role 
    { 
        get; 
        init
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new MovieDomainException("배우의 역할은 필수입니다.");
            if (value.Length > 100)
                throw new MovieDomainException("배우의 역할은 100자를 초과할 수 없습니다.");
            field = value;
        }
    }
}