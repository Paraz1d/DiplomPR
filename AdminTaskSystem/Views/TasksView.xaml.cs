using System.Windows.Controls;
using System.Windows.Input;
using AdminTaskSystem.ViewModels;

namespace AdminTaskSystem.Views;

public partial class TasksView : UserControl
{
    public TasksView() { InitializeComponent(); Loaded += (_, _) => { if (DataContext is TasksViewModel vm && vm.LoadCommand.CanExecute(null)) vm.LoadCommand.Execute(null); }; }
    private void Grid_OnMouseDoubleClick(object sender, MouseButtonEventArgs e) { if (DataContext is TasksViewModel vm && Grid.SelectedItem is not null && vm.OpenTaskCommand.CanExecute(Grid.SelectedItem)) vm.OpenTaskCommand.Execute(Grid.SelectedItem); }
}
