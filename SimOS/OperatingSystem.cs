using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimOS;

public class OperatingSystem
{
    public ProcessList ProcessList { get; } = new();
    public Cpu Cpu { get; }
    public OperatingSystem(Cpu cpu)
    {
        Cpu = cpu;
        PopulateTrapTable();
    }

    public void Run()
    {
        List<Instruction> program1 = new()
        {
            new Instruction(Opcode.LD, Register.R0, immediate: 10),
            new Instruction(Opcode.LD, Register.R1, immediate: 20),
            new Instruction(Opcode.ADD, Register.R0, Register.R1),
            new Instruction(Opcode.HLT)
        };

        List<Instruction> program2 = new()
        {
            new Instruction(Opcode.LD, Register.R0, immediate: 100),
            new Instruction(Opcode.LD, Register.R1, immediate: 202),
            new Instruction(Opcode.ADD, Register.R0, Register.R1),
            new Instruction(Opcode.HLT)
        };

        Process proc1 = new(100, program1);
        Process proc2 = new(101, program2);

        proc1.ProcessState = ProcessState.Ready;
        proc2.ProcessState = ProcessState.Ready;

        ProcessList.Add(proc1);
        ProcessList.Add(proc2);

        Cpu.LoadProcess(proc1);

        // process 1 runs two instructions
        Cpu.Execute();
        Cpu.Execute();

        // timer interrupt
        Cpu.HandleTrap(1);

        // process 2 runs two instructions
        Cpu.Execute();
        Cpu.Execute();

        // timer interrupt
        Cpu.HandleTrap(1);

        // process 1 runs two more instructions
        Cpu.Execute();
        Cpu.Execute();
    }

    private void PopulateTrapTable()
    {
        var trapTable = Cpu.TrapTable;
        trapTable[0] = HandleSystemCall;
        trapTable[1] = HandleTimerInterrupt;
    }
    private void ContextSwitch(Process newProcess)
    {
        Console.WriteLine("OS: Initiating context switch");
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

        Process current = Cpu.CurrentProcess!;
        Process next = current.PID == 100
            ? ProcessList.GetById(101)!
            : ProcessList.GetById(100)!;

        ContextSwitch(next);

        Console.WriteLine("OS: Returning from trap");
        Cpu.ReturnFromTrap();
    }
}
