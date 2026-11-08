using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Restaurant_Simulator.Data;
using Restaurant_Simulator.Models;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Restaurant_Simulator.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RestaurantAuthentication : ControllerBase
    {
        private readonly RestaurantContext dbc;

        public RestaurantAuthentication(RestaurantContext context)
        {
            dbc = context;
        }

        //Method Hashing Password
        private string PasswordHash(String password)
        {

            //Membuat objek algoritma SHA256.
            using var SHA = SHA256.Create();

            //mengubah string menjadi bytes array disimpan dalam variabel bytes
            var bytes = Encoding.UTF8.GetBytes(password);

            //menghasilkan hash dari variabel bytes 
            var hash = SHA.ComputeHash(bytes);

            //mengubah bytes array menjadi hexadecimal dan mengubah hexadecimal dalam bentuk kecil
            return Convert.ToHexString(hash).ToLower();
        }

        //Membuat atau generate Token JWT
        private string GenerateJwtToken(User user)
        {
            var Claims = new[]
            {
              new Claim("userId", user.UserId.ToString()),
              new Claim("email", user.Email),
              new Claim("role", user.Role)
             };


            //membuat kunci token dan mengubah key menjadi byte array
            var key = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes("iiiiiiiissssssss_kkkkkkkkeeeeeeey")
                );


            //menentukan kunci dan algoritma mendatangani Token
            var crendentials = new SigningCredentials(
                    key,SecurityAlgorithms.HmacSha256
                );

            //membuat objek JWT beserta CLaims dan masa berlaku
            var token = new JwtSecurityToken(claims: Claims,
                                            //menentukan masa berlaku  nya token selama 7 hari
                                            expires: DateTime.Now.AddDays(7),
                                            signingCredentials : crendentials
                );

            //mengelola proses konversi JWT dan mengubah objek JWT Menjadi String Token dan mengembalikan JWT agar bisa dikirim client
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        //Membuat endpoint login
        [HttpPost("Siging")]
        public IActionResult Login(LoginResult result)
        {
            var password = PasswordHash(result.Password);

            var email = dbc.Users.FirstOrDefault(u => u.Email == result.Email && u.PasswordHash == password);

            if (email == null)
            {
                return Unauthorized("Email Or Password Wrong");
            }

            var token = GenerateJwtToken(email);

            return Ok(new
            {
                message = "Login Berhasil ",
                token = token
            });

        }

        //Membuat endpoint untuk mengambil user
        [HttpGet("me")]
        [Authorize]
        public IActionResult Get()
        {
            var userId = User.FindFirstValue("userId");

            if (!int.TryParse(userId, out int id))
            {
                return Unauthorized();
            }
            var user = dbc.Users.Where(u => u.UserId == id).Select(u => new
            {
                userId = u.UserId,
                fullname = u.FullName,
                email = u.Email,
                role = u.Role
            }).FirstOrDefault();

            if (user == null)
            {
                return NotFound("User Tidak Ditemukan");
            }

            return Ok(new
            {
                data = user
            });
        }

        public class LoginResult
        {
            [Required] public string Email { get; set; }
            [Required] public string Password { get; set; }
        }
    }
}