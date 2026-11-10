
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Restaurant_Simulator.Data;
using Restaurant_Simulator.Models;

namespace Restaurant_Simulator.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MenuController : ControllerBase
    {
        private readonly RestaurantContext dbc;

        public MenuController(RestaurantContext context)
        {
            dbc = context;
        }

        [HttpGet("GetAllMenus")]
        public async Task<IActionResult> GetAllMenus()
        {
            var menus = await dbc.Menus
                .Select(m => new
                {
                    m.MenuId,
                    m.CategoryId,
                    m.MenuName,
                    m.Description,
                    m.Price,
                    m.Stock,
                    m.ImageUrl,
                    m.IsAvailable
                })
                .ToListAsync();

            return Ok(menus);
        }

        [HttpGet("GetMenuById/{id}")]
        public async Task<IActionResult> GetMenuById(int id)
        {
            var menu = await dbc.Menus
                .Where(m => m.MenuId == id)
                .Select(m => new
                {
                    m.MenuId,
                    m.CategoryId,
                    m.MenuName,
                    m.Description,
                    m.Price,
                    m.Stock,
                    m.ImageUrl,
                    m.IsAvailable
                })
                .FirstOrDefaultAsync();

            if (menu == null)
                return NotFound("Menu Not Found");

            return Ok(menu);
        }

        [HttpPost("CreateMenu")]
        public async Task<IActionResult> CreateMenu(MenuRequest result)
        {
            // Check Category
            var category = await dbc.Categories
                .AnyAsync(c => c.CategoryId == result.CategoryId);

            if (!category)
                return NotFound("Category Not Found");

            // Validate Price
            if (result.Price < 0)
                return BadRequest("Price Cannot Be Negative");

            // Validate Stock
            if (result.Stock < 0)
                return BadRequest("Stock Cannot Be Negative");

            // Create Menu
            var menu = new Menu
            {
                CategoryId = result.CategoryId,
                MenuName = result.MenuName,
                Description = result.Description,
                Price = result.Price,
                Stock = result.Stock,
                ImageUrl = result.ImageUrl,
                IsAvailable = result.IsAvailable
            };

            dbc.Menus.Add(menu);

            await dbc.SaveChangesAsync();

            return Ok(new
            {
                message = "Success Create Menu",
                data = menu
            });
        }

        [HttpPut("UpdateMenu/{id}")]
        public async Task<IActionResult> UpdateMenu(
            int id,
            MenuRequest result)
        {
            var menu = await dbc.Menus
                .FirstOrDefaultAsync(m => m.MenuId == id);

            if (menu == null)
                return NotFound("Menu Not Found");

            // Check Category
            var category = await dbc.Categories
                .AnyAsync(c => c.CategoryId == result.CategoryId);

            if (!category)
                return NotFound("Category Not Found");

            // Validate Price
            if (result.Price < 0)
                return BadRequest("Price Cannot Be Negative");

            // Validate Stock
            if (result.Stock < 0)
                return BadRequest("Stock Cannot Be Negative");

            // Update Menu
            menu.CategoryId = result.CategoryId;
            menu.MenuName = result.MenuName;
            menu.Description = result.Description;
            menu.Price = result.Price;
            menu.Stock = result.Stock;
            menu.ImageUrl = result.ImageUrl;
            menu.IsAvailable = result.IsAvailable;

            await dbc.SaveChangesAsync();

            return Ok(new
            {
                message = "Success Update Menu",
                data = menu
            });
        }

        [HttpDelete("DeleteMenu/{id}")]
        public async Task<IActionResult> DeleteMenu(int id)
        {
            var menu = await dbc.Menus
                .FirstOrDefaultAsync(m => m.MenuId == id);

            if (menu == null)
                return NotFound("Menu Not Found");

            // Check Order Details
            var usedInOrder = await dbc.OrderDetails
                .AnyAsync(d => d.MenuId == id);

            if (usedInOrder)
                return Conflict("Menu Cannot Be Deleted Because It Is Used In Orders");

            dbc.Menus.Remove(menu);

            await dbc.SaveChangesAsync();

            return Ok(new
            {
                message = "Success Delete Menu"
            });
        }
    }

    public class MenuRequest
    {
        [Required]
        public int CategoryId { get; set; }

        [Required]
        public string MenuName { get; set; } = null!;

        public string? Description { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue)]
        public int Stock { get; set; }

        public string? ImageUrl { get; set; }

        public bool IsAvailable { get; set; } = true;
    }
}
