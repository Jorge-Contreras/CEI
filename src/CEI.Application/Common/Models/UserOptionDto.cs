namespace CEI.Application.Common.Models;

public sealed record UserOptionDto(Guid Id, string FullName, string Email, IReadOnlyList<string> Roles);
