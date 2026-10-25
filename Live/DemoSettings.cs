namespace cc_dotnet_demo.Live;

/// <summary>
/// Variables lues au démarrage : variables de démo (onglet « Variables d'environnement » de la console Clever)
/// et variables injectées par la plateforme. Une modification se prend en compte après <c>clever restart</c>.
/// </summary>
public static class DemoSettings
{
    public const string Injected = "injecté au déploiement";
    public const int DefaultPulseMs = 1000;
    public const string DefaultRoom = "Salle de démo";

    static string? G(string key)
    {
        var v = Environment.GetEnvironmentVariable(key)?.Trim();
        return string.IsNullOrEmpty(v) ? null : v;
    }

    static string? Cut(string? v, int n) => v is null ? null : (v.Length > n ? v[..n] : v);

    // --- Variables de démo ---
    /// <summary>APP_ENV : pastille d'environnement (dégradé Clever si « production »). Défaut « local ».</summary>
    public static readonly string AppEnv = G("APP_ENV") ?? "local";
    public static bool IsProduction => AppEnv.Equals("production", StringComparison.OrdinalIgnoreCase);

    /// <summary>ANNOUNCEMENT : bandeau en haut de toutes les pages si non vide.</summary>
    public static readonly string Announcement = G("ANNOUNCEMENT") ?? "";

    /// <summary>ROOM_NAME : nom de la salle affiché au-dessus des navigateurs connectés.</summary>
    public static readonly string RoomName = Cut(G("ROOM_NAME"), 60) ?? DefaultRoom;

    /// <summary>PULSE_MS : cadence à laquelle le serveur pousse l'état (250 à 10 000 ms, sinon 1 000).</summary>
    public static readonly int PulseMs = ParsePulse(G("PULSE_MS"));

    public static int ParsePulse(string? raw) =>
        int.TryParse(raw, out var ms) && ms >= 250 && ms <= 10_000 ? ms : DefaultPulseMs;

    // --- Variables injectées par Clever Cloud ---
    public static readonly bool Live = G("APP_ID") is not null;
    public static readonly string? AppName = G("CC_APP_NAME");
    public static readonly string? Instance = G("INSTANCE_NUMBER") is string n
        ? $"#{n}" + (G("CC_PRETTY_INSTANCE_NAME") is string p ? $" · {p}" : "")
        : null;
    public static readonly string? InstanceType = G("INSTANCE_TYPE");
    public static readonly string? Commit = Cut(G("CC_COMMIT_ID"), 7);
    public static readonly string? Deployment = Cut(G("CC_DEPLOYMENT_ID"), 16);
}
