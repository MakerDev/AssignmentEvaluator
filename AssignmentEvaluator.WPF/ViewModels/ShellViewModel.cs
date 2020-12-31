using AssignmentEvaluator.WPF.Core;
using Prism.Mvvm;
using System;

namespace AssignmentEvaluator.WPF.ViewModels
{
    public class ShellViewModel : BindableBase
    {
        private int _windowWidth = 1150;
        public int WindowWidth
        {
            get { return _windowWidth; }
            set { SetProperty(ref _windowWidth, value); }
        }

        private int _windowHeight = 550;
        public int WindowHeight
        {
            get { return _windowHeight; }
            set { SetProperty(ref _windowHeight, value); }
        }

        public ShellViewModel()
        {
            ApplicationCommands.ResizeWindow = (width, height) =>
            {
                WindowWidth = width;
                WindowHeight = height;
            };
        }
    }
}
