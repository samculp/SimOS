using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimOS;

public enum ProcessState { Initial, Ready, Running, Blocked, Final }

public class SimProcess
{
    public int PID { get; }
    public ProcessState ProcessState { get; set; }
    public CpuState CpuState { get; }

    public SimProcess? Parent { get; }
    public List<SimProcess> Children { get; }
    public List<SimInstruction> Program { get; }

    public SimProcess(int pid, List<SimInstruction> program, SimProcess? parent = null)
    {
        PID = pid;
        ProcessState = ProcessState.Initial;

        CpuState = new();

        Parent = parent;
        Children = new();
        Program = program;
    }
}

public class CpuState
{
    public int ProgramCounter { get; set; }
    public int[] Registers { get; }
    public CpuState()
    {
        ProgramCounter = 0;
        Registers = new int[8];
    }
}