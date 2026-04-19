namespace CEI.Domain.Common;

public abstract class LookupEntity
{
    public int Id { get; protected set; }

    public string SystemKey { get; protected set; } = string.Empty;

    public string Name { get; protected set; } = string.Empty;

    public bool IsActive { get; protected set; } = true;

    protected LookupEntity()
    {
    }

    protected LookupEntity(string systemKey, string name)
    {
        SystemKey = systemKey;
        Name = name;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void UpdateName(string name)
    {
        Name = name;
    }

    public void Activate()
    {
        IsActive = true;
    }
}
