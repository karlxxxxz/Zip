using Microsoft.EntityFrameworkCore;
using WayFindAR.Data;
using WayFindAR.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add session support (required for login/registration)
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = "WayFindAR.Session";
});

// Add database context with SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Add custom services
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<DestinationTrackerService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// Enable session middleware (MUST be before MapControllerRoute)
app.UseSession();

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();

        // Create database if it doesn't exist and apply migrations
        context.Database.Migrate();
        Console.WriteLine("Database migrated successfully.");

        // Check if we have any buildings
        if (!context.ARBuildings.Any())
        {
            Console.WriteLine("Adding sample AR buildings...");

            // Add sample buildings
            context.ARBuildings.AddRange(
                new WayFindAR.Models.ARBuilding
                {
                    Name = "Main Building",
                    Description = "Administration and offices",
                    Position = "0,0,0",
                    ModelType = "main",
                    Category = "Administration",
                    FloorLevel = "Ground Floor",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                },
                new WayFindAR.Models.ARBuilding
                {
                    Name = "Library",
                    Description = "Study resources center",
                    Position = "2,0,1",
                    ModelType = "library",
                    Category = "Academic",
                    FloorLevel = "Ground Floor",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                },
                new WayFindAR.Models.ARBuilding
                {
                    Name = "Science Center",
                    Description = "Laboratories and research",
                    Position = "-2,0,-1",
                    ModelType = "science",
                    Category = "Academic",
                    FloorLevel = "Level 1",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                },
                new WayFindAR.Models.ARBuilding
                {
                    Name = "Main Cafeteria",
                    Description = "Food court and dining area",
                    Position = "1,0,2",
                    ModelType = "cafeteria",
                    Category = "Services",
                    FloorLevel = "Ground Floor",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                }
            );

            await context.SaveChangesAsync();
            Console.WriteLine("Sample buildings added successfully.");
        }

        Console.WriteLine($"Total buildings in database: {context.ARBuildings.Count()}");
        Console.WriteLine($"Total users in database: {context.Users.Count()}");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while initializing the database.");
        Console.WriteLine($"Database initialization error: {ex.Message}");
    }
}

// Map controller routes
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

Console.WriteLine("WayFind AR Application Started!");
app.Run();