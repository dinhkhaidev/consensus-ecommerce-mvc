using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebActionResults.Data.Entities;

public class Payment
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    [Column(TypeName = "nvarchar(50)")]
    public PaymentMethod Method { get; set; }

    [Column(TypeName = "nvarchar(50)")]
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    [MaxLength(100)]
    public string? TransactionId { get; set; }

    [MaxLength(500)]
    public string? PaymentUrl { get; set; }

    [MaxLength(500)]
    public string? ReturnUrl { get; set; }

    [MaxLength(255)]
    public string? ErrorMessage { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? CompletedAt { get; set; }

    public DateTime? ExpiresAt { get; set; }

    [ForeignKey(nameof(OrderId))]
    public virtual Order Order { get; set; } = null!;
}