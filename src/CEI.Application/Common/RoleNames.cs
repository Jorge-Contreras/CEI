namespace CEI.Application.Common;

public static class RoleNames
{
    public const string PrincipalLawyer = "PrincipalLawyer";
    public const string Administrator = "Administrator";
    public const string Assistant = "Assistant";
    public const string Specialist = "Specialist";

    public static IReadOnlyList<string> All { get; } =
    [
        PrincipalLawyer,
        Administrator,
        Assistant,
        Specialist
    ];
}
