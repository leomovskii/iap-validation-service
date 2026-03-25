using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

internal class Program {
	private static void Main(string[] args) {
		var builder = WebApplication.CreateBuilder(args);

		builder.Services.AddDbContext<AppDbContext>(opt =>
			opt.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));

		builder.Services.AddSingleton<IConnectionMultiplexer>(
			ConnectionMultiplexer.Connect(builder.Configuration["Redis:ConnectionString"]!));

		builder.Services.AddHttpClient<GooglePlayVerifier>();
		builder.Services.AddHttpClient<AppleVerifier>();
		builder.Services.AddScoped<PurchaseRepository>();

		var app = builder.Build();
		app.MapPurchaseRoutes();
		app.Run();
	}
}