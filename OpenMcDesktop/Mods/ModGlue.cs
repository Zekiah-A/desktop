using OpenMcDesktop.Game.Definitions;

namespace OpenMcDesktop.Mods;

public class ModGlue
{
    private GameData gameData;
    
    public ModGlue(GameData data)
    {
        gameData = data;
    }
    
    Block[] GetBlockDefinitions()
    {
        return gameData.Blocks;
    }

    Item[] GetItemDefinitions()
    {
        return gameData.Items;
    }

    void DefineBlock()
    {
        
    }

    void DefineItem()
    {
        
    }

    void DefineEntity()
    {
        
    }
}