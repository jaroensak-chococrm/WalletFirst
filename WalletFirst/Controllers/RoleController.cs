using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WalletFirst.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WalletFirst.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly WalletDBContext _context;

        public RoleController (WalletDBContext context/*, IOptions<JWTSettings> jwtsettings*/)
        {
            _context = context;
            // _jwtsettings = jwtsettings.Value;
        }
        // GET: api/Role
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Role>>> GetRoles ()
        {
            return await _context.Roles.ToListAsync();
        }

        // GET: api/Role/Role/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Role>> GetRole (int id)
        {
            var role = await _context.Roles.FindAsync(id);

            if (role == null)
            {
                return NotFound();
            }

            return role;
        }

        // POST: api/Role/AddRole
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost("AddRole")]
        public async Task<ActionResult<Role>> AddRole (Role role)
        {

            var addRole = new Role
            {
                RoleName = role.RoleName,
                Description = role.Description,
                TimeCreate = DateTime.Now
            };

            _context.Roles.Add(addRole); ;
            await _context.SaveChangesAsync();

            //return CreatedAtAction("GetTodoItem", new { id = todoItem.Id }, todoItem);
            return CreatedAtAction("GetRole", new { id = addRole.RoleId }, addRole);

        }


        // PUT: api/Role/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("Update/{id}")]
        public async Task<IActionResult> UpdateRole (int id, Role role)
        {
            if (id != role.RoleId)
            {
                return BadRequest();
            }

            // _context.Entry(todoItem).State = EntityState.Modified;
            var rol = await _context.Roles.FindAsync(id);
            if (rol == null)
            {
                return NotFound();
            }

            rol.Description = role.Description;
            rol.RoleName = role.RoleName;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException) when (!RoleExist(id))
            {
                return NotFound();
            }

            return NoContent();
        }

        // DELETE: api/Role/DelRole/5
        [HttpDelete("DelRole/{id}")]
        public async Task<IActionResult> DeleteRole (int id)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        private bool RoleExist (int id)
        {
            return _context.Roles.Any(e => e.RoleId == id);
        }
    }
}

