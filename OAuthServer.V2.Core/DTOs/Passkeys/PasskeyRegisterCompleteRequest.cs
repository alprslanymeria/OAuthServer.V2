using System.Text.Json;

namespace OAuthServer.V2.Core.DTOs.Passkeys;

public record PasskeyRegisterCompleteRequest(string RequestId, JsonElement AttestationResponse);
