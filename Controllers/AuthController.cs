using System.Threading.Tasks;
using dotnet_rpg.Data;
using dotnet_rpg.Dtos.User;
using dotnet_rpg.Models;
using Microsoft.AspNetCore.Mvc;

namespace dotnet_rpg.Controllers
{
    [ApiController]
    [Route("[controller]")] 
    public class AuthController : ControllerBase
    {
        readonly IAuthRepository _authRepository;

        public AuthController(IAuthRepository authRepository)
        {
            _authRepository = authRepository;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register(UserRegisterDto request)
        {
            if (request.Password==null)
            {
                return BadRequest(new ServiceResponse<int> {Data = 0, Message = "missing password", Success = false});
            }
            ServiceResponse<int> response = await _authRepository.Register(
                new User {Username = request.Username}, request.Password);
            if (response.Success == true)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }   
        
        [HttpPost("Login")]
        public async Task<IActionResult> Login(UserLoginDto request)
        {
            if (request.Password==null)
            {
                return BadRequest(new ServiceResponse<string> {Message = "missing password", Success = false});
            }
            ServiceResponse<string> response = await _authRepository.Login(
                request.Username, request.Password);
            if (response.Success == true)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
    }
}