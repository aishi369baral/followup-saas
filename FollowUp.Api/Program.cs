using FollowUp.Api.Auth;
using FollowUp.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models; 


//Create the builder
//Loads the appsettings.json, env vars, secrets
//Sets up logging
//Prepares the DI Container
//This is the service configuration phase.
var builder = WebApplication.CreateBuilder(args);

// Add services to the Dependency Injection(DI) container.

/*
 Registers: Controller discovery, 
model binding, validation, Json handling, without this 
[ApiController] classes wont work.
 */
builder.Services.AddControllers();                                           //dependency injection

/*
 Registers: DbContext
reads ConnectionString from config
Configures EF Core to use SQLite
Registers AppDbContext in DI
 */
builder.Services.AddDbContext<AppDbContext>(options =>                       //dependency injection
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection")));


/*
 Registers: JwtService
 */
builder.Services.AddScoped<JwtService>();                                     //dependency injection


/*
 Configure Authentication system
Sets the default authentication scheme to JWT Bearer.
So authorize will expect a JWT.
 */
builder.Services.AddAuthentication("Bearer")                                  //dependency injection
    .AddJwtBearer("Bearer", options =>           // Registers middleware that reads token from Authorization: Bearer, validates it, creates HttpContext.User
    {
        options.TokenValidationParameters = new()   //Token validation, defines what makes a token valid.
        {
            ValidateIssuer = true,                // token must come from your configured issuer.
            ValidateAudience = true,          //token must be intended for your API.
            ValidateLifetime = true,         // reject expired token
            ValidateIssuerSigningKey = true,   // verify digital signature, prevents tampering
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"], // both validIssuer and ValidAudience must match values when generating the token
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)    //uses your secret key to verify token signature.
            )
        };
    });


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

//gives you swagger UI to test endpoints.
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter: Bearer {your JWT token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});


//Service configuration phase ends
//Middleware pipeline configuration begins

var app = builder.Build();


//for Development-only use swagger

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


//http redirection 
//redirects Http -> Https, security best practice
app.UseHttpsRedirection();

//AUTH MUST COME BEFORE AUTHORIZATION
//Authentication middleware
app.UseAuthentication();

//Authorization middleware
app.UseAuthorization();


//Map Controllers
//Enables attribute routing [HttpPost("Login")]
//Without this -> endpoints dont exist.
app.MapControllers();


//Starts the web server and begins listening for requests
app.Run();
