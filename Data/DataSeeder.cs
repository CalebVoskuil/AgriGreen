using AgriGreen.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgriGreen.Data
{
    public class DataSeeder
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DataSeeder(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task SeedDataAsync()
        {
            // Ensure roles exist
            await EnsureRolesExistAsync();

            // Seed Employee users
            var employeeUsers = await SeedEmployeeUsersAsync();

            // Seed Farmer users
            var farmerUsers = await SeedFarmerUsersAsync();

            // Seed Employee records
            await SeedEmployeesAsync(employeeUsers);

            // Seed Farmer records
            var farmers = await SeedFarmersAsync(farmerUsers);

            // Seed Products
            await SeedProductsAsync(farmers);

            // Save all changes
            await _context.SaveChangesAsync();
        }

        private async Task EnsureRolesExistAsync()
        {
            string[] roles = { "Farmer", "Employee" };
            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        private async Task<List<IdentityUser>> SeedEmployeeUsersAsync()
        {
            var users = new List<IdentityUser>();
            var employeeEmails = new[] 
            { 
                "sarah.johnson@agrigreen.com",
                "michael.smith@agrigreen.com",
                "jessica.williams@agrigreen.com"
            };

            foreach (var email in employeeEmails)
            {
                var user = await CreateOrGetUserAsync(email, "Employee123!", "Employee");
                users.Add(user);
            }

            return users;
        }

        private async Task<List<IdentityUser>> SeedFarmerUsersAsync()
        {
            var users = new List<IdentityUser>();
            var farmerEmails = new[] 
            { 
                "john.doe@farm.com",
                "emily.wilson@greenfarms.com",
                "robert.brown@organicvalley.com",
                "lisa.martinez@sunrisefarms.com",
                "david.taylor@naturegrowers.com"
            };

            foreach (var email in farmerEmails)
            {
                var user = await CreateOrGetUserAsync(email, "Farmer123!", "Farmer");
                users.Add(user);
            }

            return users;
        }

        private async Task<IdentityUser> CreateOrGetUserAsync(string email, string password, string role)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new IdentityUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, role);
                }
            }

            return user;
        }

        private async Task SeedEmployeesAsync(List<IdentityUser> employeeUsers)
        {
            if (!_context.Employees.Any())
            {
                var employees = new List<Employee>
                {
                    new Employee { Name = "Sarah Johnson", UserId = employeeUsers[0].Id, User = employeeUsers[0] },
                    new Employee { Name = "Michael Smith", UserId = employeeUsers[1].Id, User = employeeUsers[1] },
                    new Employee { Name = "Jessica Williams", UserId = employeeUsers[2].Id, User = employeeUsers[2] }
                };

                await _context.Employees.AddRangeAsync(employees);
            }
        }

        private async Task<List<Farmer>> SeedFarmersAsync(List<IdentityUser> farmerUsers)
        {
            if (!_context.Farmers.Any())
            {
                var farmers = new List<Farmer>
                {
                    new Farmer { Name = "John Doe's Farm", UserId = farmerUsers[0].Id, User = farmerUsers[0] },
                    new Farmer { Name = "Green Valley Organics", UserId = farmerUsers[1].Id, User = farmerUsers[1] },
                    new Farmer { Name = "Brown Family Farms", UserId = farmerUsers[2].Id, User = farmerUsers[2] },
                    new Farmer { Name = "Sunrise Produce", UserId = farmerUsers[3].Id, User = farmerUsers[3] },
                    new Farmer { Name = "Nature's Best Growers", UserId = farmerUsers[4].Id, User = farmerUsers[4] }
                };

                await _context.Farmers.AddRangeAsync(farmers);
                await _context.SaveChangesAsync();
                return farmers;
            }

            return await _context.Farmers.ToListAsync();
        }

        private async Task SeedProductsAsync(List<Farmer> farmers)
        {
            if (!_context.Products.Any())
            {
                var rnd = new Random();
                var categories = new[] { "Vegetables", "Fruits", "Dairy", "Grains", "Herbs", "Meat", "Eggs" };
                
                var products = new List<Product>();
                var productsPerFarmer = 5;

                foreach (var farmer in farmers)
                {
                    // Create a list of product names specific to each category
                    var vegetableNames = new[] { "Tomatoes", "Carrots", "Lettuce", "Broccoli", "Spinach", "Kale", "Peppers", "Onions", "Zucchini", "Cucumbers" };
                    var fruitNames = new[] { "Apples", "Strawberries", "Blueberries", "Cherries", "Peaches", "Pears", "Watermelon", "Cantaloupe", "Grapes", "Raspberries" };
                    var dairyNames = new[] { "Milk", "Cheese", "Butter", "Yogurt", "Cream", "Cottage Cheese" };
                    var grainNames = new[] { "Wheat", "Corn", "Barley", "Oats", "Rice", "Quinoa", "Rye" };
                    var herbNames = new[] { "Basil", "Mint", "Rosemary", "Thyme", "Oregano", "Sage", "Parsley", "Cilantro" };
                    var meatNames = new[] { "Beef", "Pork", "Lamb", "Chicken", "Turkey" };
                    var eggNames = new[] { "Chicken Eggs", "Duck Eggs", "Quail Eggs" };

                    for (int i = 0; i < productsPerFarmer; i++)
                    {
                        var categoryIndex = rnd.Next(categories.Length);
                        var category = categories[categoryIndex];
                        string productName;

                        // Select appropriate product name based on category
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

                        // Generate a random date within the last 6 months
                        var daysAgo = rnd.Next(1, 180);
                        var productionDate = DateTime.Now.AddDays(-daysAgo);

                        products.Add(new Product
                        {
                            Name = $"{productName}",
                            Category = category,
                            ProductionDate = productionDate,
                            FarmerId = farmer.Id,
                            Farmer = farmer
                        });
                    }
                }

                await _context.Products.AddRangeAsync(products);
            }
        }
    }
} 