using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Hotel_Backend.Data;
using Hotel_Backend.Models;
using Hotel_Backend.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Hotel_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingsController : ControllerBase
    {
        private readonly BookingService _bookingService;
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public BookingsController(BookingService bookingService, AppDbContext context, IConfiguration configuration)
        {
            _bookingService = bookingService;
            _context = context;
            _configuration = configuration;
        }

        // GET: api/Bookings
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BookingDto>>> GetBookings([FromQuery] string? status, [FromQuery] int? propertyId)
        {
            var bookings = await _bookingService.GetBookingsAsync(status, propertyId);
            return Ok(bookings.Select(MapToDto));
        }

        // GET: api/Bookings/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BookingDto>> GetBooking(int id)
        {
            var booking = await _bookingService.GetBookingByIdAsync(id);

            if (booking == null)
            {
                return NotFound();
            }

            return Ok(MapToDto(booking));
        }

        [HttpPost]
        public async Task<ActionResult<BookingDto>> PostBooking([FromForm] CreateBookingDto dto, IFormFile? idProofFile)
        {
            var room = await _context.Rooms.FindAsync(dto.RoomId);
            if (room == null)
            {
                return NotFound(new { message = "Room not found." });
            }

            if (dto.Guests > room.Capacity)
            {
                return BadRequest(new { message = $"The number of guests ({dto.Guests}) cannot exceed the room capacity ({room.Capacity})." });
            }

            // Validate double booking
            var conflict = await _context.Bookings
                .AnyAsync(b => b.RoomId == dto.RoomId 
                            && b.Status != "Cancelled" 
                            && b.Status != "No-Show"
                            && b.Status != "Completed"
                            && b.CheckInDate < dto.CheckOutDate 
                            && b.CheckOutDate > dto.CheckInDate);

            if (conflict)
            {
                return Conflict(new { message = "Room is already booked for these dates." });
            }

            var booking = new Booking
            {
                GuestName = dto.GuestName,
                Phone = dto.Phone,
                Email = dto.Email,
                IdNumber = dto.IdNumber,
                RoomId = dto.RoomId,
                CheckInDate = dto.CheckInDate,
                CheckInTime = TimeSpan.TryParse(dto.CheckInTime, out var ts) ? ts : new TimeSpan(14, 0, 0),
                CheckOutDate = dto.CheckOutDate,
                Source = dto.Source,
                Guests = dto.Guests,
                AdvanceAmount = dto.AdvanceAmount,
                PaymentMethod = dto.PaymentMethod,
                Status = dto.Status
            };

            var createdBooking = await _bookingService.CreateBookingAsync(booking);

            // Handle ID Proof File Upload
            if (idProofFile != null)
            {
                var basePath = _configuration.GetSection("AttachmentConfig").GetValue<string>("BasePath");
                if (string.IsNullOrEmpty(basePath))
                {
                    basePath = @"C:\hotel attachment";
                }

                var bookingDir = Path.Combine(basePath, "bookings");
                if (!Directory.Exists(bookingDir))
                {
                    Directory.CreateDirectory(bookingDir);
                }

                var ext = Path.GetExtension(idProofFile.FileName);
                var fileName = $"id_{createdBooking.Id}{ext}";
                var fullPath = Path.Combine(bookingDir, fileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await idProofFile.CopyToAsync(stream);
                }

                createdBooking.IdProofUrl = fileName;
                await _bookingService.UpdateBookingAsync(createdBooking.Id, createdBooking);
            }
            
            // Mark room as occupied if status is Checked-in
            if (createdBooking.Status == "Checked-in")
            {
                room.Status = "Occupied";
                await _context.SaveChangesAsync();
            }

            return CreatedAtAction(nameof(GetBooking), new { id = createdBooking.Id }, MapToDto(createdBooking));
        }

        // PATCH: api/Bookings/5/mark-no-show
        [HttpPatch("{id}/mark-no-show")]
        public async Task<IActionResult> MarkNoShow(int id)
        {
            var success = await _bookingService.MarkNoShowAsync(id);
            if (!success)
            {
                return NotFound();
            }
            return NoContent();
        }

        // PUT: api/Bookings/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBooking(int id, UpdateBookingDto dto)
        {
            var existingBooking = await _bookingService.GetBookingByIdAsync(id);
            if (existingBooking == null) return NotFound();

            var booking = new Booking
            {
                Status = dto.Status,
                ForfeitedAmount = dto.ForfeitedAmount,
                RefundAmount = dto.RefundAmount,
                RefundMethod = dto.RefundMethod,
                RefundStatus = dto.RefundStatus
            };

            var success = await _bookingService.UpdateBookingAsync(id, booking);
            if (!success) return NotFound();

            // Update room status based on booking status transitions
            if (dto.Status == "Checked-in")
            {
                var room = await _context.Rooms.FindAsync(existingBooking.RoomId);
                if (room != null)
                {
                    room.Status = "Occupied";
                    await _context.SaveChangesAsync();
                }
            }
            else if (dto.Status == "Completed" || dto.Status == "Cancelled")
            {
                var room = await _context.Rooms.FindAsync(existingBooking.RoomId);
                if (room != null)
                {
                    room.Status = "Available";
                    await _context.SaveChangesAsync();
                }
            }

            return NoContent();
        }

        private static BookingDto MapToDto(Booking b)
        {
            return new BookingDto
            {
                Id = b.Id,
                BookingCode = b.BookingCode,
                GuestName = b.GuestName,
                Phone = b.Phone,
                Email = b.Email,
                IdNumber = b.IdNumber,
                IdProofUrl = b.IdProofUrl,
                RoomId = b.RoomId,
                Room = b.Room != null ? new RoomDto
                {
                    Id = b.Room.Id,
                    Number = b.Room.Number,
                    CategoryId = b.Room.CategoryId,
                    Floor = b.Room.Floor,
                    Capacity = b.Room.Capacity,
                    BasePrice = b.Room.BasePrice,
                    Status = b.Room.Status
                } : null,
                CheckInDate = b.CheckInDate,
                CheckInTime = b.CheckInTime.ToString(),
                CheckOutDate = b.CheckOutDate,
                Source = b.Source,
                Guests = b.Guests,
                AdvanceAmount = b.AdvanceAmount,
                PaymentMethod = b.PaymentMethod,
                Status = b.Status,
                ForfeitedAmount = b.ForfeitedAmount,
                RefundAmount = b.RefundAmount,
                RefundMethod = b.RefundMethod,
                RefundStatus = b.RefundStatus
            };
        }
    }
}
