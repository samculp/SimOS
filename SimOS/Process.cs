using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimOS;

public enum ProcessState { Initial, Ready, Running, Blocked, Final }

public class Process
{
    public int PID { get; }
    public ProcessState ProcessState { get; set; }
    public CpuState CpuState { get; }

    public Process? Parent { get; }
    public List<Process> Children { get; }
    public List<Instruction> Program { get; }

    public Process(int pid, List<Instruction> program, Process? parent = null)
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