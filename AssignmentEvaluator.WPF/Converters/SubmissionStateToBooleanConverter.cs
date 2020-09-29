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
                return SubmissionState.Late;
            }

            //TODO : possible bug - 미제출 학생에서 클릭두번 하면 OnDate로 바뀜
            return SubmissionState.OnDate;
        }
    }
}
