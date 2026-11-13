using System;
using System.Collections.Generic;

namespace TransportIn.Data.Entities;

public partial class RuleExecutionHistory
{
    public int Id { get; set; }

    public int ExpertRuleId { get; set; }

    public DateTime ExecutedAt { get; set; }

    public string TriggerDescription { get; set; } = null!;

    public string Result { get; set; } = null!;

    public string Details { get; set; } = null!;

    public bool WasSuccessful { get; set; }

    public int? EmployeeId { get; set; }

    public virtual Employee? Employee { get; set; }

    public virtual ExpertRule ExpertRule { get; set; } = null!;
}
