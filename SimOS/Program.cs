using SimOS;
using OperatingSystem = SimOS.SimOS;

class Program
{
    static void Main(string[] args)
    {
        SimCpu cpu = new();
        cpu.BootOS();
        cpu.OS.Run();
    }
}