namespace MafiaContractsBot.Features.ContractsRanking.Models;

public class ContractUser
{
    public required ulong Id { get; init; }

    public required string Name { get; init; }

    public List<CompletedContract> CompletedContracts { get; private set; } = [];

    internal void CompleteContract(Contract contract, int points)
    {
        var existingContract = CompletedContracts.FirstOrDefault(cc => cc.Contract == contract);
        if (existingContract is not null)
        {
            throw new ContractsDomainException($"Użytkownik już ukończył ten kontrakt; dostał za niego {existingContract.Points} punktów w dniu {existingContract.CompletedOn}.");
        }

        var completedContract = new CompletedContract
        {
            User = this,
            Contract = contract,
            Points = points
        };

        CompletedContracts.Add(completedContract);
    }
}
