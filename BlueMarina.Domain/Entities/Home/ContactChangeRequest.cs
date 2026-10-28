// using BlueMarina.Domain.Common;
// using BlueMarina.Domain.Enums;

// namespace BlueMarina.Domain.Entities;

// /// <summary>
// /// Change of phone or email, verified on both old and new.
// /// </summary>
// public class ContactChangeRequest : BaseEntity
// {
//     public Guid UserId { get; set; }
//     public ContactChangeField Field { get; set; }
//     public string OldValue { get; set; } = string.Empty;
//     public string NewValue { get; set; } = string.Empty;
//     public ContactChangeStatus Status { get; set; }
//     public bool ConfirmedWithPin { get; set; }
//     public DateTime ExpiresAt { get; set; }
// }
