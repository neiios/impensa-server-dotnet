using System.ComponentModel.DataAnnotations;

namespace Impensa.Models;

public class GithubUser
{
    public Guid UserId { get; init; }
    [Key] public ulong GithubId { get; init; }
}
