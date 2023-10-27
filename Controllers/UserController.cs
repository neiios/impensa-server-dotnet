using ImpensaServerDotnet.Models;
using Microsoft.AspNetCore.Mvc;

namespace ImpensaServerDotnet.Controllers;

[ApiController]
[Route("/api/v1/users")]
public class UserController : ControllerBase
{
    private static List<User> Users { get; } = new()
    {
        new User
        {
            Id = 1,
            Name = "John Doe",
            Email = "john@john.com",
            Password = "pass",
            Currency = "USD"
        },
        new User
        {
            Id = 2,
            Name = "Jane Doe",
            Email = "aaa",
            Password = "bbb",
            Currency = "EUR"
        }
    };
    
    [HttpGet]
    public ActionResult<IEnumerable<User>> GetUsers()
    {
        return Users;
    }
    
    [HttpGet("{id:int}")]
    public ActionResult<User> Get(int id)
    {
        var item = Users.FirstOrDefault(i => i.Id == id);
        if (item == null) return new NotFoundResult();
        return item;
    }

    [HttpPost]
    public ActionResult<User> Post(User item)
    {
        Users.Add(item);
        return CreatedAtAction(nameof(Get), new { id = item.Id }, item);
    }
}
