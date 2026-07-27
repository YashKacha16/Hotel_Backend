using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Hotel_Backend.Data;
using Hotel_Backend.Models;

using Microsoft.Extensions.Configuration;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace Hotel_Backend.Services
{
    public class MenuService : IMenuService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public MenuService(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<IEnumerable<MenuItem>> GetAllMenuItemsAsync(int? categoryId = null)
        {
            var query = _context.MenuItems.Include(m => m.Category).AsQueryable();

            if (categoryId.HasValue)
            {
                query = query.Where(m => m.CategoryId == categoryId.Value);
            }

            return await query.OrderBy(m => m.Position).ToListAsync();
        }

        public async Task<MenuItem?> GetMenuItemByIdAsync(int id)
        {
            return await _context.MenuItems
                .Include(m => m.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<MenuItem> AddMenuItemAsync(MenuItem menuItem)
        {
            // Validate categoryId
            var categoryExists = await _context.Categories.AnyAsync(c => c.Id == menuItem.CategoryId);
            if (!categoryExists)
            {
                throw new ArgumentException($"Category with ID {menuItem.CategoryId} does not exist.");
            }

            _context.MenuItems.Add(menuItem);
            await _context.SaveChangesAsync();
            return menuItem;
        }

        public async Task<MenuItem?> UpdateMenuItemAsync(int id, MenuItem menuItem)
        {
            var existingItem = await _context.MenuItems.FindAsync(id);
            if (existingItem == null)
            {
                return null;
            }

            // Validate categoryId if it changed
            if (existingItem.CategoryId != menuItem.CategoryId)
            {
                var categoryExists = await _context.Categories.AnyAsync(c => c.Id == menuItem.CategoryId);
                if (!categoryExists)
                {
                    throw new ArgumentException($"Category with ID {menuItem.CategoryId} does not exist.");
                }
                existingItem.CategoryId = menuItem.CategoryId;
            }

            existingItem.Name = menuItem.Name;
            existingItem.Description = menuItem.Description;
            existingItem.Price = menuItem.Price;
            existingItem.Image = menuItem.Image;
            existingItem.Veg = menuItem.Veg;
            existingItem.Available = menuItem.Available;
            existingItem.Position = menuItem.Position;

            await _context.SaveChangesAsync();
            return existingItem;
        }

        public async Task<bool> DeleteMenuItemAsync(int id)
        {
            var item = await _context.MenuItems.FindAsync(id);
            if (item == null)
            {
                return false;
            }

            _context.MenuItems.Remove(item);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ToggleAvailabilityAsync(int id)
        {
            var item = await _context.MenuItems.FindAsync(id);
            if (item == null)
            {
                return false;
            }

            item.Available = !item.Available;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ReorderMenuItemsAsync(List<CategoryPositionDto> positions)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    foreach (var pos in positions)
                    {
                        var item = await _context.MenuItems.FindAsync(pos.Id);
                        if (item != null)
                        {
                            item.Position = pos.Position;
                        }
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return true;
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }

        public async Task<IEnumerable<MenuGroupedDto>> GetGroupedMenuAsync()
        {
            return await _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.Position)
                .Select(c => new MenuGroupedDto
                {
                    CategoryId = c.Id,
                    CategoryName = c.Name,
                    Position = c.Position,
                    Items = c.MenuItems
                        .OrderBy(m => m.Position)
                        .Select(m => new MenuItemDto
                        {
                            Id = m.Id,
                            Name = m.Name,
                            Description = m.Description,
                            Price = m.Price,
                            CategoryId = m.CategoryId,
                            Image = m.Image,
                            Veg = m.Veg,
                            Available = m.Available,
                            Position = m.Position
                        }).ToList()
                })
                .ToListAsync();
        }

        public async Task<bool> UploadImageAsync(int id, IFormFile image)
        {
            var menuItem = await _context.MenuItems.FindAsync(id);
            if (menuItem == null)
            {
                return false;
            }

            var basePath = _configuration.GetSection("AttachmentConfig").GetValue<string>("BasePath");
            if (string.IsNullOrEmpty(basePath))
            {
                basePath = @"C:\hotel attachment";
            }

            var menuDir = Path.Combine(basePath, "menu item");
            if (!Directory.Exists(menuDir))
            {
                Directory.CreateDirectory(menuDir);
            }

            var ext = Path.GetExtension(image.FileName);
            var fileName = $"{id}{ext}";
            var fullPath = Path.Combine(menuDir, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }

            menuItem.Image = $"/attachments/menu item/{fileName}";
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
