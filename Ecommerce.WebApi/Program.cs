using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IO;
using Microsoft.AspNetCore.Mvc.Formatters;
using SolidEdu.Share;
using static System.Console;
using Ecommerce.WebApi.Repositories;
using Ecommerce.IdentityJWT.Authentication;



var builder = WebApplication.CreateBuilder(args);

/*<integerated authentiacate*/
ConfigurationManager configuration = builder.Configuration;//read all information configuration
//B1: Create connection to DB via EF Core
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(configuration.GetConnectionString("ConnStrSQLServerDB")));
//B2: For Identity_Tao ra cac identity user va identity role save in Db
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();
//B3: Add authentication 
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidAudience = configuration["JWT:ValidAudience"],
        ValidIssuer = configuration["JWT:ValidIssuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Secret"]))
    };
});
/*</integerated authentiacate*/


//Enable CORS to prtected APIs 
builder.Services.AddCors();

// Add services to the container.(connect to sql server)
builder.Services.AddEcommerceContext();

builder.Services.AddControllers(
    options =>
    {
        WriteLine("Default output formatters:");
        foreach(IOutputFormatter formatter in options.OutputFormatters)
        {
            OutputFormatter? mediaFormatter = formatter as OutputFormatter;
            if (mediaFormatter == null)
            {
                WriteLine($"{formatter.GetType().Name}");
            }
            else
            {
                WriteLine("{0}, Media Type:{1}",
                arg0: mediaFormatter.GetType().Name,
                arg1: string.Join(", ",mediaFormatter.SupportedMediaTypes));
            }
        }
    })
    .AddXmlDataContractSerializerFormatters()
    .AddXmlSerializerFormatters();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();//inversion of control (IoC)

var app = builder.Build();

//Use Cros-origin
app.UseCors(configurePolicy : options =>
{
    options.WithMethods("GET", "POST", "PUT", "DELETE");//allow method 
    options.WithOrigins("https://localhost:5002");//allow requests from the client (diff domain)
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//Authentication
app.UseAuthentication();

app.UseAuthorization();

//Security header https
app.UseMiddleware<SecurityHeaders>();

app.MapControllers();

app.Run();
