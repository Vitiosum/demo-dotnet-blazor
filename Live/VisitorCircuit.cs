using Microsoft.AspNetCore.Components.Server.Circuits;

namespace cc_dotnet_demo.Live;

/// <summary>
/// Suit le cycle de vie du circuit SignalR de chaque navigateur. La déconnexion est vue dès la coupure du
/// WebSocket (OnConnectionDown), sans attendre l'expiration du circuit.
/// </summary>
public sealed class VisitorCircuit(LiveRoom room) : CircuitHandler
{
    string? _id;

    /// <summary>Numéro du navigateur dans la salle (0 tant que la connexion n'est pas établie).</summary>
    public int Number { get; private set; }

    public override Task OnConnectionUpAsync(Circuit circuit, CancellationToken ct)
    {
        _id = circuit.Id;
        Number = room.Join(circuit.Id);
        return Task.CompletedTask;
    }

    public override Task OnConnectionDownAsync(Circuit circuit, CancellationToken ct)
    {
        room.Leave(circuit.Id, forget: false);
        return Task.CompletedTask;
    }

    public override Task OnCircuitClosedAsync(Circuit circuit, CancellationToken ct)
    {
        room.Leave(circuit.Id, forget: true);
        return Task.CompletedTask;
    }
}
