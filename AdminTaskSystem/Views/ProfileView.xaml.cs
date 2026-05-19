using System.Windows;
using System.Windows.Controls;
using AdminTaskSystem.ViewModels;

namespace AdminTaskSystem.Views;

public partial class ProfileView : UserControl
{
    public ProfileView() { InitializeComponent(); Loaded += (_, _) => ExecuteLoad(); }
    private void ExecuteLoad() { if (DataContext is ProfileViewModel vm && vm.LoadCommand.CanExecute(null)) vm.LoadCommand.Execute(null); }
   
}
