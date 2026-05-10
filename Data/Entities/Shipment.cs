using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebActionResults.Data.Entities;

public class Shipment
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    [MaxLength(100)]
    public string? TrackingNumber { get; set; }

    [MaxLength(50)]
    public string Carrier { get; set; } = string.Empty;

    public ShipmentStatus Status { get; set; } = ShipmentStatus.Pending;

    [MaxLength(500)]
    public string? TrackingUrl { get; set; }

    [MaxLength(500)]
    public string? LastUpdate { get; set; }

    [MaxLength(500)]
    public string? EstimatedDelivery { get; set; }

    [MaxLength(500)]
    public string? ActualDelivery { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    [ForeignKey(nameof(OrderId))]
    public virtual Order Order { get; set; } = null!;
}

public enum ShipmentStatus
{
    Pending = 0,
    PickedUp = 1,
    InTransit = 2,
    OutForDelivery = 3,
    Delivered = 4,
    Failed = 5,
    Returned = 6
}