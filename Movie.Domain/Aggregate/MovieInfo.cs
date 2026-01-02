namespace Movie.Domain.Aggregate
{
    public record MovieInfo(
        string Title,
        string Director,
        IReadOnlyCollection<string> Genres,
        int RuntimeMinutes,
        string Synopsis,
        DateTime ReleaseDate,
        IReadOnlyCollection<Actor> Casts
    );
}