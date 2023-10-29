namespace Impensa.Models;

public class UserLog
{
    public int Id { get; set; }
    public Guid UserId { get; set; }
    public DateTime Date { get; set; }
    public string IP { get; set; }
    public string Browser { get; set; }
    
    public virtual User User { get; set; }

}
