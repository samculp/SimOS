using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimOS;

public class Process
{
    public enum ProcessState
    {
        Initial,
        Ready,
        Running,
        Blocked,
        Final
    }

    public int PID { get; }
    public ProcessState State { get; set; }
    public int ProgramCounter { get; set; }

    public Process? Parent { get; }
    public List<Process> Children { get; }

    public Process(int pid, Process? parent = null)
    {
        PID = pid;
        State = ProcessState.Initial;
        ProgramCounter = 0;
        Parent = parent;
        Children = new();
    }
}
