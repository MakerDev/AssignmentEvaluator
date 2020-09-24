using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AssignmentEvaluator.WPF.ViewModels
{
    public class ProblemDetailViewModel : BindableBase
    {
        private readonly IEventAggregator _eventAggregator;

        public ProblemDetailViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
        }
    }
}
