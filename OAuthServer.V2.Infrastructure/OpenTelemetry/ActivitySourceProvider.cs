using System.Diagnostics;

namespace OAuthServer.V2.Infrastructure.OpenTelemetry;

public static class ActivitySourceProvider
{
    public static ActivitySource Source = null!;
}
