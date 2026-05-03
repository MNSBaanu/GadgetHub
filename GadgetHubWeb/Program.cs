using GadgetHubWeb.Data;
using GadgetHubWeb.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllers();

// Add CORS services
builder.Services.AddCors();

// Add database context - use existing GadgetHubDBContext
builder.Services.AddDbContext<GadgetHubDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("GadgetHubDB")));

// Add HttpClient for API calls
builder.Services.AddHttpClient();

// Add AuthService
builder.Services.AddScoped<AuthService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Add CORS support
app.UseCors(builder => builder
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

// Initialize database - SIMPLE APPROACH
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<GadgetHubDBContext>();
    try
    {
        Console.WriteLine("🔍 Testing database connection...");
        var canConnect = await context.Database.CanConnectAsync();
        Console.WriteLine($"Database connection: {(canConnect ? "✅ Success" : "❌ Failed")}");
        
        if (canConnect)
        {
            Console.WriteLine("🏗️ Ensuring database and tables exist...");
            await context.Database.EnsureCreatedAsync();
            Console.WriteLine("✅ Database ready");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Database error: {ex.Message}");
    }
}

app.Run();
