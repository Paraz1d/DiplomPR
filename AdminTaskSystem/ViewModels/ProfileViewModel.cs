using AdminTaskSystem.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AdminTaskSystem.ViewModels;

public partial class ProfileViewModel : BaseViewModel
{
    private readonly EmployeeService employeeService = new();
    private readonly TicketService ticketService = new();
    private readonly TaskService taskService = new();

    public string FullName => AppSession.CurrentUser?.FullName ?? string.Empty;
    public string RoleName => AppSession.RoleName;
    public string Email => AppSession.CurrentUser?.Email ?? string.Empty;
    public string Phone => AppSession.CurrentUser?.Phone ?? string.Empty;
    public string Initials => AppSession.Initials;

    [ObservableProperty] private string departmentName = string.Empty;
    [ObservableProperty] private int myTicketsTotal;
    [ObservableProperty] private int myTicketsDone;
    [ObservableProperty] private int myTicketsOverdue;
    [ObservableProperty] private int myTasksTotal;
    [ObservableProperty] private int myTasksDone;
    [ObservableProperty] private string cacheStatus = string.Empty;
    [ObservableProperty] private string currentPassword = string.Empty;
    [ObservableProperty] private string newPassword = string.Empty;
    [ObservableProperty] private string confirmPassword = string.Empty;

    public ProfileViewModel()
    {
        Title = "Профиль";
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        IsBusy = true;
        try
        {
            var departments = await employeeService.GetDepartmentsAsync();
            DepartmentName = departments.FirstOrDefault(x => x.Id == AppSession.DepartmentId)?.Name ?? "Отдел не указан";
            var tickets = await ticketService.GetTicketsAsync();
            var tasks = await taskService.GetTasksAsync();
            var userId = AppSession.CurrentUser?.Id;
            var myTickets = tickets.Where(x => x.AssignedEmployeeId == userId || x.CreatedById == userId).ToList();
            MyTicketsTotal = myTickets.Count;
            MyTicketsDone = myTickets.Count(x => x.Status == "done");
            MyTicketsOverdue = myTickets.Count(x => x.Status == "overdue");
            var myTasks = tasks.Where(x => x.AssignedEmployeeId == userId).ToList();
            MyTasksTotal = myTasks.Count;
            MyTasksDone = myTasks.Count(x => x.Status == "done");
            CacheStatus = CacheService.CacheInfo;
        }
        catch (InvalidOperationException ex) { NotifyError(ex.Message); }
        catch (Exception ex) { NotifyError("Ошибка: " + ex.Message); }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task SavePasswordAsync()
    {
        try
        {
            if (AppSession.CurrentUser is null) return;
            if (CurrentPassword != AppSession.CurrentUser.Password) { NotifyError("Текущий пароль указан неверно"); return; }
            if (NewPassword != ConfirmPassword) { NotifyError("Новый пароль и подтверждение не совпадают"); return; }
            if (NewPassword.Length < 6) { NotifyError("Новый пароль должен содержать не менее 6 символов"); return; }
            var oldPassword = AppSession.CurrentUser.Password;
            AppSession.CurrentUser.Password = NewPassword;
            try
            {
                await employeeService.UpdateAsync(AppSession.CurrentUser);
            }
            catch
            {
                AppSession.CurrentUser.Password = oldPassword;
                throw;
            }
            CurrentPassword = NewPassword = ConfirmPassword = string.Empty;
            Notify("Пароль успешно изменён");
        }
        catch (InvalidOperationException ex) { NotifyError(ex.Message); }
        catch (Exception ex) { NotifyError("Ошибка: " + ex.Message); }
    }
}
