using System.Windows;
using System.Windows.Controls;
using AdminTaskSystem.ViewModels;

namespace AdminTaskSystem.Views;

public partial class ProfileView : UserControl
{
    public ProfileView() { InitializeComponent(); Loaded += (_, _) => ExecuteLoad(); }
    private void ExecuteLoad() { if (DataContext is ProfileViewModel vm && vm.LoadCommand.CanExecute(null)) vm.LoadCommand.Execute(null); }
    private void CurrentPasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e) { if (DataContext is ProfileViewModel vm) vm.CurrentPassword = CurrentPasswordBox.Password; }
    private void NewPasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e) { if (DataContext is ProfileViewModel vm) vm.NewPassword = NewPasswordBox.Password; }
    private void ConfirmPasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e) { if (DataContext is ProfileViewModel vm) vm.ConfirmPassword = ConfirmPasswordBox.Password; }
}
