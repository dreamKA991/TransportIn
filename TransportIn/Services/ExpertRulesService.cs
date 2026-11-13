using Microsoft.EntityFrameworkCore;
using TransportIn.Data;

namespace TransportIn.Services
{
    public class ExpertRule
    {
        public int Id { get; set; }
        public string ThreatType { get; set; } = "";
        public string Condition { get; set; } = "";
        public string ActionRecommendation { get; set; } = "";
        public string Priority { get; set; } = "Високий";
    }

    public class ExpertRulesService
    {
        private readonly AppDbContext _db;

        public ExpertRulesService(AppDbContext db) => _db = db;

        public event Action? OnChange;

        public List<ExpertRule> GetAllRules() => _db.ExpertRules
            .AsNoTracking()
            .OrderBy(rule => rule.Id)
            .Select(rule => new ExpertRule
            {
                Id = rule.Id,
                ThreatType = rule.ThreatType,
                Condition = rule.Condition,
                ActionRecommendation = rule.ActionRecommendation,
                Priority = rule.Priority
            })
            .ToList();

        public void AddRule(ExpertRule rule)
        {
            var entity = new TransportIn.Data.Entities.ExpertRule
            {
                ThreatType = rule.ThreatType,
                Condition = rule.Condition,
                ActionRecommendation = rule.ActionRecommendation,
                Priority = rule.Priority
            };

            _db.ExpertRules.Add(entity);
            _db.SaveChanges();
            rule.Id = entity.Id;
            NotifyStateChanged();
        }

        public void DeleteRule(int id)
        {
            var rule = _db.ExpertRules.FirstOrDefault(item => item.Id == id);
            if (rule == null) return;

            _db.ExpertRules.Remove(rule);
            _db.SaveChanges();
            NotifyStateChanged();
        }

        public string EvaluateThreat(string threatType, bool isInTransit)
        {
            var rule = GetAllRules().FirstOrDefault(item =>
                item.ThreatType.Contains(threatType, StringComparison.OrdinalIgnoreCase));

            return rule == null
                ? "Дійте за стандартним протоколом безпеки."
                : $"[Рекомендація експертної системи]: {rule.ActionRecommendation} (Пріоритет: {rule.Priority})";
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
