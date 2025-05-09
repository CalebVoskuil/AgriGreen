using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AgriGreen.Data;
using AgriGreen.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace AgriGreen.Controllers
{
    [Authorize(Roles = "Farmer,Employee")]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ProductsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Products
        public async Task<IActionResult> Index(string category, DateTime? startDate, DateTime? endDate, int? farmerId)
        {
            var productsQuery = _context.Products.Include(p => p.Farmer).AsQueryable();

            // Apply filters if provided
            if (!string.IsNullOrEmpty(category))
            {
                productsQuery = productsQuery.Where(p => p.Category == category);
            }

            // For date range filtering
            if (startDate.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.ProductionDate.Date >= startDate.Value.Date);
            }

            if (endDate.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.ProductionDate.Date <= endDate.Value.Date);
            }

            // For farmer filtering
            if (farmerId.HasValue && farmerId > 0)
            {
                productsQuery = productsQuery.Where(p => p.FarmerId == farmerId.Value);
            }

            // Get the current user's Farmer ID if they are a Farmer
            int? currentUserFarmerId = null;
            if (User.IsInRole("Farmer"))
            {
                var userId = _userManager.GetUserId(User);
                var farmer = await _context.Farmers.FirstOrDefaultAsync(f => f.UserId == userId);
                if (farmer != null)
                {
                    currentUserFarmerId = farmer.Id;
                    productsQuery = productsQuery.Where(p => p.FarmerId == farmer.Id);
                }
            }

            // Save the current user's Farmer ID for the view
            ViewData["CurrentUserFarmerId"] = currentUserFarmerId;

            // Provide data for filter dropdowns/pickers
            ViewData["Categories"] = new SelectList(await _context.Products.Select(p => p.Category).Distinct().ToListAsync());
            ViewData["Farmers"] = new SelectList(await _context.Farmers.ToListAsync(), "Id", "Name");
            
            // Pass back current filter values to repopulate the form
            ViewData["CurrentCategory"] = category;
            ViewData["CurrentStartDate"] = startDate?.ToString("yyyy-MM-dd");
            ViewData["CurrentEndDate"] = endDate?.ToString("yyyy-MM-dd");
            ViewData["CurrentFarmerId"] = farmerId;

            return View(await productsQuery.ToListAsync());
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Farmer)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Products/Create
        public async Task<IActionResult> Create()
        {
            if (User.IsInRole("Farmer"))
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    ModelState.AddModelError(string.Empty, "User ID is null or empty. You may not be properly logged in.");
                    return View(new Product());
                }

                var farmer = await _context.Farmers.FirstOrDefaultAsync(f => f.UserId == userId);
                if (farmer != null)
                {
                    var productViewModel = new Product { FarmerId = farmer.Id };
                    ViewData["FarmerInfo"] = $"Creating product as farmer: {farmer.Name} (ID: {farmer.Id}, UserID: {farmer.UserId})";
                    return View(productViewModel);
                }
                else
                {
                    // Provide clear feedback - farmer record doesn't exist for this user
                    ModelState.AddModelError(string.Empty, 
                        $"No Farmer record found for user ID: {userId}. Please contact administrator to set up your Farmer profile.");
                    return View(new Product());
                }
            }
            else // For Employees or other roles (if they were allowed to create)
            {
                ViewData["FarmerId"] = new SelectList(_context.Farmers, "Id", "Name");
                return View(new Product());
            }
        }

        // POST: Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Category,ProductionDate,FarmerId")] Product product)
        {
            try
            {
                // Debug info to understand what's happening
                var userId = _userManager.GetUserId(User);
                var isInFarmerRole = User.IsInRole("Farmer");
                var incomingFarmerId = product.FarmerId;
                
                if (isInFarmerRole)
                {
                    var farmer = await _context.Farmers.FirstOrDefaultAsync(f => f.UserId == userId);
                    if (farmer != null)
                    {
                        // Always set the FarmerId for a farmer user, regardless of what came in
                        product.FarmerId = farmer.Id;
                        
                        // Additional debug info
                        ViewData["FarmerDebug"] = $"User ID: {userId}, IsInFarmerRole: {isInFarmerRole}, " +
                                                  $"Original FarmerId: {incomingFarmerId}, Set to Farmer.Id: {farmer.Id}";
                    }
                    else
                    {
                        ModelState.AddModelError("FarmerId", "No Farmer record found for your user account. Please contact an administrator.");
                        ViewData["FarmerDebug"] = $"User ID: {userId}, IsInFarmerRole: {isInFarmerRole}, " +
                                                  $"Original FarmerId: {incomingFarmerId}, But no Farmer record exists!";
                        
                        // Create a Farmer record for this user if it doesn't exist
                        var newFarmer = new AgriGreen.Models.Farmer
                        {
                            Name = User.Identity.Name ?? "New Farmer",
                            UserId = userId
                        };
                        
                        _context.Farmers.Add(newFarmer);
                        await _context.SaveChangesAsync();
                        
                        // Now set the FarmerId to the newly created Farmer
                        product.FarmerId = newFarmer.Id;
                        ViewData["FarmerDebug"] += $" Created new Farmer with ID: {newFarmer.Id}";
                    }
                }
                else
                {
                    // For employees, ensure they selected a FarmerId
                    if (product.FarmerId <= 0)
                    {
                        ModelState.AddModelError("FarmerId", "Please select a farmer");
                        ViewData["FarmerDebug"] = $"Employee role, but no FarmerId selected. Incoming value: {incomingFarmerId}";
                    }
                    else
                    {
                        ViewData["FarmerDebug"] = $"Employee role, using selected FarmerId: {product.FarmerId}";
                    }
                }

                // Clear any existing model state errors for FarmerId since we've handled it
                ModelState.Remove("Farmer");
                ModelState.Remove("FarmerId");
                
                // Re-validate with the updated FarmerId
                if (product.FarmerId > 0)
                {
                    // Explicitly verify the Farmer exists to avoid validation errors
                    var farmerExists = await _context.Farmers.AnyAsync(f => f.Id == product.FarmerId);
                    if (!farmerExists)
                    {
                        ModelState.AddModelError("FarmerId", $"No Farmer record found with ID: {product.FarmerId}");
                    }
                }
                else
                {
                    ModelState.AddModelError("FarmerId", "The Farmer field is required");
                }

                if (ModelState.IsValid)
                {
                    // Explicitly attach the Farmer entity to avoid missing navigation property
                    var farmer = await _context.Farmers.FindAsync(product.FarmerId);
                    if (farmer != null)
                    {
                        // We don't need to set product.Farmer here, EF Core will handle it
                        _context.Add(product);
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError("FarmerId", "Selected Farmer not found in database");
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                ModelState.AddModelError(string.Empty, $"Error creating product: {ex.Message}");
                if (ex.InnerException != null)
                {
                    ModelState.AddModelError(string.Empty, $"Inner exception: {ex.InnerException.Message}");
                }
            }

            // If ModelState is invalid, repopulate FarmerId SelectList ONLY IF NOT a Farmer
            if (!User.IsInRole("Farmer")) 
            {
                 ViewData["FarmerId"] = new SelectList(_context.Farmers, "Id", "Name", product.FarmerId);
            }

            return View(product);
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            ViewData["FarmerId"] = new SelectList(_context.Farmers, "Id", "Name", product.FarmerId);
            return View(product);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Category,ProductionDate,FarmerId")] Product product)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["FarmerId"] = new SelectList(_context.Farmers, "Id", "Name", product.FarmerId);
            return View(product);
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Farmer)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}
