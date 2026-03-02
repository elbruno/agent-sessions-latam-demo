using Microsoft.AspNetCore.SignalR;

namespace BalizasV16.Services;

public class BalizaHub : Hub
{
    public async Task RequestBalizas()
    {
        await Clients.Caller.SendAsync("BalizasUpdated", BalizaService.CurrentBalizas);
    }
}
