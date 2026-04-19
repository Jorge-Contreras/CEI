using CEI.Domain.Common;

namespace CEI.Domain.Cases;

public class ProcedureTemplate : LookupEntity
{
    private ProcedureTemplate()
    {
    }

    public ProcedureTemplate(string systemKey, string name, string matter, string jurisdictionName)
        : base(systemKey, name)
    {
        Matter = matter;
        JurisdictionName = jurisdictionName;
    }

    public string Matter { get; private set; } = string.Empty;

    public string JurisdictionName { get; private set; } = string.Empty;

    public string? Notes { get; private set; }

    public void SetNotes(string? notes)
    {
        Notes = notes;
    }

    public void UpdateDefinition(string name, string matter, string jurisdictionName, string? notes)
    {
        UpdateName(name);
        Matter = matter;
        JurisdictionName = jurisdictionName;
        Notes = notes;
        Activate();
    }
}
