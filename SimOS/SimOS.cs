using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

namespace SimOS;

public enum TrapEntry { SYSCALL, TIMER_INTERRUPT, PROCESS_EXIT }

public class SimOS
{
    public ProcessList ProcessList { get; } = new();
    public SimCpu Cpu { get; }
    public SimScheduler Scheduler { get; } = new();
    public SimOS(SimCpu cpu)
    {
        Cpu = cpu;
        PopulateTrapTable();
    }

    public void Run()
    {
        List<SimInstruction> program1 = new()
        {
            new SimInstruction(Opcode.LD, Register.R0, immediate: 10),
            new SimInstruction(Opcode.LD, Register.R1, immediate: 20),
            new SimInstruction(Opcode.ADD, Register.R0, Register.R1),
            new SimInstruction(Opcode.HLT)
        };

        List<SimInstruction> program2 = new()
        {
            new SimInstruction(Opcode.LD, Register.R0, immediate: 100),
            new SimInstruction(Opcode.LD, Register.R1, immediate: 200),
            new SimInstruction(Opcode.ADD, Register.R0, Register.R1),
            new SimInstruction(Opcode.HLT)
        };

        List<SimInstruction> program3 = new()
        {
            new SimInstruction(Opcode.LD, Register.R0, immediate: 1000),
            new SimInstruction(Opcode.LD, Register.R1, immediate: 2000),
            new SimInstruction(Opcode.ADD, Register.R0, Register.R1),
            new SimInstruction(Opcode.HLT)
        };

        SimProcess proc1 = new(100, program1);
        SimProcess proc2 = new(101, program2);
        SimProcess proc3 = new(102, program3);

        proc1.ProcessState = ProcessState.Ready;
        proc2.ProcessState = ProcessState.Ready;
        proc3.ProcessState = ProcessState.Ready;

        ProcessList.Add(proc1);
        ProcessList.Add(proc2);
        ProcessList.Add(proc3);

        Scheduler.Add(proc1);
        Scheduler.Add(proc2);
        Scheduler.Add(proc3);

        ProcessList.DisplayProcesses();
        Schedule();

        while (Cpu.CurrentProcess != null && Cpu.CurrentProcess.ProcessState != ProcessState.Final)
            Cpu.Execute();

        ProcessList.DisplayProcesses();
    }
    public void Schedule()
    {
        SimProcess? next = Scheduler.GetNext();

        if (next == null)
            return;

        if (Cpu.CurrentProcess == null)
        {
            next.ProcessState = ProcessState.Running;
            Cpu.LoadProcess(next);
        }
        else
        {
            ContextSwitch(next);
        }
        ProcessList.DisplayProcesses();
    }
    private void PopulateTrapTable()
    {
        var trapTable = Cpu.TrapTable;
        trapTable[(int)TrapEntry.SYSCALL] = HandleSystemCall;
        trapTable[(int)TrapEntry.TIMER_INTERRUPT] = HandleTimerInterrupt;
        trapTable[(int)TrapEntry.PROCESS_EXIT] = HandleProcessExit;
    }
    private void ContextSwitch(SimProcess newProcess)
    {
        Console.WriteLine("OS: Initiating context switch");

        newProcess.ProcessState = ProcessState.Running;

        Cpu.ContextSwitch(newProcess);
    }
    private void HandleSystemCall()
    {
        Console.WriteLine("OS: Handling sysem call...");
        Console.WriteLine("OS: Returning from trap");
        Cpu.ReturnFromTrap();
    }
    private void HandleTimerInterrupt()
    {
        Console.WriteLine("OS: Handling timer interrupt...");

        SimProcess current = Cpu.CurrentProcess!;

        if (current.ProcessState != ProcessState.Final)
        {
            Scheduler.Add(current);
            current.ProcessState = ProcessState.Ready;
        }

        Schedule();

        Console.WriteLine("OS: Returning from trap");
        Console.WriteLine();
        Cpu.ReturnFromTrap();
    }
    private void HandleProcessExit()
    {
        SimProcess current = Cpu.CurrentProcess!;
        Console.WriteLine($"OS: Process {current.PID} exiting...");
        current.ProcessState = ProcessState.Final;
        Schedule();
        Cpu.ReturnFromTrap();
    }
}
