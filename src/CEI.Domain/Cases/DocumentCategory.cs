using CEI.Domain.Common;

namespace CEI.Domain.Cases;

public class DocumentCategory : LookupEntity
{
    private DocumentCategory()
    {
    }

    public DocumentCategory(string systemKey, string name)
        : base(systemKey, name)
    {
    }
}
