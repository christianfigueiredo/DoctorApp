using Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models.dtos;
using Models.Entidades;
using System.Security.Cryptography;

namespace API.Controllers
{
   
    public class UsuarioController : BaseApiController
    {
        private readonly ApplicationDbContext _db;

        public UsuarioController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Usuario>>> GetUsuarios()
        {
            return Ok(await _db.Usuarios.ToListAsync());

        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Usuario>> GetUsuario(int id)
        {
            var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Id == id);
            if (usuario == null)
            {
                return NotFound();
            }
            return Ok(usuario);
        }

        [HttpPost("registro")]
        public async Task<ActionResult<Usuario>> RegistrarUsuario(RegistroDto registroDto)
        {
            if(await UsuarioExiste(registroDto.Username))
            {
                return BadRequest("O Nome do usuário já existe");
            }

            using var hmac = new HMACSHA512();
            var usuario = new Usuario
            {
                Username = registroDto.Username.ToLower(),
                PasswordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(registroDto.Password)),
                PasswordSalt = hmac.Key
            };

            _db.Usuarios.Add(usuario);
            await _db.SaveChangesAsync();

            return Ok(usuario);
        }

        private async Task<bool> UsuarioExiste(string username)
        {
            return await _db.Usuarios.AnyAsync(u => u.Username == username.ToLower());
        }
    }
}
