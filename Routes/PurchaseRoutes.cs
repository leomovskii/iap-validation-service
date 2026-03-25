internal static class PurchaseRoutes {
	public static void MapPurchaseRoutes(this WebApplication app) {
		app.MapPost("/api/verify-purchase", async (PurchaseRequest req, IServiceProvider services, PurchaseRepository repo, ILogger<Program> logger) => {
			if (string.IsNullOrWhiteSpace(req.PurchaseToken))
				return Results.BadRequest(new { error = "Missing token" });

			var verifier = req.Platform.ToLower() switch {
				"google" => services.GetRequiredService<GooglePlayVerifier>(),
				"apple" => (IStoreVerifier) services.GetRequiredService<AppleVerifier>(),
				_ => null
			};

			if (verifier is null)
				return Results.BadRequest(new { error = "Unknown platform" });

			var isValid = await verifier.VerifyAsync(req.ProductId, req.PurchaseToken);
			if (!isValid) {
				logger.LogWarning("[FRAUD] Invalid receipt. userId={U}", req.UserId);
				return Results.BadRequest(new { error = "Invalid receipt" });
			}

			if (await repo.IsTokenUsedAsync(req.PurchaseToken)) {
				logger.LogWarning("[FRAUD] Duplicate token. userId={U}", req.UserId);
				return Results.Conflict(new { error = "Already used" });
			}

			await repo.SaveAsync(new Purchase {
				UserId = req.UserId,
				ProductId = req.ProductId,
				PurchaseToken = req.PurchaseToken,
				Platform = req.Platform
			});

			return Results.Ok(new { success = true });
		});
	}
}