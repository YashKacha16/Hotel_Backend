using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Hotel_Backend.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Hotel_Backend.Services
{
    public class NoShowBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<NoShowBackgroundService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1);
        private readonly TimeSpan _gracePeriod = TimeSpan.FromHours(4);

        public NoShowBackgroundService(IServiceProvider serviceProvider, ILogger<NoShowBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("NoShowBackgroundService is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessNoShowsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while processing no-shows.");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }

            _logger.LogInformation("NoShowBackgroundService is stopping.");
        }

        private async Task ProcessNoShowsAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var now = DateTime.UtcNow;

            var expiredBookings = await context.Bookings
                .Include(b => b.Room)
                .Where(b => b.Status == "Confirmed")
                .ToListAsync();

            var noShowBookings = expiredBookings.Where(b => 
            {
                var checkInDateTime = b.CheckInDate.Date.Add(b.CheckInTime);
                var cutoffTime = checkInDateTime.Add(_gracePeriod);
                return now > cutoffTime;
            }).ToList();

            if (noShowBookings.Any())
            {
                _logger.LogInformation($"Found {noShowBookings.Count} no-show bookings.");

                foreach (var booking in noShowBookings)
                {
                    booking.Status = "No-Show";
                    booking.ForfeitedAmount = booking.AdvanceAmount;
                    booking.UpdatedAt = DateTime.UtcNow;

                    if (booking.Room != null && booking.Room.Status == "Occupied")
                    {
                        booking.Room.Status = "Available";
                    }
                }

                await context.SaveChangesAsync();
                _logger.LogInformation("Processed and updated no-show bookings.");
            }
        }
    }
}
