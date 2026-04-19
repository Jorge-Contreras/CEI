using CEI.Domain.Common;

namespace CEI.Domain.Cases;

public class CaseStatus : LookupEntity
{
    private CaseStatus()
    {
    }

    public CaseStatus(string systemKey, string name)
        : base(systemKey, name)
    {
    }
}
