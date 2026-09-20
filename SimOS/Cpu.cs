using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SimOS;

public enum CpuMode { User, Kernel };

public sealed class Cpu
{
    public OperatingSystem OS { get; private set; }
    public CpuMode Mode { get; private set; } = CpuMode.User;
    public Process? CurrentProcess { get; private set; }
    public int[] Registers { get; } = new int[8];
    public int ProgramCounter { get; private set; }
    public Dictionary<int, Action> TrapTable { get; } = new();

    public void BootOS()
    {
        OS = new(this);
    }

    public void LoadProcess(Process process)
    {
        if (process.ProcessState != ProcessState.Initial
            && process.ProcessState != ProcessState.Ready)
        {
            throw new InvalidOperationException("Process cannot be loaded");
        }

        Console.WriteLine($"CPU: Loading process {process.PID}");

        CurrentProcess = process;
        process.ProcessState = ProcessState.Running;

        ProgramCounter = process.CpuState.ProgramCounter;

        Array.Copy(
            process.CpuState.Registers,
            Registers,
            Registers.Length);
    }
    public void SaveProcessState(Process process)
    {
        if (CurrentProcess == null)
        {
            throw new InvalidOperationException("No process loaded");
        }
        
        Console.WriteLine($"CPU: Saving process {process.PID}");

        process.CpuState.ProgramCounter = ProgramCounter;
        Array.Copy(
            Registers,
            process.CpuState.Registers,
            Registers.Length);
    }
    public void Execute()
    {
        if (CurrentProcess == null)
        {
            throw new InvalidOperationException("No process loaded.");
        }
        if (CurrentProcess.ProcessState == ProcessState.Final)
        {
            throw new InvalidOperationException("Process has already terminated.");
        }

        Instruction instruction = CurrentProcess.Program[ProgramCounter];

        Console.WriteLine(
            $"PID: {CurrentProcess.PID} ({CurrentProcess.ProcessState})    " +
            $"PC: {ProgramCounter}    " +
            $"Instruction: {instruction}");

        Console.WriteLine();
        PrintRegisterContents();
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
                CurrentProcess.ProcessState = ProcessState.Final;
                break;
        }

        ProgramCounter++;
    }
    public void HandleTrap(int trapNumber)
    {
        SaveProcessState(CurrentProcess!);
        EnterKernelMode();

        Console.WriteLine(
            $"TRAP {trapNumber}: PID {CurrentProcess!.PID} " +
            $"- entering kernel mode");

        // simulates the CPU going to the trap table, grabbing the address specified by the OS, and navigating to that address
        if (!TrapTable.TryGetValue(trapNumber, out var handler))
        {
            throw new InvalidOperationException($"Trap number not registered: {trapNumber}");
        }
        
        handler();
    }
    public void ContextSwitch(Process newProcess)
    {
        if (CurrentProcess != null)
        {
            SaveProcessState(CurrentProcess);
            CurrentProcess.ProcessState = ProcessState.Ready;
        }
        LoadProcess(newProcess);
    }
    public void ReturnFromTrap()
    {
        ReturnToUserMode();
    }
    public void EnterKernelMode()
    {
        Mode = CpuMode.Kernel;
    }
    public void ReturnToUserMode()
    {
        Mode = CpuMode.User;
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
}
