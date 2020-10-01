using AssignmentEvaluator.WPF.Core;
using Prism.Regions;
using System.Windows;

namespace AssignmentEvaluator.WPF.Views
{
    /// <summary>
    /// Interaction logic for ShellView.xaml
    /// </summary>
    public partial class ShellView : Window
    {
        public ShellView(IRegionManager regionManager)
        {
            InitializeComponent();

            regionManager.RegisterViewWithRegion(RegionNames.CONTENT_REGION, typeof(MainView));
        }
    }
}
