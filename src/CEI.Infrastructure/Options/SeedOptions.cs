namespace CEI.Infrastructure.Options;

public sealed class SeedOptions
{
    public const string SectionName = "Seed";

    public string DemoPassword { get; set; } = string.Empty;
}
