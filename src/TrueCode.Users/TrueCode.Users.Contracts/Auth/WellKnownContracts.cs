using System.Text.Json.Serialization;

namespace TrueCode.Users.Contracts.Auth;

public sealed record DiscoveryDocument(
    [property: JsonPropertyName("issuer")] string Issuer,
    [property: JsonPropertyName("jwks_uri")] string JwksUri,
    [property: JsonPropertyName("id_token_signing_alg_values_supported")] string[] IdTokenSigningAlgValuesSupported);
