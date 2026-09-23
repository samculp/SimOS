using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace SimOS;

public sealed class EventLogger
{
    private static EventLogger _instance;
    private static readonly object _lock = new();

    public static EventLogger Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                        _instance = new();
                }
            }

            return _instance;
        }
    }

    private EventLogger() { }

    public ObservableCollection<SimEvent> SimEvents { get; } = new();
}

public enum SimEventType
{
    CPU, TRAP, MEMORY, OS, SCHED
}

public class SimEvent
{
    public DateTime Timestamp { get; set; }
    public SimEventType EventType { get; set; }
    public string Message { get; set; } = "";
    public override string ToString()
    {
        return $"[{Timestamp:HH:mm:ss.fff}]\t{EventType}\t{Message}";
    }
}