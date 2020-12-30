using System.Windows.Controls;

namespace AssignmentEvaluator.WPF.Views
{
    /// <summary>
    /// Interaction logic for EvaluationView.xaml
    /// </summary>
    public partial class EvaluationView : UserControl
    {
        public EvaluationView()
        {
            InitializeComponent();
        }

        private void OnKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                _searchButton.Command.Execute(null);
            }
        }
    }
}
