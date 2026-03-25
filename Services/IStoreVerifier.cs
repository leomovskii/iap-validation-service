internal interface IStoreVerifier {
	Task<bool> VerifyAsync(string productId, string purchaseToken);
}