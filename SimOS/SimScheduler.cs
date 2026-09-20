using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimOS;

public class SimScheduler
{
    private readonly Queue<SimProcess> _readyQueue = new();

    public void Add(SimProcess process)
    {
        _readyQueue.Enqueue(process);
    }

    public SimProcess? GetNext()
    {
        if (_readyQueue.Count == 0)
            return null;

        return _readyQueue.Dequeue();
    }
}
