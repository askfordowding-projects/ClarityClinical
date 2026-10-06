using Microsoft.EntityFrameworkCore;

namespace ClarityClinical.Infrastructure.Persistence;

public sealed class ClarityClinicalDbContext(DbContextOptions<ClarityClinicalDbContext> options)
    : DbContext(options)
{
}
