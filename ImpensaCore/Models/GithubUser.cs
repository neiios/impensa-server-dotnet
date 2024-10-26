using System.ComponentModel.DataAnnotations;

namespace ImpensaCore.Models;

public class GithubUser
{
    public Guid UserId { get; init; }
    [Key] public ulong GithubId { get; init; }
}
