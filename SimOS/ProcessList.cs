using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimOS;

public class ProcessList
{
    private readonly List<Process> processes = new();
    public IReadOnlyList<Process> Processes => processes;

    public void Add(Process process)
    {
        processes.Add(process);
    }
    public void Remove(Process process)
    {
        processes.Remove(process);
    }
    public Process? GetById(int pid)
    {
        return processes.FirstOrDefault(p => p.PID == pid);
    }
}
