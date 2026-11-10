using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Restaurant_Simulator.Data;
using Restaurant_Simulator.Models;

namespace Restaurant_Simulator.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RestaurantTablesController : ControllerBase
    {
        private readonly RestaurantContext dbc;

        public RestaurantTablesController(RestaurantContext context)
        {
            dbc = context;
        }

        [HttpGet("GetTable")]
        public async Task<IActionResult> GetAllTable()
        {
            var tables = await dbc.RestaurantTables.Select(Options => new
            {
                tableid = Options.TableId,
                tablenumber = Options.TableNumber,
                capacity = Options.Capacity,
                status = Options.Status
            }).ToListAsync();

            return Ok(tables);
        }

        // get table id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetIdTables(int id)
        {
            var tables = await dbc.RestaurantTables.Where(r => r.TableId == id)
                         .Select(r => new
                         {
                             r.TableId,
                             r.TableNumber,
                             r.Capacity,
                             r.Status
                         }).FirstOrDefaultAsync();
            if (tables == null)
            {
                return NotFound("Tables Not Found");
            }

            return Ok(tables);
        }

        // EndPoint menambahkan Meja
        [HttpPost("CreateTable/")]
        public async Task<IActionResult> CreateTable(TablesRequest result)
        {
            // memeriksa jika kapasitas 0 user harus segera mengisi table
            if(result.Capacity <= 0)
            {
                return BadRequest("Go add table");
            }

            // memeriksa apakah no meja yang ingin ditambah kan ada didalam database
            var exit = await dbc.RestaurantTables.AnyAsync(t => t.TableNumber == result.TableNumber);

            // memeriksa jika meja sudah digunakan maka akan conflict
            if (exit)
            {
                return Conflict("Number table Used");
            }

            var tables = new RestaurantTable
            {
                TableNumber = result.TableNumber,
                Capacity = result.Capacity,
                Status = "Available"
            };

            dbc.RestaurantTables.Add(tables);

            await dbc.SaveChangesAsync();

            return Ok(new
            {
                message = "Succes Create Table",
                data = tables
            });

        }

        // update endpoint tables

        [HttpPut("UpdateTable/{id}")]
        public async Task<IActionResult> UpdateTable(int id, TablesRequest result)
        {
            var tables = await dbc.RestaurantTables.FindAsync(id);

            if(tables == null)
            {
                return NotFound("table Not found");
            }

            var number = await dbc.RestaurantTables.AnyAsync(r => r.TableNumber == result.TableNumber && r.TableId != id);

            if (number)
            {
                return Conflict("Table Number Used");
            }

            // validasi kapasitas
            if (result.Capacity <= 0)
            {
                return BadRequest("Capacity must be greater than 0");
            }

            // Membuat daftar status yang diperbolehkan
            var validstatus = new[]
            {
                "Available",
                "Reserved",
                "Occupied"
            };

            //Memeriksa status yang dikirim user
            if (!validstatus.Contains(result.Status))
            {
                return BadRequest("Invalid table Status");
            }


            // update table number
            tables.TableNumber = result.TableNumber;
            // update capacitiy
            tables.Capacity = result.Capacity;
            // update status 
            tables.Status = result.Status;


            await dbc.SaveChangesAsync();

            return Ok("Succes Update");
        }

        // endpoint delete
        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> DeletTable(int id)
        {
            var tables = await dbc.RestaurantTables.FindAsync(id);

            if(tables == null)
            {
                return NotFound("Table not Found");
            }

            dbc.RestaurantTables.Remove(tables);

            await dbc.SaveChangesAsync();

            return Ok("Tables Succes Delete");
        }


        // Dto Untuk Create dan update
        public class TablesRequest
        {
            [Required] public string TableNumber { get; set; }
            [Required] public int Capacity { get; set; }
            [Required] public string Status { get; set; }
        }
    }
}
