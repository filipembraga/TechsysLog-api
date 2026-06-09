using Microsoft.AspNetCore.SignalR;

namespace TechsysLog.Infrastructure.WebSockets.Hubs;

/// <summary>
/// SignalR hub for real-time notification broadcasting.
///
/// Design decision: no custom hub methods are defined here because
/// the server is the only producer of notifications — clients only receive.
/// </summary>
public class NotificationHub : Hub { }