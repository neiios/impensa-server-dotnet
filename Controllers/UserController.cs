using Impensa.Models;
using Impensa.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Impensa.Controllers;

[ApiController]
[Route("/api/v1/users")]
public class UserController : ControllerBase
{

    private readonly IUserRepository _userRepository;

    public UserController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    [HttpGet]
    public List<User> GetUsers()
    {
        return _userRepository.GetAll();
    }
}