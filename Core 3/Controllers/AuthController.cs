using Core_3.Models;
using Core_3.Services;
using Microsoft.AspNetCore.Mvc;

namespace Core_3.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthServices _authServices;

        public AuthController(AuthServices authServices)
        {
            _authServices = authServices;
        }


        [HttpPost("register")]
        public async Task<IActionResult> Registrar([FromBody] RegistrarUsuarioDto dto)
        {
            try
            {
                var usuario = await _authServices.RegistrarUsuarioAsync(dto.Username, dto.Email, dto.Password);
                return Ok(usuario);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginUsuarioDto dto)
        {
            try
            {
                var usuario = await _authServices.LoginUsuarioAsync(dto.Email, dto.Password);
                return Ok(usuario);
            }
            catch (Exception ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [HttpPost("solicitar-recuperacion")]
        public async Task<IActionResult> SolicitarRecuperacion([FromBody] SolicitarRecuperacionDto dto)
        {
            try
            {
                var token = Guid.NewGuid().ToString();
                var tokenExpiry = DateTime.UtcNow.AddHours(1);
                await _authServices.SolicitarRecuperacionPasswordAsync(dto.Email, token, tokenExpiry);
                return Ok(new { token });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("resetear-password")]
        public async Task<IActionResult> ResetearPassword([FromBody] ResetearPasswordDto dto)
        {
            try
            {
                await _authServices.ResetearPasswordAsync(dto.Token, dto.NewPassword);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("by-valid-token")]
        public async Task<IActionResult> GetByValidToken([FromQuery] string token)
        {
            try
            {
                var usuario = await _authServices.GetUsuarioByValidTokenAsync(token);
                return Ok(usuario);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById( string id)
        {
            try
            {
                var usuario = await _authServices.GetUsuarioById(id);
                return Ok(usuario);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpPost("GetUsuarios")]
        public async Task<IActionResult> GetUsuarios()
        {
            try
            {
                var usuarios = await _authServices.GetUsuariosAsync();
                return Ok(usuarios);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message, stackTrace = ex.StackTrace });
            }
        }
    }
}
