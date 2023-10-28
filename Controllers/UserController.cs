using Impensa.Models;
using Impensa.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Impensa.Controllers;

[ApiController]
[Route("/api/v1/users")]
public class UserController : ControllerBase
{
    private readonly AppDbContext _context;

    public UserController(AppDbContext context)
    {
        _context = context;
    }

    // TODO: this endpoint makes no sense and should be removed later
    [HttpGet]
    public List<User> GetUsers()
    {
        return _context.Users.ToList();
    }
}