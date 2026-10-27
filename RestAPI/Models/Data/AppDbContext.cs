using Microsoft.EntityFrameworkCore;

namespace RestAPI.Models.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Workshop> Workshops => Set<Workshop>();
    public DbSet<Worker> Workers => Set<Worker>();
    public DbSet<Workwear> Workwears => Set<Workwear>();
    public DbSet<Receipt> Receipts => Set<Receipt>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Workshop>(entity =>
        {
            entity.ToTable("workshops");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").HasColumnType("character varying").IsRequired();
            entity.Property(e => e.HeadFullName).HasColumnName("head_full_name").HasColumnType("character varying").IsRequired();
            entity.HasMany(e => e.Workers).WithOne(e => e.Workshop).HasForeignKey(e => e.WorkshopId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Worker>(entity =>
        {
            entity.ToTable("workers");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.FullName).HasColumnName("full_name").HasColumnType("character varying").IsRequired();
            entity.Property(e => e.Position).HasColumnName("position").HasColumnType("character varying").IsRequired();
            entity.Property(e => e.DiscountPercent).HasColumnName("discount_percent").HasColumnType("numeric(5,2)");
            entity.Property(e => e.WorkshopId).HasColumnName("workshop_id");
            entity.HasMany(e => e.Receipts).WithOne(e => e.Worker).HasForeignKey(e => e.WorkerId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Workwear>(entity =>
        {
            entity.ToTable("workwears");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Type).HasColumnName("type").HasColumnType("character varying").IsRequired();
            entity.Property(e => e.WearMonths).HasColumnName("wear_months");
            entity.Property(e => e.UnitCost).HasColumnName("unit_cost").HasColumnType("numeric(12,2)");
            entity.HasMany(e => e.Receipts).WithOne(e => e.Workwear).HasForeignKey(e => e.WorkwearId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Receipt>(entity =>
        {
            entity.ToTable("receipts");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.WorkerId).HasColumnName("worker_id");
            entity.Property(e => e.WorkwearId).HasColumnName("workwear_id");
            entity.Property(e => e.ReceivedAt).HasColumnName("received_at");
            entity.Property(e => e.Signed).HasColumnName("signed");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(e => e.Login);
            entity.Property(e => e.Login).HasColumnName("login").HasColumnType("character varying");
            entity.Property(e => e.Password).HasColumnName("password").HasColumnType("character varying").IsRequired();
        });
    }
}
