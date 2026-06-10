using System.Diagnostics;

namespace WorldCup.Api.Common.Observability;

/// <summary>Shared OpenTelemetry ActivitySource for custom spans (scoring engine, external provider calls).</summary>
public static class Telemetry
{
    public const string ServiceName = "WorldCup.Api";
    public static readonly ActivitySource Source = new(ServiceName);
}
