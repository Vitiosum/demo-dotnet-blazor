namespace cc_dotnet_demo.Live;

public sealed record Visitor(int Number, DateTime Since, int Hue);

public sealed record LiveEvent(long Seq, DateTime At, int Number, string Kind);

/// <summary>Instantané immuable de la salle, rendu par chaque circuit.</summary>
public sealed record RoomSnapshot(
    IReadOnlyList<Visitor> Visitors,
    IReadOnlyList<LiveEvent> Events,
    IReadOnlyList<int> History,
    long Tick,
    long Pushed,
    long Signals,
    long LastSignalSeq,
    int LastSignalBy,
    DateTime ServerTime);

/// <summary>
/// État partagé par tous les circuits Blazor de l'instance : navigateurs connectés, journal, battement serveur.
/// Il vit en mémoire : c'est pour cela que la démo tourne sur une seule instance.
/// </summary>
public sealed class LiveRoom
{
    const int MaxEvents = 6;
    const int MaxHistory = 36;

    readonly object _gate = new();
    readonly Dictionary<string, Visitor> _known = new();      // circuitId → visiteur (numéro stable)
    readonly HashSet<string> _online = new();
    readonly LinkedList<LiveEvent> _events = new();
    readonly Queue<int> _history = new(Enumerable.Repeat(0, MaxHistory));
    readonly Dictionary<int, DateTime> _lastSignal = new();
    int _nextNumber;
    long _seq, _tick, _pushed, _signals, _lastSignalSeq;
    int _lastSignalBy, _pushesThisTick;
    RoomSnapshot _snapshot;

    public LiveRoom() => _snapshot = Build();

    /// <summary>Déclenché après chaque changement ; chaque abonné re-rend son composant (= un message SignalR).</summary>
    public event Action<RoomSnapshot>? Changed;

    public RoomSnapshot Snapshot => _snapshot;

    public int Join(string circuitId)
    {
        int number;
        lock (_gate)
        {
            if (!_known.TryGetValue(circuitId, out var v))
            {
                number = ++_nextNumber;
                v = new Visitor(number, DateTime.UtcNow, (number - 1) % 6);
                _known[circuitId] = v;
            }
            number = v.Number;
            if (!_online.Add(circuitId)) return number;
            Log(number, "join");
        }
        Publish();
        return number;
    }

    public void Leave(string circuitId, bool forget)
    {
        lock (_gate)
        {
            var wasOnline = _online.Remove(circuitId);
            if (_known.TryGetValue(circuitId, out var v))
            {
                if (forget) { _known.Remove(circuitId); _lastSignal.Remove(v.Number); }
                if (!wasOnline) return;
                Log(v.Number, "leave");
            }
            else return;
        }
        Publish();
    }

    /// <summary>Un navigateur envoie un signal à tous les autres (limité à un toutes les 400 ms).</summary>
    public bool Signal(int number)
    {
        lock (_gate)
        {
            if (number <= 0) return false;
            var now = DateTime.UtcNow;
            if (_lastSignal.TryGetValue(number, out var last) && (now - last).TotalMilliseconds < 400) return false;
            _lastSignal[number] = now;
            _signals++;
            _lastSignalBy = number;
            Log(number, "signal");
            _lastSignalSeq = _seq;
        }
        Publish();
        return true;
    }

    /// <summary>Battement serveur, appelé par <see cref="PulseService"/> toutes les PULSE_MS.</summary>
    public void Tick()
    {
        lock (_gate)
        {
            _tick++;
            _history.Dequeue();
            _history.Enqueue(_pushesThisTick);
            _pushesThisTick = 0;
            if (_online.Count == 0) { _snapshot = Build(); return; }
        }
        Publish();
    }

    void Log(int number, string kind)
    {
        _events.AddFirst(new LiveEvent(++_seq, DateTime.UtcNow, number, kind));
        while (_events.Count > MaxEvents) _events.RemoveLast();
    }

    void Publish()
    {
        RoomSnapshot snap;
        lock (_gate)
        {
            var subscribers = Changed?.GetInvocationList().Length ?? 0;
            _pushed += subscribers;
            _pushesThisTick += subscribers;
            snap = _snapshot = Build();
        }
        Changed?.Invoke(snap);
    }

    RoomSnapshot Build() => new(
        _online.Select(id => _known[id]).OrderBy(v => v.Number).ToArray(),
        _events.ToArray(),
        _history.ToArray(),
        _tick, _pushed, _signals, _lastSignalSeq, _lastSignalBy, DateTime.UtcNow);
}
