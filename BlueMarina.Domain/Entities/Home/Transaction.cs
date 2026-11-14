// using BlueMarina.Domain.Common;
// using BlueMarina.Domain.Enums;

// namespace BlueMarina.Domain.Entities;

// /// <summary>
// /// Ledger entry. Every money movement creates one.
// /// </summary>
// public class Transaction : BaseEntity
// {
//     public string Reference { get; set; } = string.Empty;
//     public OwnerType OwnerType { get; set; }
//     public Guid OwnerId { get; set; }
//     public AccountType AccountType { get; set; }
//     public TransactionType Type { get; set; }
//     public Direction Direction { get; set; }
//     public long AmountMinor { get; set; }
//     public long FeeMinor { get; set; }
//     public Currency Currency { get; set; }
//     public TransactionStatus Status { get; set; }
//     public string? Counterparty { get; set; }
//     public TransactionChannel? Channel { get; set; }
//     public string? Note { get; set; }
//     public string? ProviderReference { get; set; }
//     public string? RelatedEntityType { get; set; }
//     public Guid? RelatedEntityId { get; set; }
//     public Guid? ReversalOfTransactionId { get; set; }
//     public DateTime? CompletedAt { get; set; }
// }
