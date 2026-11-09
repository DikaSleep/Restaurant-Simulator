using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Restaurant_Simulator.Data;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Identity;
using Restaurant_Simulator.Models;


namespace Restaurant_Simulator.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class EmployeesController : ControllerBase
    {
        private readonly RestaurantContext dbc;

        public EmployeesController(RestaurantContext context)
        {
            dbc = context;
        }

        // Mengambil Semua data karyawan/Employess
        [HttpGet("Employees")]
        public async Task<IActionResult> GetAll()
        {
            var Employees = await dbc.Employees.Select(Options => new
            {
                employeeid = Options.EmployeeId,
                fullname = Options.FullName,
                email = Options.Email,
                phone = Options.PhoneNumber,
                postion = Options.Position,
                salary = Options.Salary,
                hiredate = Options.HireDate,
                active = Options.IsActive

            }).ToListAsync();

            return Ok(Employees);
        }

        // endpoint detail karyawan
        [HttpGet("Id")]
        public async Task<IActionResult> GetId(int id)
        {
            var Employees = await dbc.Employees.Where(e => e.EmployeeId == id)
                            .Select(e => new
                            {
                                e.EmployeeId,
                                e.FullName,
                                e.Email,
                                e.PhoneNumber,
                                e.Position,
                                e.Salary,
                                e.HireDate,
                                e.IsActive

                            }).FirstOrDefaultAsync();

            if(Employees == null)
            {
                return NotFound("Employees Not Found");
            }

            return Ok(Employees);
        }

        // Menambahkan Karyawan

        [HttpPost("Add-Employee")]
        public async Task<IActionResult> CreateEmployee(Create result)
        {
            // membuat objek karyawan baru
            var employee = new Employee
            {
                FullName = result.FullName,
                Email = result.Email,
                PhoneNumber = result.PhoneNumber,
                Position = result.Position,
                Salary = result.Salary,
                HireDate = result.HireDate,
                IsActive = result.IsActive
            };

            // menambahkan karyawan ke database
            dbc.Employees.Add(employee);

            // Menganti perubahan Database
            await dbc.SaveChangesAsync();

            // Return Ok http 200 Berhasil dan memberikan message succses create Employees
            return Ok(new
            {
                message = "Succes Create Employees",
                data = employee
            });

        }

        // Update Karyawan endpoint
        [HttpPut("Update-Employee")]
        public async Task<IActionResult> UpdateEmployee(int id,Create result)
        {
            // memeriksa apakah id dari karyawan ada
            var employee = await dbc.Employees.FindAsync(id);

                if(employee == null)
                {
                    return NotFound("Employee Not Found");
                }

                // Untuk Mengecek apakah email sudah digunakan atau belum
                var EmailExits = await dbc.Employees.AnyAsync(e => e.Email == result.Email && e.EmployeeId != id);

                if (EmailExits == null)
                {
                    return Conflict("Email Already Used");
                }

                // memperbarui atau update (Fullname, email, PhoenNumber, Position, Salary, hiredate, isactive)
                employee.FullName = result.FullName;
                employee.Email = result.Email;
                employee.PhoneNumber = result.PhoneNumber;
                employee.Position = result.Position;
                employee.Salary = result.Salary;
                employee.HireDate = result.HireDate;
                employee.IsActive = result.IsActive;

                // perubahan pada database
                await dbc.SaveChangesAsync();

                return Ok("Update Succes");

            }

        // endpoint untuk menghapus employee
        [HttpDelete("Delete")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            var employee = await dbc.Employees.FindAsync(id);

            if(employee == null)
            {
                return NotFound("Employee not Found");
            }

            // menghapus karyawan dari database
            dbc.Employees.Remove(employee);

            // perubahan pada database
            await dbc.SaveChangesAsync();

            return Ok("Employee Has Delete");

           
        }
        }

        // Dto Untuk Pembuatan Karyawan Baru

        public class Create
        {
            [Required] public string FullName { get; set; }
            [Required] public string Email { get; set; }
            [Required] public string PhoneNumber { get; set; }
            [Required] public string Position { get; set; }
            [Required] public decimal Salary { get; set; }
            [Required] public DateOnly HireDate { get; set; }
            [Required] public bool IsActive { get; set; }
        }
    }
}
