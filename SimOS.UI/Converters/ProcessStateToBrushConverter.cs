using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using SimOS;

namespace SimOS.UI.Converters
{
    public class ProcessStateToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not ProcessState state)
                return Brushes.Transparent;

            return state switch
            {
                ProcessState.Initial => new SolidColorBrush(Color.FromRgb(0xA0, 0xA0, 0xA0)), // gray
                ProcessState.Ready => new SolidColorBrush(Color.FromRgb(0xF9, 0xFC, 0x7C)), // yellow
                ProcessState.Running => new SolidColorBrush(Color.FromRgb(0x31, 0xF5, 0x27)), // green
                ProcessState.Blocked => new SolidColorBrush(Color.FromRgb(0xF8, 0xD7, 0xA3)), // orange
                ProcessState.Final => new SolidColorBrush(Color.FromRgb(0x7C, 0xB6, 0xFC)), // blue
                _ => Brushes.Transparent,
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
