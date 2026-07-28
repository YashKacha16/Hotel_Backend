using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Hotel_Backend.Data;
using Hotel_Backend.Models;
using Microsoft.Extensions.Logging;

namespace Hotel_Backend.Services
{
    public class WaitlistService : IWaitlistService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<WaitlistService> _logger;

        public WaitlistService(AppDbContext context, ILogger<WaitlistService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<WaitlistDto>> GetAllActiveAsync()
        {
            var entries = await _context.WaitlistEntries
                .Include(w => w.AssignedTable)
                .Where(w => w.Status == WaitlistStatus.Waiting || w.Status == WaitlistStatus.Notified)
                .OrderBy(w => w.CreatedAt)
                .ToListAsync();

            var now = DateTime.UtcNow;

            return entries.Select(w => new WaitlistDto
            {
                Id = w.Id,
                Token = w.Token,
                GuestName = w.GuestName,
                Phone = w.Phone,
                PartySize = w.PartySize,
                SeatingPreference = w.SeatingPreference,
                Notes = w.Notes,
                Status = w.Status.ToString(),
                CreatedAt = w.CreatedAt,
                WaitedMin = (int)(now - w.CreatedAt).TotalMinutes,
                AssignedTableId = w.AssignedTableId,
                AssignedTableName = w.AssignedTable?.Name
            });
        }

        public async Task<WaitlistDto> AddAsync(CreateWaitlistDto dto)
        {
            // Generate a token like A21
            var today = DateTime.UtcNow.Date;
            var todayCount = await _context.WaitlistEntries
                .Where(w => w.CreatedAt >= today)
                .CountAsync();
            
            string token = $"A{todayCount + 1}";

            var entry = new WaitlistEntry
            {
                Token = token,
                GuestName = dto.GuestName,
                Phone = dto.Phone,
                PartySize = dto.PartySize,
                SeatingPreference = dto.SeatingPreference,
                Notes = dto.Notes,
                Status = WaitlistStatus.Waiting,
                CreatedAt = DateTime.UtcNow
            };

            _context.WaitlistEntries.Add(entry);
            await _context.SaveChangesAsync();

            return new WaitlistDto
            {
                Id = entry.Id,
                Token = entry.Token,
                GuestName = entry.GuestName,
                Phone = entry.Phone,
                PartySize = entry.PartySize,
                SeatingPreference = entry.SeatingPreference,
                Notes = entry.Notes,
                Status = entry.Status.ToString(),
                CreatedAt = entry.CreatedAt,
                WaitedMin = 0
            };
        }

        public async Task<WaitlistDto> UpdateStatusAsync(int id, WaitlistStatus status)
        {
            var entry = await _context.WaitlistEntries.Include(w => w.AssignedTable).FirstOrDefaultAsync(w => w.Id == id);
            if (entry == null) throw new Exception("Waitlist entry not found");

            entry.Status = status;
            await _context.SaveChangesAsync();

            return new WaitlistDto
            {
                Id = entry.Id,
                Token = entry.Token,
                GuestName = entry.GuestName,
                Phone = entry.Phone,
                PartySize = entry.PartySize,
                SeatingPreference = entry.SeatingPreference,
                Notes = entry.Notes,
                Status = entry.Status.ToString(),
                CreatedAt = entry.CreatedAt,
                WaitedMin = (int)(DateTime.UtcNow - entry.CreatedAt).TotalMinutes,
                AssignedTableId = entry.AssignedTableId,
                AssignedTableName = entry.AssignedTable?.Name
            };
        }

        public async Task<WaitlistDto> AssignTableAsync(int id, int tableId)
        {
            var entry = await _context.WaitlistEntries.FirstOrDefaultAsync(w => w.Id == id);
            if (entry == null) throw new Exception("Waitlist entry not found");

            var table = await _context.RestaurantTables.FindAsync(tableId);
            if (table == null) throw new Exception("Table not found");

            if (table.Status != TableStatus.Free) throw new Exception("Table is not free");

            // Mark table as occupied
            table.Status = TableStatus.Occupied;

            // Create an order for the table
            var order = new Order
            {
                OrderNumber = "ORD-" + DateTime.Now.ToString("HHmmssff"),
                Type = OrderType.DineIn,
                TableId = tableId,
                MergeGroupId = table.MergeGroupId,
                CustomerName = entry.GuestName,
                SpecialInstructions = entry.Notes,
                Status = OrderStatus.New,
                IsPriority = false,
                CreatedAt = DateTime.UtcNow,
                Subtotal = 0,
                HasNewAddOns = false
            };
            _context.Orders.Add(order);

            // Assign table and mark as seated
            entry.AssignedTableId = tableId;
            entry.Status = WaitlistStatus.Seated;

            await _context.SaveChangesAsync();

            return new WaitlistDto
            {
                Id = entry.Id,
                Token = entry.Token,
                GuestName = entry.GuestName,
                Phone = entry.Phone,
                PartySize = entry.PartySize,
                SeatingPreference = entry.SeatingPreference,
                Notes = entry.Notes,
                Status = entry.Status.ToString(),
                CreatedAt = entry.CreatedAt,
                WaitedMin = (int)(DateTime.UtcNow - entry.CreatedAt).TotalMinutes,
                AssignedTableId = entry.AssignedTableId,
                AssignedTableName = table.Name
            };
        }
    }
}
