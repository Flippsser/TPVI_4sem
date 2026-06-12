using Microsoft.EntityFrameworkCore;

namespace DAL_Celebrity_MSSQL;

public sealed class Context : DbContext
{
    private readonly string? connectionString;

    public Context(string connectionString)
    {
        this.connectionString = connectionString;
    }

    public Context(DbContextOptions<Context> options)
        : base(options)
    {
    }

    public DbSet<Celebrity> Celebrities => Set<Celebrity>();
    public DbSet<Lifeevent> Lifeevents => Set<Lifeevent>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured && !string.IsNullOrWhiteSpace(connectionString))
        {
            optionsBuilder.UseSqlServer(connectionString);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Celebrity>(entity =>
        {
            entity.ToTable("Celebrities");
            entity.HasKey(celebrity => celebrity.Id);
            entity.Property(celebrity => celebrity.FullName)
                .HasMaxLength(128)
                .IsRequired();
            entity.Property(celebrity => celebrity.Nationality)
                .HasMaxLength(2)
                .IsRequired();
            entity.Property(celebrity => celebrity.ReqPhotoPath)
                .HasMaxLength(256);
        });

        modelBuilder.Entity<Lifeevent>(entity =>
        {
            entity.ToTable("Lifeevents");
            entity.HasKey(lifeevent => lifeevent.Id);
            entity.Property(lifeevent => lifeevent.Description)
                .HasMaxLength(512)
                .IsRequired();
            entity.Property(lifeevent => lifeevent.ReqPhotoPath)
                .HasMaxLength(256);
            entity.HasOne(lifeevent => lifeevent.Celebrity)
                .WithMany(celebrity => celebrity.Lifeevents)
                .HasForeignKey(lifeevent => lifeevent.CelebrityId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
