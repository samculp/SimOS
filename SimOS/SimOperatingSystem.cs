using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace SimOS;

public enum TrapEntry { SYSCALL, TIMER_INTERRUPT, PROCESS_EXIT }

public class SimOperatingSystem
{
    public ProcessList ProcessList { get; } = new();
    public SimCpu Cpu { get; }
    public SimScheduler Scheduler { get; } = new();
    public SimOperatingSystem(SimCpu cpu)
    {
        Cpu = cpu;
        PopulateTrapTable();
    }
    private void LoadPrograms()
    {
        ProcessList.Clear();
        Scheduler.Clear();

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

        List<SimInstruction> program4 = new()
        {
            new SimInstruction(Opcode.LD, Register.R0, immediate: 3000),
            new SimInstruction(Opcode.LD, Register.R1, immediate: 4000),
            new SimInstruction(Opcode.ADD, Register.R0, Register.R1),
            new SimInstruction(Opcode.HLT)
        };

        SimProcess proc1 = new(100, program1);
        SimProcess proc2 = new(101, program2);
        SimProcess proc3 = new(102, program3);
        SimProcess proc4 = new(103, program4);

        proc1.ProcessState = ProcessState.Ready;
        proc2.ProcessState = ProcessState.Ready;
        proc3.ProcessState = ProcessState.Ready;
        proc4.ProcessState = ProcessState.Ready;

        ProcessList.Add(proc1);
        ProcessList.Add(proc2);
        ProcessList.Add(proc3);
        ProcessList.Add(proc4);

        Scheduler.Add(proc1);
        Scheduler.Add(proc2);
        Scheduler.Add(proc3);
        Scheduler.Add(proc4);
    }
    public void Run()
    {
        EventLogger.Instance.SimEvents.Clear();
        EventLogger.Instance.SimEvents.Insert(0, new SimEvent
        {
            Timestamp = DateTime.Now,
            EventType = SimEventType.OS,
            Message = $"Setting up environment"
        });
        LoadPrograms();
        Cpu.ResetCpuState();
    }
    public void Step()
    {
        if (Cpu.CurrentProcess == null || Cpu.CurrentProcess.ProcessState == ProcessState.Final)
        {
            Schedule();
        }
        else
        {
            Cpu.ExecuteCurrentProcess();
        }
    }
    public void Schedule()
    {
        SimProcess? next = Scheduler.GetNext();

        if (next == null)
            return;

        if (Cpu.CurrentProcess == null)
        {
            next.ProcessState = ProcessState.Running;
            EventLogger.Instance.SimEvents.Insert(0, new SimEvent
            {
                Timestamp = DateTime.Now,
                EventType = SimEventType.SCHED,
                Message = $"Scheduling process {next.PID}"
            });
            Cpu.LoadProcess(next);
        }
        else
        {
            ContextSwitch(next);
        }
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
        SimProcess? current = Cpu.CurrentProcess;
        newProcess.ProcessState = ProcessState.Running;

        if (current != null)
        {
            EventLogger.Instance.SimEvents.Insert(0, new SimEvent
            {
                Timestamp = DateTime.Now,
                EventType = SimEventType.SCHED,
                Message = $"Saving process {current.PID}"
            });
            Cpu.SaveProcessState(current);
        }

        EventLogger.Instance.SimEvents.Insert(0, new SimEvent
        {
            Timestamp = DateTime.Now,
            EventType = SimEventType.SCHED,
            Message = $"Scheduling process {newProcess.PID}"
        });
        Cpu.LoadProcess(newProcess);
    }
    private void HandleSystemCall()
    {
        EventLogger.Instance.SimEvents.Insert(0, new SimEvent
        {
            Timestamp = DateTime.Now,
            EventType = SimEventType.OS,
            Message = "Handling system call..."
        });
        EventLogger.Instance.SimEvents.Insert(0, new SimEvent
        {
            Timestamp = DateTime.Now,
            EventType = SimEventType.OS,
            Message = "Returning from trap"
        });
        Cpu.ReturnFromTrap();
    }
    private void HandleTimerInterrupt()
    {
        EventLogger.Instance.SimEvents.Insert(0, new SimEvent
        {
            Timestamp = DateTime.Now,
            EventType = SimEventType.OS,
            Message = "Handling timer interrupt..."
        });

        SimProcess current = Cpu.CurrentProcess!;

        if (current.ProcessState != ProcessState.Final)
        {
            Scheduler.Add(current);
            current.ProcessState = ProcessState.Ready;
        }

        Schedule();

        EventLogger.Instance.SimEvents.Insert(0, new SimEvent
        {
            Timestamp = DateTime.Now,
            EventType = SimEventType.OS,
            Message = "Returning from trap"
        });
        Cpu.ReturnFromTrap(restoreCpu: false);
    }
    private void HandleProcessExit()
    {
        SimProcess current = Cpu.CurrentProcess!;

        EventLogger.Instance.SimEvents.Insert(0, new SimEvent
        {
            Timestamp = DateTime.Now,
            EventType = SimEventType.OS,
            Message = $"Process {current.PID} exiting..."
        });

        current.ProcessState = ProcessState.Final;

        Schedule();

        EventLogger.Instance.SimEvents.Insert(0, new SimEvent
        {
            Timestamp = DateTime.Now,
            EventType = SimEventType.OS,
            Message = "Returning from trap"
        });

        Cpu.ReturnFromTrap(restoreCpu: false);
    }
}
