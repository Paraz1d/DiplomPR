using Microsoft.Extensions.Configuration;
using Supabase;

namespace AdminTaskSystem.Services;

public sealed class SupabaseService
{
    public Client Client { get; private set; } = null!;

    public async System.Threading.Tasks.Task InitializeAsync()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var url = configuration["Supabase:Url"] ?? string.Empty;
        var key = configuration["Supabase:Key"] ?? string.Empty;

        var options = new SupabaseOptions
        {
            AutoConnectRealtime = false
        };

        Client = new Client(url, key, options);
        await Client.InitializeAsync();
    }

    public async System.Threading.Tasks.Task MarkOverdueAsync()
    {
        if (Client is null)
        {
            return;
        }

        await Client.Rpc("fn_mark_overdue", new Dictionary<string, object>());
    }
}
