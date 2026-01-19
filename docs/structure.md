# Project Structure (Modular Monolith + In-Process Messaging)

## 1. 개요
- 아키텍처: **모듈형 모놀리스(Modular Monolith)**
- 모듈 간 통합: 외부 브로커 없이 **인프로세스 메시징(MediatR `INotification`)**
- 처리 방식: 기본 **동기** + **단일 DB 트랜잭션**에서 커밋(실패 시 전체 롤백)

보장 목표
- 각 모듈은 자체 책임/도메인 규칙을 가진다.
- 모듈 간 연동은 `IntegrationEvents` 계약으로 수행한다.
- 요청 처리 + 이벤트 핸들링이 하나의 트랜잭션으로 원자적으로 처리된다.

---

## 2. 모듈 구성(Projects)
- Movie 모듈
  - `Movie.Domain`
  - `Movie.Infrastructure`
  - `Movie.API`
- Screening 모듈
  - `Screening.Domain`
  - `Screening.Infrastructure`
  - `Screening.API`
- 모듈 간 계약(Contract)
  - `IntegrationEvents`
- Host / Composition Root
  - `movie-shop-asp.Server` (단일 프로세스에서 모든 모듈 조립/실행)

---

## 3. 레이어 정의(이 프로젝트 기준)

### 3.1 Domain (`*.Domain`)
책임
- 엔티티/애그리게잇/값 객체/도메인 규칙/상태 전이
- 도메인 예외

### 3.2 Application (`*.API/Application`)
책임
- 유스케이스 구현(Command/Query + Handler)
- 입력 검증(Validation)
- 통합 이벤트 “발행 요청”(enqueue)

### 3.3 Infrastructure (`*.Infrastructure`, `movie-shop-asp.Server/Infrastructure`)
책임
- EF Core 영속성(`DbContext`, 매핑, Repository 등)
- 외부 시스템 연동(확장 지점)

### 3.4 Presentation/API (`*.API/Controllers`)
책임
- HTTP 진입점
- DTO 매핑/입출력 변환
- `MediatR.Send`로 Application에 위임

### 3.5 Host / Composition Root (`movie-shop-asp.Server`)
책임
- DI 구성
- `MediatR`/파이프라인(Behavior) 등록
- `DbContext` 구성
- 예외/미들웨어 구성

---

## 4. 트랜잭션/메시징 골든 플로우(Golden Path)
Register Movie
1) Controller → `MediatR.Send(RegisterMovieCommand)`
2) `ValidatorBehavior`에서 검증
3) `TransactionBehavior`에서 트랜잭션 시작
4) CommandHandler에서 DB 저장 + `SaveEntitiesAsync`
5) `integrationEventService.Add(new MovieCreatedIntegrationEvent(...))`로 이벤트 적재
6) `TransactionBehavior`에서 `DispatchIntegrationEventsAsync()` 실행(동기 핸들링)
7) 타 모듈 핸들러도 동일 트랜잭션 내에서 저장
8) 커밋