using AssignmentEvaluator.Models;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace AssignmentEvaluator.WPF.Events
{
    public class StudentChangeEvent : PubSubEvent<Student>
    {

    }
}
