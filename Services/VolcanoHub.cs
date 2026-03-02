using Microsoft.AspNetCore.SignalR;

namespace BalizasV16.Services;

public class VolcanoHub : Hub
{
    public async Task RequestVolcanoData()
    {
        await Clients.Caller.SendAsync("VolcanoUpdated", VolcanoService.CurrentData);
    }
}
