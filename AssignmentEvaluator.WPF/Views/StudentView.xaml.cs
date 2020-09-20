using AssignmentEvaluator.WPF.Core;
using Prism.Regions;
using System.Windows.Controls;

namespace AssignmentEvaluator.WPF.Views
{
    /// <summary>
    /// Interaction logic for StudentView
    /// </summary>
    public partial class StudentView : UserControl
    {
        public StudentView(IRegionManager regionManager)
        {
            InitializeComponent();

            regionManager.RegisterViewWithRegion(RegionNames.PROBLEM_LIST_REGION, typeof(ProblemList));
        }
    }
}
