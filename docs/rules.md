# Behavioral Rules (Must-Follow)

이 문서는 "모듈형 모놀리스 + 인프로세스 통합 이벤트 + 단일 트랜잭션"을 유지하기 위해,
코드를 작성/변경할 때 반드시 지켜야 하는 **행동 규칙(규율)** 을 정의한다.

---

## 1. 의존성 규칙(방향) - 깨지면 즉시 리팩터링
허용되는 의존 방향은 “안쪽으로”만 향한다.

- `Presentation(API)` → `Application` → `Domain`
- `Infrastructure` → `Domain` (매핑/저장을 위한 참조는 가능)
- `Host(movie-shop-asp.Server)` → 전체 조립(컴포지션 루트이므로 참조 가능)

금지(명시)
- `Domain`이 `Infrastructure`, `API`, `MediatR`, `EF Core` 참조 금지
- 타 모듈의 `Domain` 타입(엔티티/VO) 직접 사용 금지  
  - 필요 데이터는 `IntegrationEvents`로 전달
  - 수신 모듈에서 **명시적 매핑(switch/translator)** 으로 자기 모델로 변환

---

## 2. “명령(Command)” 처리 골든 파이프라인 규칙 (중요)

본 프로젝트의 커맨드 처리 표준 흐름은 다음 순서를 **항상** 따른다.

1) Controller → `IMediator.Send(command)`
2) `ValidatorBehavior`에서 입력 검증
3) `TransactionBehavior`에서 트랜잭션 시작(필요 시)
4) CommandHandler(유스케이스) 실행
5) CommandHandler가 변경사항 저장 + Integration Event 적재(enqueue)
6) `TransactionBehavior`가 `DispatchIntegrationEventsAsync()` 실행(동기 핸들링)
7) 모든 변경 커밋(한 번에)
8) 실패 시 전체 롤백

### 2.1 Controller(진입점) 규칙
- Controller는 **비즈니스 로직 금지**, 단순 위임만 한다.
- Controller에서 다음 행위 금지:
  - `DbContext`/Repository 직접 접근
  - 트랜잭션 생성/커밋/롤백
  - `IMediator.Publish` 호출(이벤트 즉시 발행 금지)

### 2.2 Validation 규칙 (`ValidatorBehavior`)
- 모든 Command/Query는 가능한 한 Validator로 검증한다.
- 검증 실패는 예외로 처리하며, Handler 로직을 실행하지 않는다.
- 핸들러 내부에서 “입력 형식 검증”을 반복하지 않는다(도메인 규칙은 예외).

### 2.3 Transaction 규칙 (`TransactionBehavior`)
- 트랜잭션은 파이프라인이 소유한다.
  - CommandHandler는 트랜잭션을 시작/커밋하지 않는다.
- Integration Event 발행은 항상 다음 순서를 강제한다.
  - Handler 종료 후 → `DispatchIntegrationEventsAsync()` → 커밋
- 이미 활성 트랜잭션이 있으면(`HasActiveTransaction`) 중첩 트랜잭션을 만들지 않는다.

### 2.4 CommandHandler(유스케이스) 규칙
CommandHandler는 아래 “3가지”만 책임진다.

1) 도메인 모델 로드/생성
2) 도메인 규칙에 따라 상태 변경(도메인 메서드 사용)
3) 저장 + Integration Event 적재(enqueue)

CommandHandler에서 금지:
- `IMediator.Publish`/`Publish` 계열 API 호출 금지  
  (이 프로젝트는 **즉시 발행이 아니라 적재 후 디스패치**가 표준)
- 트랜잭션 직접 제어 금지
- 타 모듈 Domain 모델 직접 생성/수정 금지

CommandHandler 저장 규칙:
- DB 변경이 있다면 `SaveEntitiesAsync()`를 호출하여 영속화한다.
- 이벤트를 적재했다면, “이벤트와 함께 커밋”되는 전제를 깨지 않도록
  **핸들러 내부에서 임의로 커밋/Dispose/새 DbContext 생성**을 하지 않는다.

---

## 3. Integration Events 규칙 (인프로세스/동기/단일 트랜잭션)

### 3.1 계약 위치/형태
- 계약 정의: `IntegrationEvents/Events/*IntegrationEvent.cs`
- 이벤트 타입: `MediatR.INotification`

### 3.2 발행(적재) 규칙
- CommandHandler는 이벤트를 즉시 발행하지 않는다.
- 반드시 `InProcessIntegrationEventService.Add(INotification)`로 **적재만** 한다.
- 실제 발행은 `TransactionBehavior` 내부 `DispatchIntegrationEventsAsync()`에서 수행한다.

### 3.3 이벤트 데이터(payload) 규칙
- 다른 모듈이 필요로 하는 “최소 데이터”만 포함(primitive + enum 중심)
- 도메인 엔티티/VO를 payload로 전달 금지
- 모듈 경계 넘는 enum/status는 수신 모듈에서 **명시적 매핑** 한다.

### 3.4 이벤트 핸들러 규칙 (INotificationHandler)
- 이벤트 핸들러는 “수신 모듈의 로컬 모델”만 변경한다.
- 이벤트 핸들러에서 금지:
  - 타 모듈 Domain 타입을 직접 사용/영속화
  - 트랜잭션 시작/커밋
  - 외부 시스템 호출(필요하면 별도 Outbox/비동기 채널로 분리)

- 이벤트 핸들러 저장:
  - `SaveEntitiesAsync()`는 호출할 수 있으나,
    최종 커밋은 `TransactionBehavior`가 수행한다.

---

