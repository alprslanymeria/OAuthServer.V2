using System.Text.Json;

namespace OAuthServer.V2.Core.DTOs.Passkeys;

public record PasskeyLoginCompleteRequest(string RequestId, JsonElement AssertionResponse);
