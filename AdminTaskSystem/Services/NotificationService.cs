using AdminTaskSystem.Models;

namespace AdminTaskSystem.Services;

public sealed class NotificationService
{
    public async Task<List<Notification>> GetUnreadAsync()
    {
        var userId = AppSession.CurrentUser?.Id ?? 0;
        List<Notification> notifications;
        if (CacheService.IsOnline)
        {
            var response = await SupabaseService.Client.From<Notification>().Get();
            notifications = response.Models.ToList();
            await CacheService.SaveAsync("notifications.json", notifications);
        }
        else
        {
            notifications = await CacheService.LoadAsync<Notification>("notifications.json");
        }

        return notifications.Where(x => x.EmployeeId == userId && !x.IsRead)
            .OrderByDescending(x => x.CreatedAt)
            .ToList();
    }

    public async Task MarkReadAsync(int id)
    {
        EnsureOnline();
        var all = (await SupabaseService.Client.From<Notification>().Get()).Models.ToList();
        var notification = all.FirstOrDefault(x => x.Id == id);
        if (notification is not null)
        {
            notification.IsRead = true;
            await SupabaseService.Client.From<Notification>().Update(notification);
        }
    }

    public async Task MarkAllReadAsync()
    {
        EnsureOnline();
        var userId = AppSession.CurrentUser?.Id ?? 0;
        var all = (await SupabaseService.Client.From<Notification>().Get()).Models.ToList();
        foreach (var notification in all.Where(x => x.EmployeeId == userId && !x.IsRead))
        {
            notification.IsRead = true;
            await SupabaseService.Client.From<Notification>().Update(notification);
        }
        await CacheService.SaveAsync("notifications.json", all);
    }

    private static void EnsureOnline()
    {
        if (!CacheService.IsOnline)
        {
            throw new InvalidOperationException("Нет подключения. Изменения недоступны в офлайн-режиме.");
        }
    }
}
