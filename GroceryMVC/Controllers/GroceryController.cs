using GroceryMVC.Data;
using GroceryMVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace GroceryMVC.Controllers
{
    public class GroceryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GroceryController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Stored procedure returns all rows — fine
            var items = await _context.GroceryItems
                .FromSqlRaw("EXEC sp_GetAllGroceryItems")
                .AsNoTracking()
                .ToListAsync();

            return View(items);
        }

        public Task<IActionResult> AddEdit(int? id)
        {
            if (id == null)
            {
                // New item case
                var newItemList = new List<GroceryItem> { new GroceryItem() };
                return Task.FromResult<IActionResult>(View(newItemList));
            }

            var groceryItems = _context.GroceryItems
             .FromSqlRaw("EXEC sp_GetGroceryItemById @GroceryId",
                 new SqlParameter("@GroceryId", id))
             .AsEnumerable();  // switch to in-memory

            var groceryItem = groceryItems.FirstOrDefault();

            if (groceryItem == null)
            {
                return Task.FromResult<IActionResult>(NotFound());
            }

            var itemList = new List<GroceryItem> { groceryItem };
            return Task.FromResult<IActionResult>(View(itemList));

        }

        [HttpPost]
        public async Task<IActionResult> AddEdit(List<GroceryItem> items)
        {
            if (items == null || items.Count == 0)
                return RedirectToAction("Index");

            foreach (var item in items)
            {
                // Skip empty rows (no name or zero quantity/amount)
                if (string.IsNullOrWhiteSpace(item.ItemName) ||
                    item.Quantity <= 0 ||
                    item.Amount <= 0)
                {
                    continue;
                }

                if (item.GroceryId == 0)
                {
                    // 🔹 Insert new
                    await _context.Database.ExecuteSqlRawAsync(
                        "EXEC sp_InsertGroceryItem @ItemName, @Quantity, @Amount",
                        new SqlParameter("@ItemName", item.ItemName),
                        new SqlParameter("@Quantity", item.Quantity),
                        new SqlParameter("@Amount", item.Amount)
                    );
                }
                else
                {
                    // 🔹 Update existing
                    await _context.Database.ExecuteSqlRawAsync(
                        "EXEC sp_UpdateGroceryItem @GroceryId, @ItemName, @Quantity, @Amount",
                        new SqlParameter("@GroceryId", item.GroceryId),
                        new SqlParameter("@ItemName", item.ItemName),
                        new SqlParameter("@Quantity", item.Quantity),
                        new SqlParameter("@Amount", item.Amount)
                    );
                }
            }

            return RedirectToAction("Index");
        }


        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC sp_DeleteGroceryItem @GroceryId",
                    new SqlParameter("@GroceryId", id)
                );

                return Json(new { success = true });
            }
            catch (Exception)
            {
                return Json(new { success = false });
            }
        }
    }
}
