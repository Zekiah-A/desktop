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
    public ModLoader(GameData data)
    {
        gameData = data;
        glue = new ModGlue(gameData);
    }
    
    public async Task ExecutePack(string url)
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
        engine.DocumentSettings.AddSystemDocument("api", ModuleCategory.Standard, @"");
        engine.DocumentSettings.AddSystemDocument("world", ModuleCategory.Standard, @"");
        engine.DocumentSettings.AddSystemDocument("definitions", ModuleCategory.Standard, @"");

        engine.Execute(new DocumentInfo(uri) { Category = ModuleCategory.Standard }, initialScript);
    }
}