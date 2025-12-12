using FinDashers.API.Models.Dashboard;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace FinDashers.API.Services.Dashboard;

public interface IWebSocketService
{
    Task HandleWebSocketAsync(WebSocket webSocket);
    Task BroadcastDashboardUpdateAsync(DashboardResponse data);
}

public class WebSocketService : IWebSocketService
{
    private readonly ConcurrentDictionary<string, WebSocket> _connections = new();
    private readonly ILogger<WebSocketService> _logger;

    public WebSocketService(ILogger<WebSocketService> logger)
    {
        _logger = logger;
    }

    public async Task HandleWebSocketAsync(WebSocket webSocket)
    {
        var connectionId = Guid.NewGuid().ToString();
        _connections.TryAdd(connectionId, webSocket);
        _logger.LogInformation($"WebSocket connection established: {connectionId}");

        try
        {
            var buffer = new byte[1024 * 4];
            while (webSocket.State == WebSocketState.Open)
            {
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection closed", CancellationToken.None);
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"WebSocket error for connection {connectionId}");
        }
        finally
        {
            _connections.TryRemove(connectionId, out _);
            _logger.LogInformation($"WebSocket connection closed: {connectionId}");
        }
    }

    public async Task BroadcastDashboardUpdateAsync(DashboardResponse data)
    {
        if (_connections.IsEmpty) return;

        var json = JsonSerializer.Serialize(data);
        var buffer = Encoding.UTF8.GetBytes(json);
        var segment = new ArraySegment<byte>(buffer);

        var tasks = new List<Task>();
        var connectionsToRemove = new List<string>();

        foreach (var connection in _connections)
        {
            if (connection.Value.State == WebSocketState.Open)
            {
                tasks.Add(connection.Value.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None));
            }
            else
            {
                connectionsToRemove.Add(connection.Key);
            }
        }

        // Remove closed connections
        foreach (var connectionId in connectionsToRemove)
        {
            _connections.TryRemove(connectionId, out _);
        }

        if (tasks.Count > 0)
        {
            await Task.WhenAll(tasks);
            _logger.LogInformation($"Broadcasted dashboard update to {tasks.Count} connections");
        }
    }
}