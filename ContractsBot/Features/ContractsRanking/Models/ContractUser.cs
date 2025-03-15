using Discord;

namespace ContractsBot.Features.ContractsRanking.Models;

public class ContractUser
{
    public required ulong Id { get; init; }

    public required string Name { get; init; }

    public List<CompletedContract> CompletedContracts { get; private set; } = [];

    internal void CompleteContract(Contract contract, int points)
    {
        var completedContract = CompletedContracts.FirstOrDefault(cc => cc.Contract.ThreadId == contract.ThreadId);
        if (completedContract != null)
        {
            completedContract.Points = points;
        }
        else
        {
            CompletedContracts.Add(new CompletedContract
            {
                User = this,
                Contract = contract,
                Points = points,
            });
        }
    }
}
