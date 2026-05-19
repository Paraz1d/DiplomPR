using System.Windows.Controls;
using AdminTaskSystem.ViewModels;

namespace AdminTaskSystem.Views;

public partial class ReportsView : UserControl
{
    public ReportsView() { InitializeComponent(); Loaded += (_, _) => { if (DataContext is ReportsViewModel vm && vm.LoadCommand.CanExecute(null)) vm.LoadCommand.Execute(null); }; }
}
