using CEI.Domain.Common;
using CEI.Domain.Enums;

namespace CEI.Domain.Cases;

public class LegalCase : AuditableEntity
{
    public string InternalCode { get; private set; } = string.Empty;

    public string? ExternalCaseNumber { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public string Summary { get; private set; } = string.Empty;

    public string? Notes { get; private set; }

    public int CaseCategoryId { get; private set; }

    public int CaseStatusId { get; private set; }

    public JurisdictionLevel JurisdictionLevel { get; private set; }

    public string VenueState { get; private set; } = string.Empty;

    public string VenueName { get; private set; } = string.Empty;

    public int ProcedureTemplateId { get; private set; }

    public bool IsSensitive { get; private set; }

    public DateOnly OpenDate { get; private set; }

    public DateOnly? CloseDate { get; private set; }

    public Guid ResponsibleLawyerId { get; private set; }

    public CaseCategory CaseCategory { get; set; } = null!;

    public CaseStatus CaseStatus { get; set; } = null!;

    public ProcedureTemplate ProcedureTemplate { get; set; } = null!;

    public ICollection<CaseAssignment> Assignments { get; } = new List<CaseAssignment>();

    public ICollection<CaseEvent> Events { get; } = new List<CaseEvent>();

    public ICollection<CaseDeadline> Deadlines { get; } = new List<CaseDeadline>();

    public ICollection<CaseDocument> Documents { get; } = new List<CaseDocument>();

    private LegalCase()
    {
    }

    public LegalCase(
        string internalCode,
        string title,
        string summary,
        int caseCategoryId,
        int caseStatusId,
        JurisdictionLevel jurisdictionLevel,
        string venueState,
        string venueName,
        int procedureTemplateId,
        bool isSensitive,
        DateOnly openDate,
        Guid responsibleLawyerId,
        string? externalCaseNumber = null,
        string? notes = null)
    {
        InternalCode = internalCode;
        Title = title;
        Summary = summary;
        CaseCategoryId = caseCategoryId;
        CaseStatusId = caseStatusId;
        JurisdictionLevel = jurisdictionLevel;
        VenueState = venueState;
        VenueName = venueName;
        ProcedureTemplateId = procedureTemplateId;
        IsSensitive = isSensitive;
        OpenDate = openDate;
        ResponsibleLawyerId = responsibleLawyerId;
        ExternalCaseNumber = externalCaseNumber;
        Notes = notes;
    }

    public void UpdateDetails(
        string title,
        string summary,
        int caseCategoryId,
        int caseStatusId,
        JurisdictionLevel jurisdictionLevel,
        string venueState,
        string venueName,
        int procedureTemplateId,
        bool isSensitive,
        DateOnly openDate,
        DateOnly? closeDate,
        Guid responsibleLawyerId,
        string? externalCaseNumber,
        string? notes)
    {
        Title = title;
        Summary = summary;
        CaseCategoryId = caseCategoryId;
        CaseStatusId = caseStatusId;
        JurisdictionLevel = jurisdictionLevel;
        VenueState = venueState;
        VenueName = venueName;
        ProcedureTemplateId = procedureTemplateId;
        IsSensitive = isSensitive;
        OpenDate = openDate;
        CloseDate = closeDate;
        ResponsibleLawyerId = responsibleLawyerId;
        ExternalCaseNumber = externalCaseNumber;
        Notes = notes;
    }
}
