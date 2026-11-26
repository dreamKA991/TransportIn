using Microsoft.EntityFrameworkCore;
using TransportIn.Data;

namespace TransportIn.Tests;

internal sealed class NoDatabaseContextFactory : IDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext() =>
        throw new InvalidOperationException("This scenario must finish before accessing the database.");
}
