namespace AdminTaskSystem.Services;

public static class SupabaseService
{
    private static Supabase.Client? client;
    public static Supabase.Client Client => client ?? throw new InvalidOperationException("Supabase client is not initialized.");

    public static async Task InitializeAsync(string url, string key)
    {
        var options = new Supabase.SupabaseOptions { AutoConnectRealtime = false };
        client = new Supabase.Client(url, key, options);
        await client.InitializeAsync();
    }
}
