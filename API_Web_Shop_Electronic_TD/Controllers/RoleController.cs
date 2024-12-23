using API_Web_Shop_Electronic_TD.Data;
using API_Web_Shop_Electronic_TD.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API_Web_Shop_Electronic_TD.Controllers
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	public class RoleController : Controller
	{
		private readonly Hshop2023Context db;

		public RoleController(Hshop2023Context db)
		{
			this.db = db;

		}
		[HttpPost]
		public async Task<IActionResult> CreateRole([FromBody] RoleMD createRole)
		{
			if (string.IsNullOrEmpty(createRole.roleName))
			{
				return BadRequest("Role name is required");
			}
			var roleExit = await db.Roles.FirstOrDefaultAsync(r => r.RoleName == createRole.roleName);
			if (roleExit != null)
			{
				return BadRequest("Role already exist");

			}
			var role = new Role
			{
				RoleName = createRole.roleName,
				Description = createRole.description,
			};
			db.Roles.Add(role);
			await db.SaveChangesAsync();
			return Ok();
		}

		[HttpGet]
		public async Task<IActionResult> GetRoles()
		{
			var role = await db.Roles.Select(r => new RoleMD
			{
				id = r.Id,
				roleName = r.RoleName,
				description = r.Description
			}).ToListAsync();
			return Ok(role);
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> deleteRole(int id)
		{
			var role = await db.Roles.FirstOrDefaultAsync(r => r.Id == id);
			if (role == null)
			{
				return NotFound("Role not found");
			}
			db.Roles.Remove(role);
			await db.SaveChangesAsync();
			return Ok("Delete success!");
		}
	}
}
