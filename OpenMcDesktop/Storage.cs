using System.Text.Json;

namespace OpenMcDesktop;

public static class Storage
{
    private static readonly string DataPath;

    static Storage()
    {
        DataPath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "OpenMcDesktop");
        
        if (!Directory.Exists(DataPath))
        {
            Directory.CreateDirectory(DataPath);
        }
    }
    
    public static T? Get<T>(string name)
    {
        var path = Path.Join(DataPath, name);
        return File.Exists(path) ? JsonSerializer.Deserialize<T>(File.ReadAllText(path)) : (T?) (object?) null;
    }

    public static void Save<T>(string name, T value)
    {
        var serialised = JsonSerializer.Serialize(value);
        File.WriteAllText(Path.Join(DataPath, name), serialised);
    }
}