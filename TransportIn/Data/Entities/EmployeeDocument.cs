using System;
using System.Collections.Generic;

namespace TransportIn.Data.Entities;

public partial class EmployeeDocument
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public int DocumentType { get; set; }
    public string DocumentNumber { get; set; } = null!;
    public string? DocumentSeries { get; set; }
    public string? UniqueRecordNumber { get; set; }
    public DateTime IssueDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string IssuedBy { get; set; } = null!;
    public string Details { get; set; } = null!;
    public string? InstitutionName { get; set; }
    public string? Specialty { get; set; }
    public int? GraduationYear { get; set; }
    public string? MilitarySpecialtyCode { get; set; }
    public string? MilitaryRank { get; set; }
    public string? RecruitmentOffice { get; set; }
    public string? FilePath { get; set; }
    public virtual Employee Employee { get; set; } = null!;
}
