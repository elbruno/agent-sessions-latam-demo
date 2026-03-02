using BalizasV16.Models;
using Microsoft.AspNetCore.SignalR;

namespace BalizasV16.Services;

public class TacoHub : Hub
{
    private readonly TacoService _tacoService;

    public TacoHub(TacoService tacoService)
    {
        _tacoService = tacoService;
    }

    public async Task GetTacoStands(string? city, string? tacoType)
    {
        var stands = await _tacoService.GetTacoStandsAsync(city, tacoType);
        await Clients.Caller.SendAsync("TacoStandsUpdated", stands);
    }

    public async Task VoteTacoStand(long standId)
    {
        // In a real app, this would persist to a database
        await Clients.All.SendAsync("VoteReceived", standId);
    }
}
