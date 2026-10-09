namespace TrueCode.Users.Contracts.Auth;

public sealed record DiscoveryDocument(
    string Issuer, string JwksUri, string[] IdTokenSigningAlgValuesSupported);
