using System.ComponentModel.DataAnnotations.Schema;
using BlueMarina.Domain.Common;
using BlueMarina.Domain.ValueObjects;

namespace BlueMarina.Domain.Entities.Cash;


[Table("main_wallets")]
public class MainWallet : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public Money BalanceMinor { get; set; }
}
