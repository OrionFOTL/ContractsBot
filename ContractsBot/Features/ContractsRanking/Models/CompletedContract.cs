namespace MafiaContractsBot.Features.ContractsRanking.Models;

public class CompletedContract
{
    public Guid Id { get; private set; }

    public required ContractUser User { get; set; }

    public required Contract Contract { get; set; }

    public required int Points { get; set; }

    public DateTimeOffset CompletedOn { get; private set; } = DateTimeOffset.Now;
}
