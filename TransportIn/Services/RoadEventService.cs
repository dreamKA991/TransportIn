
using Microsoft.EntityFrameworkCore;
using TransportIn.Data;
using TransportIn.Data.Entities;

namespace TransportIn.Services;

public class RoadEventService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public RoadEventService(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public event Action? OnChange;

    public async Task<List<RoadEvent>> GetAllAsync()
    {
        await using var db = await _contextFactory.CreateDbContextAsync();

        return await db.RoadEvents
            .AsNoTracking()
            .OrderByDescending(e => e.StartedAt)
            .ToListAsync();
    }

    public async Task AddAsync(RoadEvent roadEvent)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();

        roadEvent.StartedAt = roadEvent.StartedAt == default
            ? DateTime.Now
            : roadEvent.StartedAt;

        db.RoadEvents.Add(roadEvent);
        await db.SaveChangesAsync();

        OnChange?.Invoke();
    }

    public async Task UpdateAsync(RoadEvent roadEvent)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();

        db.RoadEvents.Update(roadEvent);
        await db.SaveChangesAsync();

        OnChange?.Invoke();
    }

    public async Task DeleteAsync(int id)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();

        var roadEvent = await db.RoadEvents.FindAsync(id);

        if (roadEvent == null)
            return;

        db.RoadEvents.Remove(roadEvent);
        await db.SaveChangesAsync();

        OnChange?.Invoke();
    }

    public async Task SetActiveAsync(int id, bool isActive)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();

        var roadEvent = await db.RoadEvents.FindAsync(id);

        if (roadEvent == null)
            return;

        roadEvent.IsActive = isActive;

        if (!isActive && roadEvent.EndedAt == null)
            roadEvent.EndedAt = DateTime.Now;

        if (isActive)
            roadEvent.EndedAt = null;

        await db.SaveChangesAsync();

        OnChange?.Invoke();
    }
}
