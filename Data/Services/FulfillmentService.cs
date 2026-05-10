using WebActionResults.Data.Entities;
using WebActionResults.Data.Repositories;

namespace WebActionResults.Data.Services;

public interface IFulfillmentService
{
    Task<Shipment?> GetShipmentByIdAsync(int id);
    Task<Shipment?> GetShipmentByOrderIdAsync(int orderId);
    Task<Shipment> CreateShipmentAsync(int orderId, string carrier, string? trackingNumber = null);
    Task UpdateShipmentStatusAsync(int shipmentId, ShipmentStatus status, string? lastUpdate = null);
    Task<List<Shipment>> GetAllShipmentsAsync();
    Task<List<Shipment>> GetPendingShipmentsAsync();
    Task<string> GenerateTrackingUrlAsync(string carrier, string trackingNumber);
}

public class FulfillmentService : IFulfillmentService
{
    private readonly IShipmentRepository _shipmentRepository;

    public FulfillmentService(IShipmentRepository shipmentRepository)
    {
        _shipmentRepository = shipmentRepository;
    }

    public async Task<Shipment?> GetShipmentByIdAsync(int id)
        => await _shipmentRepository.GetByIdAsync(id);

    public async Task<Shipment?> GetShipmentByOrderIdAsync(int orderId)
        => await _shipmentRepository.GetByOrderIdAsync(orderId);

    public async Task<Shipment> CreateShipmentAsync(int orderId, string carrier, string? trackingNumber = null)
    {
        var shipment = new Shipment
        {
            OrderId = orderId,
            Carrier = carrier,
            TrackingNumber = trackingNumber,
            Status = ShipmentStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        return await _shipmentRepository.CreateAsync(shipment);
    }

    public async Task UpdateShipmentStatusAsync(int shipmentId, ShipmentStatus status, string? lastUpdate = null)
    {
        var shipment = await _shipmentRepository.GetByIdAsync(shipmentId);
        if (shipment == null) return;

        shipment.Status = status;
        shipment.LastUpdate = lastUpdate;
        shipment.UpdatedAt = DateTime.UtcNow;

        if (status == ShipmentStatus.Delivered)
            shipment.ActualDelivery = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

        await _shipmentRepository.UpdateAsync(shipment);
    }

    public async Task<List<Shipment>> GetAllShipmentsAsync()
        => await _shipmentRepository.GetAllAsync();

    public async Task<List<Shipment>> GetPendingShipmentsAsync()
        => await _shipmentRepository.GetPendingAsync();

    public Task<string> GenerateTrackingUrlAsync(string carrier, string trackingNumber)
    {
        var url = carrier.ToLower() switch
        {
            "ghn" => $"https://tracking.ghn.com.vn/?order_code={trackingNumber}",
            "ghn_express" => $"https://tracking.ghn.com.vn/?order_code={trackingNumber}",
            "viettel_post" => $"https://viettelpost.com.vn/track?tracking_code={trackingNumber}",
            "vnpost" => $"https://www.vnpost.vn/track?tracking_code={trackingNumber}",
            "ninja_van" => $"https://www.ninjavan.co/en-vn/track?tracking_code={trackingNumber}",
            _ => $"https://example.com/track/{trackingNumber}"
        };

        return Task.FromResult(url);
    }
}