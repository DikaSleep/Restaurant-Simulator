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
    [Authorize(Roles = "admin")]
    public class UserController : ControllerBase
    {
        // mengambil database Context
        private readonly RestaurantContext dbc;

        public UserController(RestaurantContext context)
        {
            dbc = context;
        }

        //Endpoitn Untuk Mengambil DAta User
        [HttpGet("Me")]
        public async Task<IActionResult> GetAll()
        {
            var user = await dbc.Users.Select(Options => new
            {
                userid = Options.UserId,
                fullname = Options.FullName,
                email = Options.Email,
                role = Options.Role,
                createdAt = Options.CreatedAt
                // To list digunakan untuk mengambil semua data yang memenuhi kondisi query dan menyimpan dalam bentuk list
            }).ToListAsync();

            return Ok(User);
        }

        // endpoint detail user 
        [HttpGet("Id")]
        public async Task<IActionResult> GetId(int id)
        {

            var user = await dbc.Users.Where(u => u.UserId == id)
                .Select(u => new
                {
                    u.UserId,
                    u.FullName,
                    u.Email,
                    u.Role,
                    u.CreatedAt

                }).FirstOrDefaultAsync();


            if(user == null)
            {
                return NotFound("User Id Not Found");
            }

            return Ok(user);
        }

        // Memperbarui 
        [HttpPut("Update-Data")]
        public async Task<IActionResult> Update(int id, UpdateUser result)
        {

            // Untuk Mengecek Apakah User Id ada atau tidak ada 
            var user = await dbc.Users.FindAsync(id);

            // Jika User Tidak ada maka akan melakukan validasi
            if (user == null)
            {
                return NotFound("User Not Found");
            }

            // Mengecek apakah email sudah dipakai dan mengecek 
            var EmailExist = await dbc.Users.AnyAsync(u => u.Email == result.Email && u.UserId != id);

            if (EmailExist == null)
            {
                return Conflict("Email Already Used");
            }
            
            // Update FullName
            user.FullName = result.FullName;

            // Update Email
            user.Email = result.Email;

            // Update Password
            if (!string.IsNullOrEmpty(result.Password))
            {
                // Membuat Objek Baru yang bertugas melakukan Proses Hash Untuk Models USer
                var hash = new PasswordHasher<User>();

                // method unutuk mengubah password menjadi hash password baru yang dikirim melalui swagger dan menyimpan hasil ke properti pengguna 
                user.PasswordHash = hash.HashPassword(user, result.Password);
            }

            // Menyimpan dalam Database
            await dbc.SaveChangesAsync();

            return Ok("Update Succes");
        }

        // EndPoint Delete User berdasarkan Id

        [HttpDelete("Delete")]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await dbc.Users.FindAsync(id);

            if(user == null)
            {
                return NotFound("User Not Found");
            }

            dbc.Users.Remove(user);

            await dbc.SaveChangesAsync();

            return Ok("Delete succes User");
        }


        // DTO Untuk Update Data
        public class UpdateUser
        {
            [Required] public string FullName { get; set; }

            [Required] public string Email { get; set; }

            [Required] public string Password { get; set; }
        };
    }
}
