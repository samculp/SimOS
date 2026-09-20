using SimOS;
using OperatingSystem = SimOS.OperatingSystem;

class Program
{
    static void Main(string[] args)
    {
        Cpu cpu = new();
        cpu.BootOS();
        cpu.OS.Run();
    }
}