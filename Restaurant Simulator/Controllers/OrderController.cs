using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Restaurant_Simulator.Data;
using Restaurant_Simulator.Models;

namespace Restaurant_Simulator.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly RestaurantContext dbc;

        public OrderController(RestaurantContext context)
        {
            dbc = context;
        }

        [HttpGet("GetOrder")]
        public async Task<IActionResult> GetTable()
        {
            var order = await dbc.Orders.Select(Option => new
                {
                    Option.OrderId,
                    Option.UserId,
                    Option.EmployeeId,
                    Option.TableId,
                    Option.OrderDate,
                    Option.Status,
                    Option.TotalAmount,
                    Option.Employee,
                    Option.Payment,
                    Option.OrderDetails,
                    Option.Table,
                    Option.User
            }).ToListAsync();

            return Ok(order);
        }

        // Endpoint Mengambil order berupa id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetIdOrder(int id)
        {
            var order = await dbc.Orders.Where(o => o.OrderId == id).Select(o => new
            {
                o.OrderId,
                o.UserId,
                o.EmployeeId,
                o.TableId,
                o.OrderDate,
                o.Status,
                o.TotalAmount,
            }).FirstOrDefaultAsync();

            if(order == null)
            {
                return NotFound("Order Not found");
            }

            return Ok(order);
        }

        // EndPoint menambahkan Order
        [HttpPost("CreateOrder")]
        public async Task<IActionResult> CreateOrder(OrderRequest result)
        {
            var user = await dbc.Orders.AnyAsync(u => u.UserId == result.UserId);

            if(user == null)
            {
                return NotFound("User Not Found");
            }

            var Table = await dbc.RestaurantTables.FirstOrDefaultAsync(t => t.TableId == result.TableId);

            if(Table == null)
            {
                return NotFound("Table not Found");
            }

            if(Table.Status != "Available")
            {
                return Conflict("Table is not Available");
            }

            var Employee = await dbc.Employees.AnyAsync(e => e.EmployeeId == result.EmployeeId);

            if(Employee == null)
            {
                return NotFound("Employee Not Found");
            }

            var order = new Order
            {
                UserId = result.UserId,
                TableId = result.TableId,
                EmployeeId = result.EmployeeId,
                OrderDate = DateTime.Now,
                Status = "Pending",
                TotalAmount = 0,
                Notes = result.Note
            };

            Table.Status = "Occupied";

            dbc.Orders.Add(order);

            await dbc.SaveChangesAsync();

            return Ok(new
            {
                message = "Succes Add order",
                data = order
            });
        }

        // endpoint Update Order (Status)
        [HttpPut("Update/{id}")]
        public async Task<IActionResult> UpdateOrderStatus(int id, OrderRequest result)
        {
            var order = await dbc.Orders
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
                return NotFound("Order Not Found");

            // Valid Order Status
            var validStatus = new[]
            {
                "Pending",
                "Preparing",
                "Ready",
                "Completed",
                "Cancelled"
            };

            if (!validStatus.Contains(result.Status))
                return BadRequest("Invalid Order Status");

            // Update Order Status
            order.Status = result.Status;

            // Release Table When Order Completed Or Cancelled
            if (result.Status == "Completed" ||
                result.Status == "Cancelled")
            {
                var activeOrder = await dbc.Orders.AnyAsync(o =>
                    o.TableId == order.TableId &&
                    o.OrderId != id &&
                    (o.Status == "Pending" ||
                     o.Status == "Preparing" ||
                     o.Status == "Ready"));

                if (!activeOrder)
                {
                    var table = await dbc.RestaurantTables
                        .FindAsync(order.TableId);

                    if (table != null)
                        table.Status = "Available";
                }
            }

            await dbc.SaveChangesAsync();

            return Ok(new
            {
                message = "Success Update Order Status",
                data = order
            });
        }


        // Dto Createa Orderan
        public class OrderRequest
        {
            [Required] public  int UserId { get; set; }
            [Required] public  int EmployeeId { get; set; }
            [Required] public int TableId { get; set; }
            [Required] public string Status { get; set; }
            [Required] public string Note { get; set; }
        }
    }
}
