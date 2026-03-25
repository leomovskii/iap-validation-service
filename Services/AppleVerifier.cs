using System.Text.Json.Serialization;

internal class AppleVerifier : IStoreVerifier {

	private readonly HttpClient _http;
	private readonly IConfiguration _config;

	public AppleVerifier(HttpClient http, IConfiguration config) {
		_http = http;
		_config = config;
	}

	public async Task<bool> VerifyAsync(string productId, string purchaseToken) {
		var payload = new {
			receiptData = purchaseToken,
			password = _config["Apple:SharedSecret"]
		};

		// production -> sandbox

		var url = "https://buy.itunes.apple.com/verifyReceipt";
		var response = await PostAsync(url, payload);

		if (response?.Status == 21007) {
			url = "https://sandbox.itunes.apple.com/verifyReceipt";
			response = await PostAsync(url, payload);
		}

		return response?.Status == 0; // 0 is OK
	}

	private async Task<AppleResponse?> PostAsync(string url, object payload) {
		var res = await _http.PostAsJsonAsync(url, payload);
		return await res.Content.ReadFromJsonAsync<AppleResponse>();
	}
}

record AppleResponse(
	[property: JsonPropertyName("status")] int Status
);