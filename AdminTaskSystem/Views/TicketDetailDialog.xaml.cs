using System.Windows.Controls;
using AdminTaskSystem.ViewModels;

namespace AdminTaskSystem.Views;

public partial class TicketDetailDialog : UserControl
{
    public TicketDetailDialog() { InitializeComponent(); Loaded += (_, _) => { if (DataContext is TicketDetailViewModel vm && vm.LoadCommand.CanExecute(null)) vm.LoadCommand.Execute(null); }; }
}
