namespace Movie.Domain.Aggregate
{
    public record MovieInfo
    {
        public required string Title { get; init; }
        public required string Director { get; init; }
        public required IReadOnlyCollection<string> Genres { get; init; }
        public required int RuntimeMinutes { get; init; }
        public required AdienceRating AdienceRating { get; init; }
        public required string Synopsis { get; init; }
        public required DateTime ReleaseDate { get; init; }
        public required IReadOnlyCollection<Actor> Casts { get; init; }
    }
}