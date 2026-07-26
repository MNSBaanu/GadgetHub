using Microsoft.EntityFrameworkCore;
using GadgetCentralAPI.Data;

var builder = WebApplication.CreateBuilder(args);

// Database connection
var conn = builder.Configuration.GetConnectionString("GadgetCentralDB");
builder.Services.AddDbContext<GadgetCentralDBContext>(options => options.UseSqlServer(conn));

// Repository services
builder.Services.AddScoped<ProductRepo>();
builder.Services.AddScoped<QuotationRepo>();

// AutoMapper
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// CORS configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowGadgetHub", policy =>
    {
        policy.WithOrigins(
                "https://localhost:7234", "http://localhost:7234",
                "https://localhost:7091", "http://localhost:7091",
                "https://gadgethub.runasp.net", "http://gadgethub.runasp.net")
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

// Use CORS
app.UseCors("AllowGadgetHub");

app.UseAuthorization();
app.MapControllers();

app.Run();
