using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Supabase.Postgrest.Models;

namespace AdminTaskSystem.Services;

public static class CacheService
{
    public static bool IsOnline { get; set; } = true;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver
        {
            Modifiers =
            {
                static typeInfo =>
                {
                    if (!typeof(BaseModel).IsAssignableFrom(typeInfo.Type))
                    {
                        return;
                    }

                    for (var i = typeInfo.Properties.Count - 1; i >= 0; i--)
                    {
                        if (typeInfo.Properties[i].AttributeProvider is PropertyInfo propertyInfo &&
                            propertyInfo.DeclaringType == typeof(BaseModel))
                        {
                            typeInfo.Properties.RemoveAt(i);
                        }
                    }
                }
            }
        }
    };

    private static readonly string CacheDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "AdminTaskSystem");

    static CacheService()
    {
        Directory.CreateDirectory(CacheDir);
    }

    public static async Task SaveAsync<T>(string fileName, List<T> data)
    {
        var path = Path.Combine(CacheDir, fileName);
        var json = JsonSerializer.Serialize(data, JsonOptions);
        await File.WriteAllTextAsync(path, json);
    }

    public static async Task<List<T>> LoadAsync<T>(string fileName)
    {
        var path = Path.Combine(CacheDir, fileName);
        if (!File.Exists(path))
        {
            return new List<T>();
        }

        try
        {
            var json = await File.ReadAllTextAsync(path);
            return JsonSerializer.Deserialize<List<T>>(json, JsonOptions) ?? new List<T>();
        }
        catch (JsonException)
        {
            return new List<T>();
        }
    }

    public static string CacheInfo => IsOnline ? "Онлайн" : "Офлайн - данные из кэша";
}
