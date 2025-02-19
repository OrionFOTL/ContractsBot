using System.ComponentModel.DataAnnotations;

namespace MafiaContractsBot.Features.ContractsRanking.Models;

public class Contract
{
    [Key]
    public required ulong ThreadId { get; init; }

    public required string Title { get; set; }
}
