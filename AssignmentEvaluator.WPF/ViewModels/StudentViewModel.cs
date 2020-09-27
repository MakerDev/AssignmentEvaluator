using AssignmentEvaluator.Models;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Text;

namespace AssignmentEvaluator.WPF.ViewModels
{
    public class StudentViewModel : BindableBase, INavigationAware
    {
        private Student _student = new Student();
        public Student Student
        {
            get { return _student; }
            set { SetProperty(ref _student, value); }
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            Student = navigationContext.Parameters["Student"] as Student;
        }
    }
}
