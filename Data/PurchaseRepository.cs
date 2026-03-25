using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

public class PurchaseRepository {

	private readonly AppDbContext _db;
	private readonly IConnectionMultiplexer _redis;

	public PurchaseRepository(AppDbContext db, IConnectionMultiplexer redis) {
		_db = db;
		_redis = redis;
	}

	public async Task<bool> IsTokenUsedAsync(string token) {
		var cache = _redis.GetDatabase();
		var cached = await cache.StringGetAsync($"iap:{token}");
		if (cached.HasValue)
			return true;

		return await _db.Purchases.AnyAsync(p => p.PurchaseToken == token);
	}

	public async Task SaveAsync(Purchase purchase) {
		_db.Purchases.Add(purchase);
		await _db.SaveChangesAsync();

		var cache = _redis.GetDatabase();
		await cache.StringSetAsync($"iap:{purchase.PurchaseToken}", "1", TimeSpan.FromDays(30));
	}
}