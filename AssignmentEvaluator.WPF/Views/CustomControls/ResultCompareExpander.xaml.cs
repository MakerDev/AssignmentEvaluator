using AssignmentEvaluator.WPF.Core;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace AssignmentEvaluator.WPF.Views.CustomControls
{
    public partial class ResultCompareExpander : UserControl
    {
        public bool IsExpaned
        {
            get { return (bool)GetValue(IsExpanedProperty); }
            set { SetValue(IsExpanedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsExpaned.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsExpanedProperty =
            DependencyProperty.Register("IsExpaned", typeof(bool), typeof(ResultCompareExpander), new PropertyMetadata(false));

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

            _diffViewer.PreviewMouseWheel += OnPreviewMouseWheel;
        }

        private void OnPreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            ApplicationCommands.Scroll(e.Delta);
        }
    }
}
