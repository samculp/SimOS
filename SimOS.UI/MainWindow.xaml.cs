using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SimOS;

namespace SimOS.UI;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly SimCpu _cpu;
    private readonly SimOperatingSystem _os;

    public MainWindow()
    {
        InitializeComponent();

        _cpu = new SimCpu();
        _os = _cpu.OS;

        _os.LoadPrograms();
        RefreshUI();
    }

    private void Step_Click(object sender, RoutedEventArgs e)
    {
        _os.Step();
        RefreshUI();
    }

    private void Run_Click(object sender, RoutedEventArgs e)
    {

    }

    private void Stop_Click(object sender, RoutedEventArgs e)
    {

    }

    private void RefreshUI()
    {
        CpuModeText.Text = _cpu.Mode.ToString();
        CurrentPidText.Text = _cpu.CurrentProcess?.PID.ToString() ?? "---";
        ProgramCounterText.Text = _cpu.ProgramCounter.ToString();

        RegisterR0Text.Text = _cpu.Registers[0].ToString();
        RegisterR1Text.Text = _cpu.Registers[1].ToString();
        RegisterR2Text.Text = _cpu.Registers[2].ToString();
        RegisterR3Text.Text = _cpu.Registers[3].ToString();
        RegisterR4Text.Text = _cpu.Registers[4].ToString();
        RegisterR5Text.Text = _cpu.Registers[5].ToString();
        RegisterR6Text.Text = _cpu.Registers[6].ToString();
        RegisterR7Text.Text = _cpu.Registers[7].ToString();


        ProcessGrid.ItemsSource = null;
        ProcessGrid.ItemsSource = _os.ProcessList.Processes;
    }
}