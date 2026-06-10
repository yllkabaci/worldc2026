using WorldCup.Domain.Common;

namespace WorldCup.Domain.Users;

public sealed record UserRegisteredEvent(UserId UserId, DateTimeOffset OccurredOnUtc) : IDomainEvent;
