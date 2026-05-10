using Microsoft.AspNetCore.Mvc;
using WebActionResults.Data.Entities;
using WebActionResults.Data.Services;

namespace WebActionResults.Controllers;

public class FulfillmentController : Controller
{
    private readonly IFulfillmentService _fulfillmentService;
    private readonly IOrderService _orderService;

    public FulfillmentController(IFulfillmentService fulfillmentService, IOrderService orderService)
    {
        _fulfillmentService = fulfillmentService;
        _orderService = orderService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var shipments = await _fulfillmentService.GetAllShipmentsAsync();
        return View(shipments);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var shipment = await _fulfillmentService.GetShipmentByIdAsync(id);
        if (shipment == null)
            return NotFound();

        return View(shipment);
    }

    [HttpGet]
    public async Task<IActionResult> Track(string orderNumber)
    {
        var order = await _orderService.GetOrderByOrderNumberAsync(orderNumber);
        if (order == null)
            return NotFound("Order not found.");

        var shipment = await _fulfillmentService.GetShipmentByOrderIdAsync(order.Id);
        if (shipment == null)
            return NotFound("Shipment not found.");

        if (!string.IsNullOrEmpty(shipment.TrackingNumber))
        {
            shipment.TrackingUrl = await _fulfillmentService.GenerateTrackingUrlAsync(shipment.Carrier, shipment.TrackingNumber);
        }

        return View(shipment);
    }

    [HttpPost]
    public async Task<IActionResult> Webhook([FromBody] TrackingWebhookModel model)
    {
        if (model == null || string.IsNullOrEmpty(model.TrackingNumber))
            return BadRequest("Invalid webhook data.");

        try
        {
            var shipment = await _fulfillmentService.GetShipmentByOrderIdAsync(model.OrderId);
            if (shipment == null)
                return NotFound("Shipment not found.");

            var newStatus = ParseStatus(model.Status);
            await _fulfillmentService.UpdateShipmentStatusAsync(shipment.Id, newStatus, model.Description);

            return Ok("Webhook processed successfully.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error processing webhook: {ex.Message}");
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateShipment(int orderId, string carrier, string? trackingNumber = null)
    {
        var shipment = await _fulfillmentService.CreateShipmentAsync(orderId, carrier, trackingNumber);
        await _orderService.UpdateOrderStatusAsync(orderId, OrderStatus.Processing);

        TempData["ToastSuccess"] = "Shipment created successfully.";
        return RedirectToAction(nameof(Details), new { id = shipment.Id });
    }

    private ShipmentStatus ParseStatus(string? status)
    {
        return status?.ToLower() switch
        {
            "pending" => ShipmentStatus.Pending,
            "picked_up" or "pickedup" => ShipmentStatus.PickedUp,
            "in_transit" or "intransit" => ShipmentStatus.InTransit,
            "out_for_delivery" or "outfordelivery" => ShipmentStatus.OutForDelivery,
            "delivered" => ShipmentStatus.Delivered,
            "failed" => ShipmentStatus.Failed,
            "returned" => ShipmentStatus.Returned,
            _ => ShipmentStatus.Pending
        };
    }
}

public class TrackingWebhookModel
{
    public int OrderId { get; set; }
    public string? TrackingNumber { get; set; }
    public string? Status { get; set; }
    public string? Description { get; set; }
    public string? EstimatedDelivery { get; set; }
    public DateTime? Timestamp { get; set; }
}