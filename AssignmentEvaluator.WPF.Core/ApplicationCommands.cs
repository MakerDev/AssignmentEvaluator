using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace AssignmentEvaluator.WPF.Core
{
    public class ApplicationCommands
    {
        public static CompositeCommand CloseAllExpanders { get; set; } = new CompositeCommand();
    }
}
