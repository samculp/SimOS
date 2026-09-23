using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SimOS;

public enum CpuMode { User, Kernel };

public sealed class SimCpu
{
    #region Properties
    public SimOperatingSystem OS { get; private set; }
    public CpuMode Mode { get; private set; } = CpuMode.User;
    public SimProcess? CurrentProcess { get; private set; }
    private CpuState TrapState { get; set; }
    public int[] Registers { get; } = new int[8];
    public int ProgramCounter { get; private set; }
    public Dictionary<int, Action> TrapTable { get; } = new();
    private int InstructionsSinceTimer { get; set; }
    private const int TIMER_QUANTUM = 2;
    #endregion

    public SimCpu()
    {
        OS = new(this);
    }

    #region Methods
    public void LoadProcess(SimProcess process)
    {
        if (process.ProcessState != ProcessState.Running)
        {
            throw new InvalidOperationException("Process cannot be loaded");
        }

        CurrentProcess = process;

        ProgramCounter = process.CpuState.ProgramCounter;
        InstructionsSinceTimer = 0;

        EventLogger.Instance.SimEvents.Insert(0, new SimEvent
        {
            Timestamp = DateTime.Now,
            EventType = SimEventType.CPU,
            Message = $"Loading process {process.PID}"
        });

        Array.Copy(
            process.CpuState.Registers,
            Registers,
            Registers.Length);
    }
    public void SaveProcessState(SimProcess process)
    {
        if (CurrentProcess == null)
        {
            throw new InvalidOperationException("No process loaded");
        }

        process.CpuState.ProgramCounter = ProgramCounter;
        Array.Copy(
            Registers,
            process.CpuState.Registers,
            Registers.Length);
    }
    private CpuState SaveCpuState()
    {
        EventLogger.Instance.SimEvents.Insert(0, new SimEvent
        {
            Timestamp = DateTime.Now,
            EventType = SimEventType.CPU,
            Message = "Saving CPU state"
        });
        return new CpuState
        {
            ProgramCounter = ProgramCounter,
            Registers = (int[])Registers.Clone()
        };
    }
    private void RestoreCpuState()
    {
        EventLogger.Instance.SimEvents.Insert(0, new SimEvent
        {
            Timestamp = DateTime.Now,
            EventType = SimEventType.CPU,
            Message = "Restoring CPU state"
        });

        ProgramCounter = TrapState.ProgramCounter;
        Array.Copy(
            TrapState.Registers,
            Registers,
            Registers.Length);
    }
    public void ExecuteCurrentProcess()
    {
        if (CurrentProcess == null)
        {
            throw new InvalidOperationException("No process loaded.");
        }
        if (CurrentProcess.ProcessState == ProcessState.Final)
        {
            throw new InvalidOperationException("Process has already terminated.");
        }

        SimInstruction instruction = CurrentProcess.Program[ProgramCounter];

        Console.WriteLine(
            $"PID: {CurrentProcess.PID} ({CurrentProcess.ProcessState})    " +
            $"PC: {ProgramCounter}    " +
            $"Instruction: {instruction}");

        Console.WriteLine();
        PrintRegisterContents(); // these are the contents of CPU registers prior to executing the fetched instruction
        Console.WriteLine();

        switch (instruction.Opcode)
        {
            case Opcode.NOP:
                break;

            case Opcode.LD:
                Registers[(int)instruction.Destination!] = instruction.Immediate!.Value;
                break;

            case Opcode.ADD:
                Registers[(int)instruction.Destination!] += Registers[(int)instruction.Source!];
                break;

            case Opcode.SUB:
                Registers[(int)instruction.Destination!] -= Registers[(int)instruction.Source!];
                break;

            case Opcode.SYSCALL:
                HandleTrap(instruction.TrapNumber!.Value);
                    break;

            case Opcode.HLT:
                HandleTrap((int)TrapEntry.PROCESS_EXIT);
                return;
        }

        EventLogger.Instance.SimEvents.Insert(0, new SimEvent
        {
            Timestamp = DateTime.Now,
            EventType = SimEventType.CPU,
            Message = $"Executing instruction {instruction}"
        });

        ProgramCounter++;
        TickTimer();
    }
    private void TickTimer()
    {
        InstructionsSinceTimer++;

        if (InstructionsSinceTimer >= TIMER_QUANTUM)
        {
            InstructionsSinceTimer = 0;
            HandleTrap((int)TrapEntry.TIMER_INTERRUPT);
        }
    }
    public void HandleTrap(int trapNumber)
    {
        EventLogger.Instance.SimEvents.Insert(0, new SimEvent
        {
            Timestamp = DateTime.Now,
            EventType = SimEventType.TRAP,
            Message = $"PID {CurrentProcess!.PID} -> TRAP {trapNumber}"
        });

        EnterKernelMode();

        TrapState = SaveCpuState();

        // simulates the CPU going to the trap table, grabbing the address specified by the OS, and navigating to that address
        if (!TrapTable.TryGetValue(trapNumber, out var handler))
        {
            throw new InvalidOperationException($"Trap number not registered: {trapNumber}");
        }
        
        handler();
    }
    public void ReturnFromTrap(bool restoreCpu = true)
    {
        if (restoreCpu)
        {
            RestoreCpuState();
        }
        
        ReturnToUserMode();
    }
    public void ResetCpuState()
    {
        CurrentProcess = null;
        for (int i = 0; i < Registers.Length; i++)
        {
            Registers[i] = 0;
        }
        ProgramCounter = 0;
        InstructionsSinceTimer = 0;

        EventLogger.Instance.SimEvents.Insert(0, new SimEvent
        {
            Timestamp = DateTime.Now,
            EventType = SimEventType.CPU,
            Message = "Resetting CPU state"
        });
    }
    public void EnterKernelMode()
    {
        Mode = CpuMode.Kernel;
        EventLogger.Instance.SimEvents.Insert(0, new SimEvent
        {
            Timestamp = DateTime.Now,
            EventType = SimEventType.CPU,
            Message = "Entering kernel mode"
        });
    }
    public void ReturnToUserMode()
    {
        Mode = CpuMode.User;
        EventLogger.Instance.SimEvents.Insert(0, new SimEvent
        {
            Timestamp = DateTime.Now,
            EventType = SimEventType.CPU,
            Message = "Entering user mode"
        });
    }
    public void PrintRegisterContents()
    {
        Console.WriteLine($"R0: {Registers[0]}");
        Console.WriteLine($"R1: {Registers[1]}");
        Console.WriteLine($"R2: {Registers[2]}");
        Console.WriteLine($"R3: {Registers[3]}");
        Console.WriteLine($"R4: {Registers[4]}");
        Console.WriteLine($"R5: {Registers[5]}");
        Console.WriteLine($"R6: {Registers[6]}");
        Console.WriteLine($"R7: {Registers[7]}");
    }
    #endregion
}
