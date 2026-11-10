using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Restaurant_Simulator.Data;
using Restaurant_Simulator.Models;

namespace Restaurant_Simulator.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderDetailsController : ControllerBase
    {
        private readonly RestaurantContext dbc;

        public OrderDetailsController(RestaurantContext context)
        {
            dbc = context;
        }

        // menambil semua order detail
        [HttpGet("GetAllOrderDetails")]
        public async Task<IActionResult> GetAllOrderDetails()
        {
            var details = await dbc.OrderDetails
                .Select(d => new
                {
                    d.OrderDetailId,
                    d.OrderId,
                    d.MenuId,
                    d.Quantity,
                    d.UnitPrice,
                    d.Subtotal
                })
                .ToListAsync();

            return Ok(details);
        }

        // mengambil order detail dari order id
        [HttpGet("GetByOrderId/{orderId}")]
        public async Task<IActionResult> GetByOrderId(int orderId)
        {
            var details = await dbc.OrderDetails
                .Where(d => d.OrderId == orderId)
                .Select(d => new
                {
                    d.OrderDetailId,
                    d.OrderId,
                    d.MenuId,
                    d.Quantity,
                    d.UnitPrice,
                    d.Subtotal
                })
                .ToListAsync();

            if (details.Count == 0)
                return NotFound("Order Details Not Found");

            return Ok(details);
        }

        // mengambil semua orderdetail berdasarkan id
        [HttpGet("GetById/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var detail = await dbc.OrderDetails
                .Where(d => d.OrderDetailId == id)
                .Select(d => new
                {
                    d.OrderDetailId,
                    d.OrderId,
                    d.MenuId,
                    d.Quantity,
                    d.UnitPrice,
                    d.Subtotal
                })
                .FirstOrDefaultAsync();

            if (detail == null)
                return NotFound("Order Detail Not Found");

            return Ok(detail);
        }

        // create order detail
        [HttpPost("CreateOrderDetail")]
        public async Task<IActionResult> CreateOrderDetail(
            OrderDetailRequest result)
        {
            // Check Order
            var order = await dbc.Orders
                .FirstOrDefaultAsync(o => o.OrderId == result.OrderId);

            if (order == null)
                return NotFound("Order Not Found");

            if (order.Status != "Pending")
                return BadRequest("Only Pending Orders Can Be Updated");

            // Check Menu
            var menu = await dbc.Menus
                .FirstOrDefaultAsync(m => m.MenuId == result.MenuId);

            if (menu == null)
                return NotFound("Menu Not Found");

            if (!menu.IsAvailable)
                return BadRequest("Menu Is Not Available");

            // Create Order Detail
            var detail = new OrderDetail
            {
                OrderId = result.OrderId,
                MenuId = result.MenuId,
                Quantity = result.Quantity,
                UnitPrice = menu.Price
            };

            dbc.OrderDetails.Add(detail);

            await dbc.SaveChangesAsync();

            // Calculate Total Amount
            order.TotalAmount = await dbc.OrderDetails
                .Where(d => d.OrderId == result.OrderId)
                .SumAsync(d => d.Quantity * d.UnitPrice);

            await dbc.SaveChangesAsync();

            return Ok(new
            {
                message = "Success Create Order Detail",
                data = detail
            });
        }

        // endpoint Upate Ordertail
        [HttpPut("UpdateOrderDetail/{id}")]
        public async Task<IActionResult> UpdateOrderDetail(
            int id,
            OrderDetailUpdateRequest result)
        {
            var detail = await dbc.OrderDetails
                .FirstOrDefaultAsync(d => d.OrderDetailId == id);

            if (detail == null)
                return NotFound("Order Detail Not Found");

            var order = await dbc.Orders
                .FirstOrDefaultAsync(o => o.OrderId == detail.OrderId);

            if (order == null)
                return NotFound("Order Not Found");

            if (order.Status != "Pending")
                return BadRequest("Only Pending Orders Can Be Updated");

            // Update Quantity
            detail.Quantity = result.Quantity;

            await dbc.SaveChangesAsync();

            // Recalculate Total Amount
            order.TotalAmount = await dbc.OrderDetails
                .Where(d => d.OrderId == detail.OrderId)
                .SumAsync(d => d.Quantity * d.UnitPrice);

            await dbc.SaveChangesAsync();

            return Ok(new
            {
                message = "Success Update Order Detail",
                data = detail
            });
        }

        // ==========================================
        // 6. DELETE ORDER DETAIL
        // ==========================================
        [HttpDelete("DeleteOrderDetail/{id}")]
        public async Task<IActionResult> DeleteOrderDetail(int id)
        {
            var detail = await dbc.OrderDetails
                .FirstOrDefaultAsync(d => d.OrderDetailId == id);

            if (detail == null)
                return NotFound("Order Detail Not Found");

            var order = await dbc.Orders
                .FirstOrDefaultAsync(o => o.OrderId == detail.OrderId);

            if (order == null)
                return NotFound("Order Not Found");

            if (order.Status != "Pending")
                return BadRequest("Only Pending Orders Can Be Updated");

            int orderId = detail.OrderId;

            dbc.OrderDetails.Remove(detail);

            await dbc.SaveChangesAsync();

            // Recalculate Total Amount
            order.TotalAmount = await dbc.OrderDetails
                .Where(d => d.OrderId == orderId)
                .SumAsync(d => d.Quantity * d.UnitPrice);

            await dbc.SaveChangesAsync();

            return Ok(new
            {
                message = "Success Delete Order Detail"
            });
        }
    }

    public class OrderDetailRequest
    {
        [Required]
        public int OrderId { get; set; }

        [Required] public int MenuId { get; set; }

        [Required] public int Quantity { get; set; }
    }

    public class OrderDetailUpdateRequest
    {
       [Required] public int Quantity { get; set; }
    }
}
