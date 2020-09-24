using AssignmentEvaluator.WPF.Core;
using Prism.Ioc;
using Prism.Regions;
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
using System.Windows.Shapes;

namespace AssignmentEvaluator.WPF.Views
{
    /// <summary>
    /// Interaction logic for ShellView.xaml
    /// </summary>
    public partial class ShellView : Window
    {
        private readonly IRegionManager _regionManager;
        private readonly IContainerExtension _containerExtension;

        public ShellView(IRegionManager regionManager, IContainerExtension containerExtension)
        {
            InitializeComponent();
            _regionManager = regionManager;
            _containerExtension = containerExtension;

            
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);

            var dataLoadView = _containerExtension.Resolve<MainView>();
            _regionManager.AddToRegion(RegionNames.CONTENT_RIGION, dataLoadView);
        }
    }
}
