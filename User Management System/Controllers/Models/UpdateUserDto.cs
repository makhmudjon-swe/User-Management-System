using Domain.Enums;

public record UpdateUsersDto(List<Guid> Ids, UserStatus Status);