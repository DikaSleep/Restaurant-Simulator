using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Restaurant_Simulator.Data;
using Restaurant_Simulator.Models;

namespace Restaurant_Simulator.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly RestaurantContext dbc;

        public PaymentsController(RestaurantContext context)
        {
            dbc = context;
        }

        // mengambil semua pemabayaran
        [HttpGet("GetAllPayments")]
        public async Task<IActionResult> GetAllPayments()
        {
            var payments = await dbc.Payments
                .Select(p => new
                {
                    p.PaymentId,
                    p.OrderId,
                    p.PaymentMethod,
                    p.Amount,
                    p.PaymentDate,
                    p.Status,
                    p.ReferenceNumber
                })
                .ToListAsync();

            return Ok(payments);
        }

        // mengambil pembayaran berupa id
        [HttpGet("GetPaymentById/{id}")]
        public async Task<IActionResult> GetPaymentById(int id)
        {
            var payment = await dbc.Payments
                .Where(p => p.PaymentId == id)
                .Select(p => new
                {
                    p.PaymentId,
                    p.OrderId,
                    p.PaymentMethod,
                    p.Amount,
                    p.PaymentDate,
                    p.Status,
                    p.ReferenceNumber
                })
                .FirstOrDefaultAsync();

            if (payment == null)
                return NotFound("Payment Not Found");

            return Ok(payment);
        }
        
        // mengambil pembayarn berdasarkan orderid
        [HttpGet("GetPaymentByOrderId/{orderId}")]
        public async Task<IActionResult> GetPaymentByOrderId(int orderId)
        {
            var payment = await dbc.Payments
                .Where(p => p.OrderId == orderId)
                .Select(p => new
                {
                    p.PaymentId,
                    p.OrderId,
                    p.PaymentMethod,
                    p.Amount,
                    p.PaymentDate,
                    p.Status,
                    p.ReferenceNumber
                })
                .FirstOrDefaultAsync();

            if (payment == null)
                return NotFound("Payment Not Found");

            return Ok(payment);
        }

        // endpoint membuat pembayaran
        [HttpPost("CreatePayment")]
        public async Task<IActionResult> CreatePayment(PaymentRequest result)
        {
            // Check Order
            var order = await dbc.Orders
                .FirstOrDefaultAsync(o => o.OrderId == result.OrderId);

            if (order == null)
                return NotFound("Order Not Found");

            // Check Order Amount
            if (order.TotalAmount <= 0)
                return BadRequest("Order Amount Must Be Greater Than 0");

            // Check Existing Payment
            var existingPayment = await dbc.Payments
                .AnyAsync(p => p.OrderId == result.OrderId);

            if (existingPayment)
                return Conflict("Payment Already Exists");

            // Validate Payment Method
            var validMethod = new[]
            {
                "Cash",
                "DebitCard",
                "CreditCard",
                "EWallet"
            };

            if (!validMethod.Contains(result.PaymentMethod))
                return BadRequest("Invalid Payment Method");

            // Create Payment
            var payment = new Payment
            {
                OrderId = result.OrderId,
                PaymentMethod = result.PaymentMethod,
                Amount = order.TotalAmount,
                PaymentDate = DateTime.Now,
                Status = "Pending",
                ReferenceNumber = result.ReferenceNumber
            };

            dbc.Payments.Add(payment);

            await dbc.SaveChangesAsync();

            return Ok(new
            {
                message = "Success Create Payment",
                data = payment
            });
        }

        // update
        [HttpPut("UpdatePaymentStatus/{id}")]
        public async Task<IActionResult> UpdatePaymentStatus(
            int id,
            PaymentStatusRequest result)
        {
            var payment = await dbc.Payments
                .FirstOrDefaultAsync(p => p.PaymentId == id);

            if (payment == null)
                return NotFound("Payment Not Found");

            // Validate Payment Status
            var validStatus = new[]
            {
                "Pending",
                "Paid",
                "Failed",
                "Refunded"
            };

            if (!validStatus.Contains(result.Status))
                return BadRequest("Invalid Payment Status");

            // Update Payment Status
            payment.Status = result.Status;

            await dbc.SaveChangesAsync();

            return Ok(new
            {
                message = "Success Update Payment Status",
                data = payment
            });
        }
    }
     
    // dto
    public class PaymentRequest
    {
        [Required]
        public int OrderId { get; set; }

        [Required]
        public string PaymentMethod { get; set; } = null!;

        public string? ReferenceNumber { get; set; }
    }

    public class PaymentStatusRequest
    {
        [Required]
        public string Status { get; set; } = null!;
    }
}
