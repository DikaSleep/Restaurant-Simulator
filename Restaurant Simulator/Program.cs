using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Restaurant_Simulator.Data;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Reflection.Metadata;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

//Mengambil Connection Database 
builder.Services.AddDbContext<RestaurantContext>(Options =>
        Options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
    );

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();


//Set Up JWT Untuk Mengaktifkan autentiaksi menggunakan JWT Bearer
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(Options =>
{
    //mengatur aturan validasi JWT
    Options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,     //tidak memeriksa penerbit token
        ValidateAudience = false, //tidak memeriksa penerima token
        ValidateLifetime = true,  //memeriksa apakah token belum kadaluarsa
        ValidateIssuerSigningKey = true, //memeriksa keaslian tanda tangan token

        //secret key yang digunakan untuk validasi tanda tangan jwt
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes("iiiiiiiissssssss_kkkkkkkkeeeeeeey"))
    };

    // === Token BlackList===

    // Digunakan jika ingin memeriksa apakah JWT
    // sudah dibatalkan dan dimasukkan ke database.

    // Options.Events = new JwtBearerEvents
    // {
    //     // Event ini dijalankan setelah JWT berhasil
    //     // melewati validasi standar.
    //     OnTokenValidated = async context =>
    //     {
    //         // Mengambil RestaurantContext melalui Dependency Injection
    //         var dbcontext = context.HttpContext.RequestServices
    //             .GetRequiredService<RestaurantContext>();

    //         // Mengambil JWT dari Authorization Header
    //         var token = context.Request.Headers["Authorization"]
    //             .ToString()
    //             .Replace("Bearer ", "")
    //             .Trim();

    //         // Memeriksa apakah token ditemukan di tabel TokenBlacklists
    //         var isBlacklist = await dbcontext.TokenBlacklists
    //             .AnyAsync(t => t.Token == token);

    //         // Jika token ditemukan, tolak proses autentikasi
    //         if (isBlacklist)
    //         {
    //             context.Fail("Token Has Been Blacklisted");
    //         }
    //     }
    // };

    // Jika tidak menggunakan Token Blacklist,
    // biarkan seluruh bagian Events tetap dikomentari.
});

//Set up Swagger

//mendaftarkan API Expoler Agar endpoint  dapat dikenali oleh swagger
builder.Services.AddEndpointsApiExplorer();

//megaktifkan swagger generator untuk membuat dokumentasi API
builder.Services.AddSwaggerGen(Options =>
{
    //mendaftarkan konfigurasi autentikasi JWT Bearer
Options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            BearerFormat = "JWT",  //menentukan format token yang digunakan  untuk mengirimkan token
            Name = "Authorization", //menentukan nama http header untuk mengirimkan token
            Scheme = "Bearer", //Menentukan skema autentikasi yang digunakan 
            In = ParameterLocation.Header,  //menentukan lokasi pengiriman yang digunakan
            Type = SecuritySchemeType.Http,     //menentukan jenis autentikasi menggunakan http autentikasi
            Description = "MASUKIN TOKEN JWT NYA WOI"     //menampilkan instruksi pada bagian authorization di swagger UI
        });

    //mengatur security requirement dengan konfigurasi bearer
    Options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
        {
                {
            //menghubungkan security dengan bearer
                    new OpenApiSecuritySchemeReference("Bearer", document),
            //list kosong karena menggunakan jwt bearer, bukan autentikasi oAuth2
                    new List<string>()
                }

             });
        });


var app = builder.Build();

app.MapOpenApi();

// Configure the HTTP request pipeline.
app.UseSwaggerUI(Options =>
{
    Options.SwaggerEndpoint("/openapi/v1.json", "API v1");
});

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapGet("/", () => "API JALAN. COBA BUKA /Swagger");

app.Run();
