using CEI.Domain.Common;

namespace CEI.Domain.Cases;

public class CaseEventType : LookupEntity
{
    private CaseEventType()
    {
    }

    public CaseEventType(string systemKey, string name)
        : base(systemKey, name)
    {
    }
}
