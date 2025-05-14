using AgriGreen.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Identity.UI.Services;
using AgriGreen.Services;
using AgriGreen.Models;

var builder = WebApplication.CreateBuilder(args);

// Configure database context based on environment
if (builder.Environment.IsDevelopment())
{
    // In development, use SQLite for lightweight local database
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
    {
        options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"), 
            sqliteOptions => sqliteOptions.CommandTimeout(30));
    });
}
else
{
    // In production, use SQL Server with retry logic for cloud resilience
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
    {
        options.UseSqlServer(builder.Configuration.GetConnectionString("AzureConnection"), 
            sqlOptions => 
            {
                // Enable retry mechanism for transient failures in cloud environments
                // This helps handle temporary connection issues that may occur
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorNumbersToAdd: null);
                sqlOptions.CommandTimeout(30);
            });
    });
}

// Configure authentication
builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        // Load Google OAuth credentials from configuration
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    });

// Add Identity services with async token providers
builder.Services.AddDefaultIdentity<IdentityUser>(options => 
{
    options.SignIn.RequireConfirmedAccount = true;
    // Configure token providers for better cold start performance
    // This optimizes identity token generation for faster application startup
    options.Tokens.ProviderMap.Add("Default", new TokenProviderDescriptor(
        typeof(IUserTwoFactorTokenProvider<IdentityUser>)));
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Register email sender as singleton for connection reuse
// Using singleton pattern ensures connection pooling and reuse across requests
builder.Services.AddSingleton<IEmailSender, MailjetEmailSender>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure error handling first
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

// Initialize roles and seed data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var logger = services.GetRequiredService<ILogger<Program>>();

        // Ensure database is created
        context.Database.EnsureCreated();
        
        // Create application roles if they don't exist
        // This establishes the role-based security structure for the app
        string[] roles = { "Farmer", "Employee" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
                logger.LogInformation($"Created role: {role}");
            }
        }

        // Set up demo employee accounts for testing and initial system setup
        var employeeEmails = new[] 
        { 
            "sarah.johnson@agrigreen.com",
            "michael.smith@agrigreen.com",
            "jessica.williams@agrigreen.com"
        };

        var employeeUsers = new List<IdentityUser>();
        foreach (var email in employeeEmails)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                // Create identity user with pre-confirmed email for easier testing
                user = new IdentityUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, "Employee123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Employee");
                    logger.LogInformation($"Created employee user: {email}");
                }
            }
            employeeUsers.Add(user);
        }

        // Create employee profile records linked to user accounts
        // This maintains the relationship between identity users and domain model
        if (!context.Employees.Any())
        {
            var employees = new List<Employee>
            {
                new Employee { Name = "Sarah Johnson", UserId = employeeUsers[0].Id },
                new Employee { Name = "Michael Smith", UserId = employeeUsers[1].Id },
                new Employee { Name = "Jessica Williams", UserId = employeeUsers[2].Id }
            };

            await context.Employees.AddRangeAsync(employees);
            await context.SaveChangesAsync();
            logger.LogInformation("Created employee records");
        }

        // Set up demo farmer accounts for testing and showcasing app functionality
        var farmerEmails = new[] 
        { 
            "john.doe@farm.com",
            "emily.wilson@greenfarms.com",
            "robert.brown@organicvalley.com",
            "lisa.martinez@sunrisefarms.com",
            "david.taylor@naturegrowers.com"
        };

        var farmerUsers = new List<IdentityUser>();
        foreach (var email in farmerEmails)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                // Create identity user with pre-confirmed email for easier testing
                user = new IdentityUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, "Farmer123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Farmer");
                    logger.LogInformation($"Created farmer user: {email}");
                }
            }
            farmerUsers.Add(user);
        }

        // Create farmer profile records linked to user accounts
        // This establishes the business entities that will own products
        var farmers = await context.Farmers.ToListAsync();
        if (!farmers.Any())
        {
            farmers = new List<Farmer>
            {
                new Farmer { Name = "John Doe's Farm", UserId = farmerUsers[0].Id },
                new Farmer { Name = "Green Valley Organics", UserId = farmerUsers[1].Id },
                new Farmer { Name = "Brown Family Farms", UserId = farmerUsers[2].Id },
                new Farmer { Name = "Sunrise Produce", UserId = farmerUsers[3].Id },
                new Farmer { Name = "Nature's Best Growers", UserId = farmerUsers[4].Id }
            };

            await context.Farmers.AddRangeAsync(farmers);
            await context.SaveChangesAsync();
            logger.LogInformation("Created farmer records");
            farmers = await context.Farmers.ToListAsync();
        }

        // Seed product data for demonstration purposes
        // This creates a realistic dataset for the application to display
        if (!context.Products.Any())
        {
            var rnd = new Random();
            var categories = new[] { "Vegetables", "Fruits", "Dairy", "Grains", "Herbs", "Meat", "Eggs" };
            
            var products = new List<Product>();

            foreach (var farmer in farmers)
            {
                var vegetableNames = new[] { "Tomatoes", "Carrots", "Lettuce", "Broccoli", "Spinach", "Kale", "Peppers", "Onions", "Zucchini", "Cucumbers" };
                var fruitNames = new[] { "Apples", "Strawberries", "Blueberries", "Cherries", "Peaches", "Pears", "Watermelon", "Cantaloupe", "Grapes", "Raspberries" };
                var dairyNames = new[] { "Milk", "Cheese", "Butter", "Yogurt", "Cream", "Cottage Cheese" };
                var grainNames = new[] { "Wheat", "Corn", "Barley", "Oats", "Rice", "Quinoa", "Rye" };
                var herbNames = new[] { "Basil", "Mint", "Rosemary", "Thyme", "Oregano", "Sage", "Parsley", "Cilantro" };
                var meatNames = new[] { "Beef", "Pork", "Lamb", "Chicken", "Turkey" };
                var eggNames = new[] { "Chicken Eggs", "Duck Eggs", "Quail Eggs" };

                // Create 5 products per farmer to generate diverse inventory
                for (int i = 0; i < 5; i++)
                {
                    var categoryIndex = rnd.Next(categories.Length);
                    var category = categories[categoryIndex];
                    string productName;

                    // Select appropriate product name based on category
                    // This ensures products match their category for realistic data
                    switch (category)
                    {
                        case "Vegetables":
                            productName = vegetableNames[rnd.Next(vegetableNames.Length)];
                            break;
                        case "Fruits":
                            productName = fruitNames[rnd.Next(fruitNames.Length)];
                            break;
                        case "Dairy":
                            productName = dairyNames[rnd.Next(dairyNames.Length)];
                            break;
                        case "Grains":
                            productName = grainNames[rnd.Next(grainNames.Length)];
                            break;
                        case "Herbs":
                            productName = herbNames[rnd.Next(herbNames.Length)];
                            break;
                        case "Meat":
                            productName = meatNames[rnd.Next(meatNames.Length)];
                            break;
                        case "Eggs":
                            productName = eggNames[rnd.Next(eggNames.Length)];
                            break;
                        default:
                            productName = "Unknown Product";
                            break;
                    }

                    // Generate a random date within the last 6 months for realistic production dating
                    var daysAgo = rnd.Next(1, 180);
                    var productionDate = DateTime.Now.AddDays(-daysAgo);

                    products.Add(new Product
                    {
                        Name = productName,
                        Category = category,
                        ProductionDate = productionDate,
                        FarmerId = farmer.Id
                    });
                    
                    logger.LogInformation($"Created product: {productName} for farmer {farmer.Name}");
                }
            }

            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();
            logger.LogInformation($"Created {products.Count} product records");
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database");
    }
}

await app.RunAsync();