## 4. EF Core/DbContext 공유 규칙 (단일 트랜잭션 기반)
- `movie-shop-asp.Server/Infrastructure/MovieShopContext`는 여러 모듈 컨텍스트 인터페이스를 구현한다.
- DI는 동일 요청/스코프에서 동일 DbContext 인스턴스를 공유하도록 구성되어야 한다.
- 새 작업에서 다음은 금지:
  - 모듈마다 별도 DbContext 생성(동일 트랜잭션 깨짐)
  - 이벤트 핸들러가 새 스코프를 열어 별도 DbContext 사용

---

## 5. 변경 시 체크리스트 (PR 작성 전 필수)
- 커맨드 처리에서 `IMediator.Publish`를 호출하지 않았는가?
- 이벤트는 `Add(...)`로 적재하고, 디스패치는 파이프라인에서만 수행되는가?
- Controller가 비즈니스 로직/저장/트랜잭션을 포함하지 않는가?
- Domain 프로젝트가 EF Core/MediatR/ASP.NET 참조를 추가하지 않았는가?
- 타 모듈 Domain 타입을 직접 사용하지 않고 IntegrationEvent + 매핑으로 처리했는가?
- 이벤트 핸들러가 외부 호출/트랜잭션 직접 제어를 하지 않는가?

---

## X. 코드를 실제로 작성하는 규칙(Repository/UoW/Context/Handler)

### X.1 Repository 기본 규칙 (반드시 `IRepository<T>` 상속)
- 모든 Repository 인터페이스는 `BuildingBlocks.Domain.IRepository<T>`를 상속한다.
  - 이유: `IRepository<T>`가 `IUnitOfWork UnitOfWork { get; }`를 강제한다.
  - 근거: `BuildingBlocks/Domain/IRepository.cs`
    - `public interface IRepository<T> ... { IUnitOfWork UnitOfWork { get; } }`

예)
- `Movie.Domain.Aggregate.IMovieRepository : IRepository<Movie>`
- `Screening.Domain.Aggregate.MovieAggregate.IMovieRepository : IRepository<Movie>`

### X.2 Unit of Work 규칙 (저장은 항상 `SaveEntitiesAsync`)
- 저장 호출은 `IUnitOfWork.SaveEntitiesAsync(...)`만 사용한다.
  - 근거: `BuildingBlocks/Domain/IUnitOfWork.cs`
    - `Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default);`
- Application/Handler 단에서 `DbContext.SaveChangesAsync()` 직접 호출 금지.
- “저장 시점” 규칙:
  - CommandHandler: 상태 변경 후 `repository.UnitOfWork.SaveEntitiesAsync(...)` 호출
  - IntegrationEventHandler: 로컬 상태 동기화 후 `context.SaveEntitiesAsync(...)` 호출 가능(단, 커밋은 파이프라인에서)

### X.3 CommandHandler 작성 규칙 (인프라 직접 의존 금지)
CommandHandler는 아래에만 의존한다(생성자 주입 기준).
- 도메인 Repository 인터페이스(예: `Movie.Domain.Aggregate.IMovieRepository`)
- `InProcessIntegrationEventService`(이벤트 적재용)

CommandHandler에서 아래는 금지한다.
- `DbContext`, `DbSet<T>`, `context.Set<T>()`, `ToListAsync`, `Include` 등 EF Core 직접 사용
- `MovieShopContext`, `IScreeningContext` 같은 “컨텍스트” 직접 사용

정답 패턴(근거: `Movie.API/Application/Commands/RegisterMovieCommandHandler.cs`)
- `movieRepository.Add(...)`
- `await movieRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);`
- `integrationEventService.Add(new XxxIntegrationEvent(...));`
- `IMediator.Publish(...)` 호출 금지(즉시 발행 금지)

### X.4 Integration Event Handler 작성 규칙 (컨텍스트 사용 범위)
Integration Event Handler는 “로컬 읽기/쓰기”를 위해 컨텍스트 추상화 사용을 허용한다.
- 예: `public class ... (IScreeningContext context) : INotificationHandler<...>`

단, 아래를 지킨다.
- 금지: 트랜잭션 시작/커밋/롤백(커밋은 `TransactionBehavior` 담당)
- 금지: 타 모듈 Repository/Domain 모델 직접 수정

### X.5 컨텍스트 인터페이스 규칙 (`IUnitOfWork` 상속)
모듈 컨텍스트 인터페이스는 반드시 `IUnitOfWork`를 상속한다.
- 근거: 현재 파일 `Screening.Infrastructure/IScreeningContext.cs`
  - `public interface IScreeningContext : IUnitOfWork`
- 이유: Event Handler가 저장할 때도 동일한 UoW 계약(`SaveEntitiesAsync`)을 사용하게 강제하기 위함

### X.6 단일 트랜잭션 규칙 (파이프라인이 커밋)
- 커밋은 `TransactionBehavior`에서만 수행한다.
  - 근거: `movie-shop-asp.Server/Application/Behaviors/TransactionBehavior.cs`
    - `BeginTransactionAsync()` → `next()` → `DispatchIntegrationEventsAsync()` → `CommitTransactionAsync(...)`
- Handler에서 트랜잭션을 만들거나 커밋하면 규칙 위반.

### X.7 통합 이벤트 적재 규칙 (즉시 발행 금지)
- CommandHandler는 통합 이벤트를 즉시 발행하지 않고 적재만 한다.
  - `integrationEventService.Add(@event);`
- 실제 디스패치는 `TransactionBehavior`의 `DispatchIntegrationEventsAsync()`에서만 수행한다.