using BlueMarina.Domain.Entities;
using BlueMarina.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BlueMarina.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }


    public DbSet<UserProfile> UserProfiles { get; set; }
    public DbSet<Address> Addresses { get; set; }
    public DbSet<BankAccount> BankAccounts { get; set; }
    public DbSet<KycDocument> KycDocuments { get; set; }
    public DbSet<KycVerification> KycVerifications { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ---- 1. User ----
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.HasIndex(u => u.Email).IsUnique();
            entity.HasIndex(u => u.PhoneNumber).IsUnique();
        });

        // ---- 2. UserProfile ----
        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.HasKey(p => p.Id);

            // Unique constraints for BVN, NIN, and ReferralCode (only when not null)
            entity.HasIndex(p => p.Bvn).IsUnique().HasFilter("[Bvn] IS NOT NULL");
            entity.HasIndex(p => p.Nin).IsUnique().HasFilter("[Nin] IS NOT NULL");
            entity.HasIndex(p => p.ReferralCode).IsUnique().HasFilter("[ReferralCode] IS NOT NULL");

            entity.HasIndex(p => p.KycLevel);

            entity.HasQueryFilter(p => !p.IsDeleted);
        });

        // ---- 3. Address----
        modelBuilder.Entity<Address>(entity =>
        {
            entity.HasKey(a => a.Id);

            entity.HasIndex(a => new { a.UserId, a.Type });
            entity.HasIndex(a => a.IsVerified);

            // Relationship: UserProfile → Addresses (1 to Many)
            entity.HasOne(a => a.UserProfile)
                  .WithMany(p => p.Addresses)
                  .HasForeignKey(a => a.UserId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasQueryFilter(a => !a.IsDeleted);
        });

        // ---- 4. BankAccount (Child of UserProfile) ----
        modelBuilder.Entity<BankAccount>(entity =>
        {
            entity.HasKey(b => b.Id);

            // Composite unique: a user cannot add the same bank + account number twice
            entity.HasIndex(b => new { b.BankCode, b.AccountNumber }).IsUnique();

            // Index for filtering by user and currency
            entity.HasIndex(b => new { b.UserId, b.Currency });

            // Relationship: UserProfile → BankAccounts (1 to Many)
            entity.HasOne(b => b.UserProfile)
                  .WithMany(p => p.BankAccounts)
                  .HasForeignKey(b => b.UserId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasQueryFilter(b => !b.IsDeleted);
        });

        // ---- 5. KycDocument (Child of UserProfile) ----
        modelBuilder.Entity<KycDocument>(entity =>
        {
            entity.HasKey(k => k.Id);

            // One document per type per user at a time
            entity.HasIndex(k => new { k.UserId, k.Type }).IsUnique();

            entity.HasIndex(k => k.Status);

            // Relationship: UserProfile → KycDocuments (1 to Many)
            entity.HasOne(k => k.UserProfile)
                  .WithMany(p => p.KycDocuments)
                  .HasForeignKey(k => k.UserId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasQueryFilter(k => !k.IsDeleted);
        });

        // ---- 6. KycVerification (Child of UserProfile) ----
        modelBuilder.Entity<KycVerification>(entity =>
        {
            entity.HasKey(k => k.Id);

            // Index for most recent checks per user & type – we allow multiple entries for audit history
            entity.HasIndex(k => new { k.UserId, k.Type });
            entity.HasIndex(k => k.Result);

            // Relationship: UserProfile → KycVerifications (1 to Many)
            entity.HasOne(k => k.UserProfile)
                  .WithMany(p => p.KycVerifications)
                  .HasForeignKey(k => k.UserId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasQueryFilter(k => !k.IsDeleted);
        });
    }

}