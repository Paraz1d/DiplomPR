using System.Windows.Controls;
using AdminTaskSystem.ViewModels;

namespace AdminTaskSystem.Views;

public partial class TaskDetailDialog : UserControl
{
    public TaskDetailDialog() { InitializeComponent(); Loaded += (_, _) => { if (DataContext is TaskDetailViewModel vm && vm.LoadCommand.CanExecute(null)) vm.LoadCommand.Execute(null); }; }
}
