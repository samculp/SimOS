using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Media;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace SimOS.UI.Converters
{
    internal class EventTypeToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Brush result = value switch
            {
                SimEventType.CPU => Brushes.LightBlue,
                SimEventType.TRAP => Brushes.LightGreen,
                SimEventType.OS => Brushes.OrangeRed,
                SimEventType.SCHED => Brushes.MediumPurple,
                _ => Brushes.White
            };

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
