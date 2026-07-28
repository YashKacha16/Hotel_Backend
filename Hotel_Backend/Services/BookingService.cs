using Microsoft.EntityFrameworkCore;
using Hotel_Backend.Data;
using Hotel_Backend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hotel_Backend.Services
{
    public class BookingService
    {
        private readonly AppDbContext _context;

        public BookingService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Booking>> GetBookingsAsync(string? status, int? propertyId)
        {
            var query = _context.Bookings.Include(b => b.Room).ThenInclude(r => r.Category).AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(b => b.Status.ToLower() == status.ToLower());
            }

            return await query.OrderBy(b => b.CheckInDate).ToListAsync();
        }

        public async Task<Booking?> GetBookingByIdAsync(int id)
        {
            return await _context.Bookings
                .Include(b => b.Room)
                .ThenInclude(r => r.Category)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<Booking> CreateBookingAsync(Booking booking)
        {
            // Set random BookingCode
            booking.BookingCode = "BK-" + new Random().Next(1000, 9999);
            booking.CreatedAt = DateTime.UtcNow;
            booking.UpdatedAt = DateTime.UtcNow;

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();
            return booking;
        }

        public async Task<bool> UpdateBookingAsync(int id, Booking booking)
        {
            var existing = await _context.Bookings.FindAsync(id);
            if (existing == null) return false;

            existing.Status = booking.Status;
            existing.ForfeitedAmount = booking.ForfeitedAmount;
            existing.RefundAmount = booking.RefundAmount;
            existing.RefundMethod = booking.RefundMethod;
            existing.RefundStatus = booking.RefundStatus;
            existing.UpdatedAt = DateTime.UtcNow;

            _context.Bookings.Update(existing);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MarkNoShowAsync(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null) return false;

            booking.Status = "No-Show";
            booking.ForfeitedAmount = booking.AdvanceAmount;
            booking.UpdatedAt = DateTime.UtcNow;

            // Free the room
            var room = await _context.Rooms.FindAsync(booking.RoomId);
            if (room != null && room.Status == "Occupied")
            {
                room.Status = "Available";
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
