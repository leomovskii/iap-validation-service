using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext {
	public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
	public DbSet<Purchase> Purchases => Set<Purchase>();

	protected override void OnModelCreating(ModelBuilder model) {
		model.Entity<Purchase>().HasIndex(p => p.PurchaseToken).IsUnique();
	}
}

public class Purchase {
	public int Id { get; set; }
	public string UserId { get; set; } = "";
	public string ProductId { get; set; } = "";
	public string PurchaseToken { get; set; } = "";
	public string Platform { get; set; } = "";
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}