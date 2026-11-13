using System;
using System.Collections.Generic;

namespace TransportIn.Data.Entities;

public partial class RuleCondition
{
    public int Id { get; set; }

    public int ExpertRuleId { get; set; }

    public string Parameter { get; set; } = null!;

    public string Operator { get; set; } = null!;

    public string ExpectedValue { get; set; } = null!;

    public string LogicalOperator { get; set; } = null!;

    public int SequenceNumber { get; set; }

    public string Description { get; set; } = null!;

    public virtual ExpertRule ExpertRule { get; set; } = null!;
}
