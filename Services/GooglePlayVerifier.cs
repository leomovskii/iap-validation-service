using Google.Apis.Auth.OAuth2;
using System.Net.Http.Headers;
using System.Text.Json.Serialization;

internal class GooglePlayVerifier : IStoreVerifier {

	private readonly HttpClient _http;
	private readonly IConfiguration _config;

	public GooglePlayVerifier(HttpClient http, IConfiguration config) {
		_http = http;
		_config = config;
	}

	public async Task<bool> VerifyAsync(string productId, string purchaseToken) {
		var packageName = _config["Google:PackageName"];
		var accessToken = await GetAccessTokenAsync();

		_http.DefaultRequestHeaders.Authorization =
			new AuthenticationHeaderValue("Bearer", accessToken);

		var url = $"https://androidpublisher.googleapis.com/androidpublisher/v3" +
				  $"/applications/{packageName}/purchases/products/{productId}" +
				  $"/tokens/{purchaseToken}";

		var response = await _http.GetAsync(url);
		if (!response.IsSuccessStatusCode)
			return false;

		var result = await response.Content
			.ReadFromJsonAsync<GooglePurchaseResponse>();

		return result?.PurchaseState == 0; // 0 is purchased
	}

	private async Task<string> GetAccessTokenAsync() {
		var credential = GoogleCredential
			.FromFile(_config["Google:ServiceAccountPath"])
			.CreateScoped("https://www.googleapis.com/auth/androidpublisher");

		return await ((ServiceAccountCredential) credential.UnderlyingCredential)
			.GetAccessTokenForRequestAsync();
	}
}

record GooglePurchaseResponse(
	[property: JsonPropertyName("purchaseState")] int PurchaseState
);