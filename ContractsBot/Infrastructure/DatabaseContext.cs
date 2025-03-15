using ContractsBot.Features.ContractsRanking.Models;
using Microsoft.EntityFrameworkCore;

namespace ContractsBot.Infrastructure;

public class DatabaseContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<ContractUser> ContractUsers => Set<ContractUser>();

    public DbSet<Contract> Contracts => Set<Contract>();

    public DbSet<CompletedContract> CompletedContracts => Set<CompletedContract>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.EnableSensitiveDataLogging();
    }
}
