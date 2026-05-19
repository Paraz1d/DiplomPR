using System.Windows;
using AdminTaskSystem.Models;
using AdminTaskSystem.Services;
using AdminTaskSystem.Views;
using Microsoft.Extensions.Configuration;

namespace AdminTaskSystem;

public partial class App : Application
{
    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        try
        {
            await SupabaseService.InitializeAsync(config["Supabase:Url"]!, config["Supabase:Key"]!);
            try
            {
                await SupabaseService.Client.From<Category>().Limit(1).Get();
                CacheService.IsOnline = true;
            }
            catch
            {
                CacheService.IsOnline = false;
            }
        }
        catch
        {
            CacheService.IsOnline = false;
        }

        new LoginWindow().Show();
    }
}
