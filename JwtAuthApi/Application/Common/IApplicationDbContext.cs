using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Common;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}