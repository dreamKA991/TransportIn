
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
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public ExpertRulesService(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public event Action? OnChange;

        public List<ExpertRule> GetAllRules()
        {
            using var db = _contextFactory.CreateDbContext();

            return db.ExpertRules
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
        }

        public async Task<List<ExpertRule>> GetAllRulesAsync()
        {
            await using var db =
                await _contextFactory.CreateDbContextAsync();

            return await db.ExpertRules
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
                .ToListAsync();
        }

        public void AddRule(ExpertRule rule)
        {
            using var db = _contextFactory.CreateDbContext();

            var entity = new TransportIn.Data.Entities.ExpertRule
            {
                ThreatType = rule.ThreatType,
                Condition = rule.Condition,
                ActionRecommendation = rule.ActionRecommendation,
                Priority = rule.Priority
            };

            db.ExpertRules.Add(entity);
            db.SaveChanges();

            rule.Id = entity.Id;
            NotifyStateChanged();
        }

        public void DeleteRule(int id)
        {
            using var db = _contextFactory.CreateDbContext();

            var rule = db.ExpertRules
                .FirstOrDefault(item => item.Id == id);

            if (rule == null)
                return;

            db.ExpertRules.Remove(rule);
            db.SaveChanges();

            NotifyStateChanged();
        }

        public string EvaluateThreat(string threatType, bool isInTransit)
        {
            var rule = GetAllRules().FirstOrDefault(item =>
                item.ThreatType.Contains(
                    threatType,
                    StringComparison.OrdinalIgnoreCase
                ));

            return rule == null
                ? "Дійте за стандартним протоколом безпеки."
                : $"[Рекомендація експертної системи]: {rule.ActionRecommendation} (Пріоритет: {rule.Priority})";
        }

        private void NotifyStateChanged()
        {
            OnChange?.Invoke();
        }
    }
}
