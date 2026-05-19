using System.Windows.Controls;
using AdminTaskSystem.ViewModels;

namespace AdminTaskSystem.Views;

public partial class EmployeesView : UserControl
{
    public EmployeesView() { InitializeComponent(); Loaded += (_, _) => { if (DataContext is EmployeesViewModel vm && vm.LoadCommand.CanExecute(null)) vm.LoadCommand.Execute(null); }; }
}
