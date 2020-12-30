using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssignmentEvaluator.WPF.Core.Dialogs.ViewModels
{
    public class MessageDisplayDialogViewModel : BindableBase, IDialogAware
    {
        public string Title => "Message Dialog";

        public event Action<IDialogResult> RequestClose;

        private string _message = "";
        public string Message
        {
            get { return _message; }
            set { SetProperty(ref _message, value); }
        }

        public DelegateCommand CloseCommand { get; set; }

        public MessageDisplayDialogViewModel()
        {
            CloseCommand = new DelegateCommand(CloseDialog);
        }

        private void CloseDialog()
        {
            RequestClose?.Invoke(new DialogResult(ButtonResult.OK));
        }

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            Message = parameters.GetValue<string>("Message") ?? "";
        }
    }
}
