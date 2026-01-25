using BuildingBlocks.Domain;
using Theater.Domain.Exceptions;

namespace Theater.Domain.Aggregate;

public class Theater : IAggregateRoot
{
    public long TheaterId { get; init; }

    // 기본 정보
    public string Name
    {
        get;
        private set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new TheaterDomainException("상영관 이름은 필수입니다.");
            if (value.Length > 50)
                throw new TheaterDomainException("상영관 이름은 50자를 초과할 수 없습니다.");
            field = value;
        }
    } = null!;

    public int Floor
    {
        get;
        private set
        {
            if (value < -10 || value > 100)
                throw new TheaterDomainException("층수는 -10에서 100 사이여야 합니다.");
            field = value;
        }
    }

    // 상영관 타입
    public TheaterType Type
    {
        get;
        private set
        {
            if (!Enum.IsDefined(typeof(TheaterType), value))
                throw new TheaterDomainException("유효하지 않은 상영관 타입입니다.");
            field = value;
        }
    }

    // 좌석 정보
    private readonly List<TheaterSeat> _seats = [];
    public IReadOnlyCollection<TheaterSeat> Seats => _seats;
    public int TotalSeats => _seats.Count;

    // 상영관 크기/레이아웃
    public int RowCount
    {
        get;
        private set
        {
            if (value <= 0)
                throw new TheaterDomainException("행 수는 0보다 커야 합니다.");
            if (value > 100)
                throw new TheaterDomainException("행 수는 100을 초과할 수 없습니다.");
            field = value;
        }
    }

    public int ColumnCount
    {
        get;
        private set
        {
            if (value <= 0)
                throw new TheaterDomainException("열 수는 0보다 커야 합니다.");
            if (value > 50)
                throw new TheaterDomainException("열 수는 50을 초과할 수 없습니다.");
            field = value;
        }
    }

    // 운영 상태
    public bool IsActive { get; private set; } = true;

    private Theater() { }

    public async static Task<Theater> CreateAsync(
        ITheaterRepository theaterRepository,
        string name,
        int floor,
        TheaterType type,
        IReadOnlyCollection<string> seats,
        int rowCount,
        int columnCount)
    {
        if (seats == null || seats.Count == 0)
            throw new TheaterDomainException("최소 하나 이상의 좌석이 필요합니다.");
        if (seats.Count != rowCount * columnCount)
            throw new TheaterDomainException("좌석 수가 행과 열의 곱과 일치하지 않습니다.");
        if (seats.Distinct().Count() != seats.Count)
            throw new TheaterDomainException("중복된 좌석이 있습니다.");
        if (await theaterRepository.ContainsName(name))
            throw new TheaterDomainException($"\"{name}\" 이름의 상영관이 이미 존재합니다.");

        var theater = new Theater
        {
            Name = name,
            Floor = floor,
            Type = type,
            RowCount = rowCount,
            ColumnCount = columnCount
        };

        theater._seats.AddRange(seats.Select(s => new TheaterSeat(s)));
        return theater;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}

public enum TheaterType
{
    Standard,       // 일반
    IMAX,           // IMAX
    FourDX,         // 4DX
    ScreenX,        // ScreenX
    Dolby,          // Dolby Cinema
    Premium         // 프리미엄
}
