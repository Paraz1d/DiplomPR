using System.Windows.Controls;
using System.Windows.Input;
using AdminTaskSystem.ViewModels;

namespace AdminTaskSystem.Views;

public partial class TicketsView : UserControl
{
    public TicketsView() { InitializeComponent(); Loaded += (_, _) => { if (DataContext is TicketsViewModel vm && vm.LoadCommand.CanExecute(null)) vm.LoadCommand.Execute(null); }; }
    private void Grid_OnMouseDoubleClick(object sender, MouseButtonEventArgs e) { if (DataContext is TicketsViewModel vm && Grid.SelectedItem is not null && vm.OpenTicketCommand.CanExecute(Grid.SelectedItem)) vm.OpenTicketCommand.Execute(Grid.SelectedItem); }
}
