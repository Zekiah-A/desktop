using System.Text.Json;

namespace OpenMcDesktop;

public class Storage
{
    public string DataPath;

    public Storage(string dataPath)
    {
        DataPath = dataPath;

        if (!Directory.Exists(DataPath))
        {
            Directory.CreateDirectory(DataPath);
        }
    }

    public T? Get<T>(string name)
    {
        var path = Path.Join(DataPath, name);
        return File.Exists(path) ? JsonSerializer.Deserialize<T>(File.ReadAllText(path)) : (T?) (object?) null;
    }

    public void Save<T>(string name, T value)
    {
        var serialised = JsonSerializer.Serialize(value);
        File.WriteAllText(Path.Join(DataPath, name), serialised);
    }
}