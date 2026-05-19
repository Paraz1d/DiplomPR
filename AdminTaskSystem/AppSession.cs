using AdminTaskSystem.Models;

namespace AdminTaskSystem;

public static class AppSession
{
    public static Employee? CurrentUser { get; set; }
    public static bool IsAdmin => CurrentUser?.Role == "admin";
    public static bool IsManager => CurrentUser?.Role == "manager";
    public static bool IsEmployee => CurrentUser?.Role == "employee";
    public static bool CanManage => IsAdmin || IsManager;
    public static int? DepartmentId => CurrentUser?.DepartmentId;

    public static string Initials => CurrentUser == null ? "?" :
        string.Concat(CurrentUser.FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(2).Select(w => char.ToUpperInvariant(w[0])));

    public static string RoleName => CurrentUser?.Role switch
    {
        "admin" => "Администратор",
        "manager" => "Руководитель",
        "employee" => "Сотрудник",
        _ => string.Empty
    };

    public static void Clear() => CurrentUser = null;
}
