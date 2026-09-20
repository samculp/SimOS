using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimOS;

public enum Opcode { NOP, ADD, SUB, LD, SYSCALL, HLT };
public enum Register { R0, R1, R2, R3, R4, R5, R6, R7, R8 };

public class Instruction
{
    public Opcode Opcode { get; }
    public Register? Destination { get; }
    public Register? Source { get; }
    public int? Immediate { get; }
    public int? TrapNumber { get; }

    public Instruction(
        Opcode opcode, 
        Register? destination = null, 
        Register? source = null, 
        int? immediate = null,
        int? trapNumber = null)
    {
        Opcode = opcode;
        Destination = destination;
        Source = source;
        Immediate = immediate;
        TrapNumber = trapNumber;
    }

    public override string ToString()
    {
        string output = $"{Opcode}";
        if (Destination != null) output += $", {Destination.Value}";
        if (Source != null) output += $", {Source.Value}";
        if (Immediate != null) output += $", {Immediate.Value}";
        return output;
    }
}
