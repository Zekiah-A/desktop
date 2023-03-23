using ICSharpCode.SharpZipLib.Zip.Compression;
using Microsoft.ClearScript;
using Microsoft.ClearScript.JavaScript;
using Microsoft.ClearScript.V8;

namespace OpenMcDesktop.Mods;

public class ModLoader
{
    private const string DefaultUrl = "https://openmc.pages.dev";
    private GameData gameData;
    private ModGlue glue;
    private string apiScript;
    private string definitionsScript;
    private string worldScript;
    
    public ModLoader(GameData data)
    {
        gameData = data;
        glue = new ModGlue(gameData);
        apiScript = File.ReadAllText("Resources/ModBindings/api.js");
        definitionsScript = File.ReadAllText("Resources/ModBindings/definitions.js");
        worldScript = File.ReadAllText("Resources/ModBindings/world.js");
    }
    
    public async Task ExecutePack(string url)
    {
        try
        {
            var uri = new Uri(string.Concat(url.StartsWith("http") ? "" : DefaultUrl, url));
            var initialScript = await gameData.HttpClient.GetStringAsync(uri);

            using var engine = new V8ScriptEngine();
            engine.DocumentSettings = new DocumentSettings
            {
                SearchPath =  uri.ToString(),
                AccessFlags = DocumentAccessFlags.EnableAllLoading
            };
            
            engine.AddHostObject("glue", glue);
            engine.DocumentSettings.AddSystemDocument("api", ModuleCategory.Standard, apiScript);
            engine.DocumentSettings.AddSystemDocument("world", ModuleCategory.Standard, definitionsScript);
            engine.DocumentSettings.AddSystemDocument("definitions", ModuleCategory.Standard, worldScript);

            engine.Execute(new DocumentInfo(uri) { Category = ModuleCategory.Standard }, initialScript);
        }
        catch(Exception exception)
        {
            Console.WriteLine($"Failed to execute mod pack {url}, {exception}");
        }
    }
}