

namespace FSharp

namespace PobWrapperDotNet
    
    module PobWrapper =
        
        type PobContext =
            { Lua: NLua.Lua }
        
        type ItemSlot =
            | Weapon1
            | Weapon2
            | Helmet
            | BodyArmour
            | Gloves
            | Boots
            | Amulet
            | Ring1
            | Ring2
            | Belt
        
        val private luaTableToMap: luaTable: NLua.LuaTable -> Map<string,obj>
        
        /// <summary>Converts an itemslot to a string</summary>
        /// <param name="itemSlot">Itemslot enum</param>
        /// <returns>String representation of the itemslot</returns>
        val itemSlotToString: itemSlot: ItemSlot -> string
        
        /// <summary>Converts a string to an itemslot. Throws if no item slot can be parsed.</summary>
        /// <param name="itemSlotString">String representation of the itemslot.</param>
        /// <returns>The ItemSolt enum value</returns>
        val stringToItemSlot: itemSlotString: string -> ItemSlot
        
        /// <summary>Creates the initial lua context that will be used for later calls.</summary>
        /// <param name="logger">logger</param>
        /// <param name="pobPath">Path to the src dir of the PoB repo</param>
        /// <returns>Context object containing the reference to the lua object</returns>
        val createPobContext:
          logger: Microsoft.Extensions.Logging.ILogger ->
            pobPath: string -> PobContext
        
        /// <summary>Sets custom mods for the loaded build</summary>
        /// <param name="logger">Logger</param>
        /// <param name="context">Initialized context</param>
        /// <param name="mods">The list of mods to set</param>
        /// <returns>The context with updated mods</returns>
        val setCustomModsFromStrings:
          logger: Microsoft.Extensions.Logging.ILogger ->
            context: PobContext -> mods: string array -> PobContext
        
        /// <summary>
        /// Gets a Map<string,obj> representing the calculations for the currently loaded build.
        /// Note that the values returned in this table can be of various types (float, int, bool).
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="context">Initialized context</param>
        /// <returns>String -> object mapping of all the calculations for the current build.</returns>
        val getCalcsForContext:
          logger: Microsoft.Extensions.Logging.ILogger ->
            context: PobContext -> Map<string,obj>
        
        /// <summary>
        /// Gets a Map<string,obj> representing the calculations for the currently loaded build, 
        /// substituting the item at the provided itemslot with the item represented by the itemString param.
        /// Note that the values returned in this table can be of various types (float, int, bool).
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="context">Initialized context</param>
        /// <param name="itemSlot">The item slot to substitute with</param>
        /// <param name="itemString">The string of the item to substitute with (e.g. copied from the game or trade)</param>
        /// <returns>String -> object mapping of all the calculations for the current build with the item override.</returns>
        val getCalcsForContextWithItemOverride:
          logger: Microsoft.Extensions.Logging.ILogger ->
            context: PobContext ->
            itemSlot: ItemSlot -> itemString: string -> Map<string,obj>
        
        /// <summary>Returns the set of possible slots that an item could be placed into for the current build.</summary>
        /// <param name="logger">Logger</param>
        /// <param name="context">The context of the current built</param>
        /// <param name="itemString">The string of the item to substitute with (e.g. copied from the game or trade)</param>
        /// <returns>Array containing all of the possible slots.</returns>
        val getValidSlotsForItem:
          logger: Microsoft.Extensions.Logging.ILogger ->
            context: PobContext -> itemString: string -> ItemSlot array
        
        /// <summary>
        /// Loads build info into the provided context based on item and skill json strings pulled from the PoE api.
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="context">Initialized context</param>
        /// <param name="itemJson">
        /// Json string from https://www.pathofexile.com/character-window/get-items?accountName={acctName}&character={charName}&realm={realm}
        /// </param>
        /// <param name="skillJson">
        /// Json string from https://www.pathofexile.com/character-window/get-passive-skills?accountName={acctName}&character={charName}&realm={realm}
        /// </param>
        /// <returns>Context with the updated build state.</returns>
        val setBuildState:
          logger: Microsoft.Extensions.Logging.ILogger ->
            context: PobContext ->
            itemJson: string -> skillJson: string -> PobContext

