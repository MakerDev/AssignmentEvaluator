using AssignmentEvaluator.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace AssignmentEvaluator.WPF.Converters
{
    public class SubmissionStateToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SubmissionState state = (SubmissionState)value;

            if (state == SubmissionState.Late)
            {
                return true;
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool valueToConvert = (bool)value;

            if (valueToConvert)
            {
                return SubmissionState.OnDate;
            }

            return SubmissionState.Late;
        }
    }
}
