using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WebActionResults.Data.Services;

namespace WebActionResults.Services;

public class TrackingBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TrackingBackgroundService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(5);

    public TrackingBackgroundService(IServiceProvider serviceProvider, ILogger<TrackingBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Tracking Background Service is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckPendingShipmentsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking shipments.");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("Tracking Background Service is stopping.");
    }

    private async Task CheckPendingShipmentsAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var fulfillmentService = scope.ServiceProvider.GetRequiredService<IFulfillmentService>();

        var pendingShipments = await fulfillmentService.GetPendingShipmentsAsync();

        foreach (var shipment in pendingShipments)
        {
            try
            {
                await UpdateShipmentStatusAsync(fulfillmentService, shipment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating shipment {ShipmentId}", shipment.Id);
            }
        }

        if (pendingShipments.Any())
        {
            _logger.LogInformation("Checked {Count} pending shipments.", pendingShipments.Count);
        }
    }

    private async Task UpdateShipmentStatusAsync(IFulfillmentService fulfillmentService, Data.Entities.Shipment shipment)
    {
        if (string.IsNullOrEmpty(shipment.TrackingNumber) || string.IsNullOrEmpty(shipment.Carrier))
            return;

        var statusUpdate = await SimulateTrackingUpdateAsync(shipment.Carrier, shipment.TrackingNumber);

        if (statusUpdate != shipment.Status)
        {
            await fulfillmentService.UpdateShipmentStatusAsync(
                shipment.Id,
                statusUpdate,
                $"Auto-update: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");
        }
    }

    private Task<Data.Entities.ShipmentStatus> SimulateTrackingUpdateAsync(string carrier, string trackingNumber)
    {
        return Task.FromResult(Data.Entities.ShipmentStatus.InTransit);
    }
}