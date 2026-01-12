using Movie.Domain.Exceptions;

namespace Movie.Domain.Aggregate
{
    public record MovieInfo
    {
        public required string Title 
        { 
            get; 
            init
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new MovieDomainException("영화 제목은 필수입니다.");
                if (value.Length > 200)
                    throw new MovieDomainException("영화 제목은 200자를 초과할 수 없습니다.");
                field = value;
            }
        }
        
        public required string Director 
        { 
            get; 
            init
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new MovieDomainException("감독 이름은 필수입니다.");
                if (value.Length > 100)
                    throw new MovieDomainException("감독 이름은 100자를 초과할 수 없습니다.");
                field = value;
            }
        }
        
        public required IReadOnlyCollection<string> Genres 
        { 
            get; 
            init
            {
                if (value == null || value.Count == 0)
                    throw new MovieDomainException("최소 하나 이상의 장르가 필요합니다.");
                if (value.Any(g => string.IsNullOrWhiteSpace(g)))
                    throw new MovieDomainException("장르는 빈 값이나 공백을 포함할 수 없습니다.");
                field = value;
            }
        }
        
        public required int RuntimeMinutes 
        { 
            get; 
            init
            {
                if (value <= 0)
                    throw new MovieDomainException("상영 시간은 0보다 커야 합니다.");
                field = value;
            }
        }
        
        public required AdienceRating AdienceRating 
        { 
            get; 
            init
            {
                if (!Enum.IsDefined(typeof(AdienceRating), value))
                    throw new MovieDomainException("유효하지 않은 관람 등급입니다.");
                field = value;
            }
        }
        
        public required string Synopsis 
        { 
            get; 
            init
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new MovieDomainException("시놉시스는 필수입니다.");
                if (value.Length > 1000)
                    throw new MovieDomainException("시놉시스는 1000자를 초과할 수 없습니다.");
                field = value;
            }
        }
        
        public required DateTime ReleaseDate 
        { 
            get; 
            init
            {
                if (value == DateTime.MinValue)
                    throw new MovieDomainException("개봉일은 필수입니다.");
                field = value;
            }
        }
        
        public required IReadOnlyCollection<Actor> Casts 
        { 
            get; 
            init
            {
                if (value == null || value.Count == 0)
                    throw new MovieDomainException("최소 한 명 이상의 출연진이 필요합니다.");
                field = value;
            }
        }
    }
}