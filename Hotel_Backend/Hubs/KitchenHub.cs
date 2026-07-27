using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Hotel_Backend.Hubs
{
    public class KitchenHub : Hub
    {
        public async Task JoinKitchenGroup()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "kitchen-display");
        }

        public async Task LeaveKitchenGroup()
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "kitchen-display");
        }
    }
}
