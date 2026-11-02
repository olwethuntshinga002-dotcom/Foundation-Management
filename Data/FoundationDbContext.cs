using System;
using System.Collections.Generic;
using FoundationManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace FoundationManagement.Data;

public partial class FoundationDbContext : DbContext
{
    public FoundationDbContext(DbContextOptions<FoundationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Donation> Donations { get; set; }

    public virtual DbSet<Donor> Donors { get; set; }

    public virtual DbSet<FoodDonation> FoodDonations { get; set; }

    public virtual DbSet<Person> Persons { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Donation>(entity =>
        {
            entity.HasKey(e => e.DonationId).HasName("PK__DONATION__E3A7BA920E568B03");

            entity.ToTable("DONATIONS");

            entity.Property(e => e.DonationId).HasColumnName("Donation_ID");
            entity.Property(e => e.CreatedAt)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("Created_At");
            entity.Property(e => e.DonationDate)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("Donation_Date");
            entity.Property(e => e.DonorId).HasColumnName("Donor_ID");
            entity.Property(e => e.ItemCount).HasColumnName("Item_Count");
            entity.Property(e => e.RecordedBy)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("Recorded_By");

            entity.HasOne(d => d.Donor).WithMany(p => p.Donations)
                .HasForeignKey(d => d.DonorId)
                .HasConstraintName("FK__DONATIONS__Donor__70DDC3D8");
        });

        modelBuilder.Entity<Donor>(entity =>
        {
            entity.HasKey(e => e.DonorId).HasName("PK__DONOR__2E4F2165B53F659B");

            entity.ToTable("DONOR");

            entity.Property(e => e.DonorId)
                .ValueGeneratedNever()
                .HasColumnName("Donor_ID");
            entity.Property(e => e.DonorType)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("Donor_Type");
            entity.Property(e => e.PersonId).HasColumnName("Person_ID");

            entity.HasOne(d => d.Person).WithMany(p => p.Donors)
                .HasForeignKey(d => d.PersonId)
                .HasConstraintName("FK__DONOR__Person_ID__5CD6CB2B");
        });

        modelBuilder.Entity<FoodDonation>(entity =>
        {
            entity.HasKey(e => e.FdId).HasName("PK__FOOD_DON__5842694F36EF7212");

            entity.ToTable("FOOD_DONATIONS");

            entity.Property(e => e.FdId).HasColumnName("FD_ID");
            entity.Property(e => e.DonationId).HasColumnName("Donation_ID");
            entity.Property(e => e.ExpiryDate)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.FoodType)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("Food_Type");
            entity.Property(e => e.ItemWeight)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("Item_Weight");
            entity.Property(e => e.UnitOfMeasure)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("Unit_of_Measure");

            entity.HasOne(d => d.Donation).WithMany(p => p.FoodDonations)
                .HasForeignKey(d => d.DonationId)
                .HasConstraintName("FK__FOOD_DONA__Donat__73BA3083");
        });

        modelBuilder.Entity<Person>(entity =>
        {
            entity.HasKey(e => e.PersonId).HasName("PK__PERSONS__7EABD08B2364AC96");

            entity.ToTable("PERSONS");

            entity.Property(e => e.PersonId)
                .ValueGeneratedNever()
                .HasColumnName("Person_ID");
            entity.Property(e => e.Cellnum)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Fname)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("FName");
            entity.Property(e => e.Gender)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Lname)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("LName");
            entity.Property(e => e.Username)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
