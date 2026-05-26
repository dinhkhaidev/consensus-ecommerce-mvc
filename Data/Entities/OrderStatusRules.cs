namespace WebActionResults.Data.Entities;

public static class OrderStatusRules
{
    private static readonly OrderStatus[] SalesCountingStatuses =
    {
        OrderStatus.Confirmed,
        OrderStatus.Processing,
        OrderStatus.Shipped,
        OrderStatus.Delivered,
        OrderStatus.ReturnRejected
    };

    public static bool IsSalesCountingStatus(OrderStatus status)
        => SalesCountingStatuses.Contains(status);

    public static IReadOnlyCollection<OrderStatus> GetSalesCountingStatuses()
        => SalesCountingStatuses;

    public static bool HasDeductedStock(OrderStatus status)
        => IsSalesCountingStatus(status) || status == OrderStatus.ReturnRequested;

    public static IReadOnlyCollection<OrderStatus> GetAllowedAdminNextStatuses(OrderStatus currentStatus)
        => currentStatus switch
        {
            OrderStatus.Pending => new[] { OrderStatus.Confirmed, OrderStatus.Cancelled },
            OrderStatus.Confirmed => new[] { OrderStatus.Processing, OrderStatus.Cancelled },
            OrderStatus.Processing => new[] { OrderStatus.Shipped, OrderStatus.Cancelled },
            OrderStatus.Shipped => new[] { OrderStatus.Delivered },
            _ => Array.Empty<OrderStatus>()
        };

    public static bool CanAdminTransition(OrderStatus currentStatus, OrderStatus nextStatus)
        => currentStatus == nextStatus || GetAllowedAdminNextStatuses(currentStatus).Contains(nextStatus);
}
