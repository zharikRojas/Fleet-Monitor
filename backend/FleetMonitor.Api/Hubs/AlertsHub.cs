using FleetMonitor.Api.Domain.Enums;
using Microsoft.AspNetCore.SignalR;

namespace FleetMonitor.Api.Hubs;

public class AlertsHub : Hub
{
    public const string AdminGroup = "admins";

    public override async Task OnConnectedAsync()
    {
        if (Context.GetHttpContext()?.Items["UserRole"] is not UserRole.Admin)
        {
            Context.Abort();
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, AdminGroup);
        await base.OnConnectedAsync();
    }
}
