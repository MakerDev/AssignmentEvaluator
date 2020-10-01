using System;
using System.Collections.Generic;
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

namespace AssignmentEvaluator.WPF.Views.CustomControls
{
    public partial class ResultCompareExpander : UserControl
    {
        public string Header
        {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof(string), typeof(ResultCompareExpander), new PropertyMetadata(""));


        public string Answer
        {
            get { return (string)GetValue(AnswerProperty); }
            set { SetValue(AnswerProperty, value); }
        }

        public static readonly DependencyProperty AnswerProperty =
            DependencyProperty.Register("Answer", typeof(string), typeof(ResultCompareExpander), new PropertyMetadata(""));


        public string Result
        {
            get { return (string)GetValue(ResultProperty); }
            set { SetValue(ResultProperty, value); }
        }

        public static readonly DependencyProperty ResultProperty =
            DependencyProperty.Register("Result", typeof(string), typeof(ResultCompareExpander), new PropertyMetadata(""));


        public ResultCompareExpander()
        {            
            InitializeComponent();
        }
    }
}
