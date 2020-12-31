using AssignmentEvaluator.WPF.Core;
using System.Windows.Controls;

namespace AssignmentEvaluator.WPF.Views
{
    public partial class StudentView : UserControl
    {
        //TODO : Resize scrollviewer as the window resizes.
        public StudentView()
        {
            InitializeComponent();

            ApplicationCommands.Scroll = (delta) =>
            {
                if (delta > 0)
                {
                    _scrollViewer.LineUp();
                    _scrollViewer.LineUp();
                }
                else if (delta < 0)
                {
                    _scrollViewer.LineDown();
                    _scrollViewer.LineDown();
                }
            };
        }
    }
}
