using System;
using System.Collections.Generic;

namespace TransportIn.Data.Entities;

public partial class ExpertRule
{
    public int Id { get; set; }

    public string ThreatType { get; set; } = null!;

    public string Condition { get; set; } = null!;

    public string ActionRecommendation { get; set; } = null!;

    public string Priority { get; set; } = null!;

    public virtual ICollection<RuleCondition> RuleConditions { get; set; } = new List<RuleCondition>();

    public virtual ICollection<RuleExecutionHistory> RuleExecutionHistories { get; set; } = new List<RuleExecutionHistory>();
}
