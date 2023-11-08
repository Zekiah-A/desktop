using ICSharpCode.SharpZipLib.Zip.Compression;
using Microsoft.ClearScript;
using Microsoft.ClearScript.JavaScript;
using Microsoft.ClearScript.V8;
using Microsoft.Extensions.Logging;

namespace OpenMcDesktop.Mods;

public class ModLoader
{
    private const string DefaultUrl = "https://openmc.pages.dev";
    private ModGlue glue;
    private string apiScript;
    private string definitionsScript;
    private string worldScript;
    
    public ModLoader()
    {
        glue = new ModGlue(StaticData.GameData);
        apiScript = File.ReadAllText("Resources/ModBindings/api.js");
        definitionsScript = File.ReadAllText("Resources/ModBindings/definitions.js");
        worldScript = File.ReadAllText("Resources/ModBindings/world.js");
    }
    
    public async Task ExecutePack(string url)
    {
        try
        {
            var uri = new Uri(string.Concat(url.StartsWith("http") ? "" : DefaultUrl, url));
            var initialScript = await StaticData.GameData.HttpClient.GetStringAsync(uri);

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
            StaticData.GameData.Logger.LogError("Failed to execute mod pack {url}, {exception}", url, exception);
        }
    }
}