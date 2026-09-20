using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimOS;

public class ProcessList
{
    private readonly List<SimProcess> processes = new();
    public IReadOnlyList<SimProcess> Processes => processes;

    public void Add(SimProcess process)
    {
        processes.Add(process);
    }
    public void Remove(SimProcess process)
    {
        processes.Remove(process);
    }
    public SimProcess? GetById(int pid)
    {
        return processes.FirstOrDefault(p => p.PID == pid);
    }
    public void DisplayProcesses()
    {
        Console.WriteLine();
        Console.WriteLine("---------------------------------");
        foreach (var process in processes)
        {
            Console.WriteLine($"|\tPID: {process.PID} ({process.ProcessState})\t|");
        }
        Console.WriteLine("---------------------------------");
        Console.WriteLine();
    }
}
