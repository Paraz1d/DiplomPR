using System.Windows;
using System.Windows.Controls;
using AdminTaskSystem.ViewModels;

namespace AdminTaskSystem.Views;

public partial class EmployeeEditDialog : UserControl
{
    public EmployeeEditDialog() { InitializeComponent(); Loaded += (_, _) => { if (DataContext is EmployeeEditViewModel vm && vm.LoadCommand.CanExecute(null)) vm.LoadCommand.Execute(null); }; }
    private void PasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e) { if (DataContext is EmployeeEditViewModel vm) vm.Password = PasswordBox.Password; }
}
