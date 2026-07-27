using Microsoft.EntityFrameworkCore;
using ElectroComAPI.Data;

var builder = WebApplication.CreateBuilder(args);

// Database connection
var conn = builder.Configuration.GetConnectionString("ElectroComDB");
builder.Services.AddDbContext<ElectroComDBContext>(options => 
{
    options.UseSqlServer(conn);
    options.ConfigureWarnings(warnings => warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
});

// Repository services
builder.Services.AddScoped<ProductRepo>();
builder.Services.AddScoped<QuotationRepo>();

// AutoMapper
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// CORS configuration
var corsOrigins = new List<string>
{
    "http://localhost:7234",
    "https://localhost:7234",
    "http://localhost:7091",
    "https://localhost:7091",
    "http://gadgethub.runasp.net",
    "https://gadgethub.runasp.net",
    "http://gadgethub-gadgethub.runasp.net",
    "https://gadgethub-gadgethub.runasp.net"
};
corsOrigins.AddRange(builder.Configuration.GetSection("CorsOrigins").Get<string[]>() ?? Array.Empty<string>());

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowGadgetHub", policy =>
    {
        policy.WithOrigins(corsOrigins.Distinct().ToArray())
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials()
              .SetIsOriginAllowedToAllowWildcardSubdomains();
    });
});

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();


// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Use CORS - Must be before Authorization
app.UseCors("AllowGadgetHub");

app.UseAuthorization();
app.MapControllers();

app.Run();