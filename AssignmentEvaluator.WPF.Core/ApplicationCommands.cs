using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace AssignmentEvaluator.WPF.Core
{
    public class ApplicationCommands
    {
        public static CompositeCommand CloseAllExpanders { get; set; } = new CompositeCommand();
        public static Action<int> Scroll { get; set; } = null;
        public static Action<int, int> ResizeWindow { get; set; } = null;
    }
}
