using CEI.Domain.Common;

namespace CEI.Domain.Cases;

public class CaseCategory : LookupEntity
{
    private CaseCategory()
    {
    }

    public CaseCategory(string systemKey, string name)
        : base(systemKey, name)
    {
    }
}
