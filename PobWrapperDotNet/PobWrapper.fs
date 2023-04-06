namespace PobWrapperDotNet

module PobWrapper =
    open NLua
    open System.IO
    open Microsoft.Extensions.Logging
    
    type PobContext = {Lua:Lua}
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

    let private luaTableToMap (luaTable:LuaTable) =
        luaTable
        |> (fun n -> (n.Keys|>Seq.cast<string>, n.Values |> Seq.cast<obj>))
        |> (fun (a,b) ->  Seq.zip a b )
        |> Map.ofSeq

    /// <summary>Converts an itemslot to a string</summary>
    /// <param name="itemSlot">Itemslot enum</param>
    /// <returns>String representation of the itemslot</returns>
    let itemSlotToString (itemSlot:ItemSlot) =
        match itemSlot with
            | Weapon1 -> "Weapon 1"
            | Weapon2 -> "Weapon 2"
            | Helmet -> "Helmet"
            | BodyArmour -> "Body Armour"
            | Gloves -> "Gloves"
            | Boots -> "Boots"
            | Amulet -> "Amulet"
            | Ring1 -> "Ring 1"
            | Ring2 -> "Ring 2"
            | Belt -> "Belt"

    /// <summary>Converts a string to an itemslot. Throws if no item slot can be parsed.</summary>
    /// <param name="itemSlotString">String representation of the itemslot.</param>
    /// <returns>The ItemSolt enum value</returns>
    let stringToItemSlot (itemSlotString:string) =
        match (itemSlotString.ToLower().Replace(" ","")) with
        | "weapon1" -> Weapon1
        | "weapon2" -> Weapon2
        | "helmet" -> Helmet
        | "bodyarmour" -> BodyArmour
        | "gloves" -> Gloves
        | "boots" -> Boots
        | "amulet" -> Amulet
        | "ring1" -> Ring1
        | "ring2" -> Ring2
        | "belt" -> Belt
        | _ -> failwithf "Cannot map %s to an itemslot" itemSlotString

    /// <summary>Creates the initial lua context that will be used for later calls.</summary>
    /// <param name="logger">logger</param>
    /// <param name="pobPath">Path to the src dir of the PoB repo</param>
    /// <returns>Context object containing the reference to the lua object</returns>
    let createPobContext (logger:ILogger) (pobPath:string) =
        let validate (path:string) =
            match Path.Exists pobPath with
            | true -> path
            | false -> raise (DirectoryNotFoundException(sprintf "%s does not exist" pobPath))
            
        let createContext path =         
            let currentDir = Directory.GetCurrentDirectory()
            // Ideally we would set the working directory back but after testing that appears to break things
            Directory.SetCurrentDirectory(path)
            let state = new Lua() // fsharplint:disable-line
            state.DoFile(Path.Combine(currentDir,"shim.lua")) |> ignore
            state.DoFile(Path.Combine(currentDir,"hooks.lua")) |> ignore
            {Lua=state}

        pobPath
        |> validate
        |> (fun path -> logger.LogDebug("Initializing PobContext at path {path}", path); path)
        |> createContext
        |> (fun context -> logger.LogDebug("Created PobContext"); context)

    /// <summary>Sets custom mods for the loaded build</summary>
    /// <param name="logger">Logger</param>
    /// <param name="context">Initialized context</param>
    /// <param name="mods">The list of mods to set</param>
    /// <returns>The context with updated mods</returns>
    let setCustomModsFromStrings (logger:ILogger) (context:PobContext) (mods:string[]) =
        let modString = mods
                                |> Array.fold (fun state cur -> state + cur + "\n" ) "" 
        modString
        |> (fun modString -> logger.LogDebug("Setting the build custom mods to {modString}", modString); modString)
        |> (fun modString -> context.Lua.GetFunction("setCustomMods").Call(modString))
        |> ignore
        context

    /// <summary>
    /// Gets a Map<string,obj> representing the calculations for the currently loaded build.
    /// Note that the values returned in this table can be of various types (float, int, bool).
    /// </summary>
    /// <param name="logger">Logger</param>
    /// <param name="context">Initialized context</param>
    /// <returns>String -> object mapping of all the calculations for the current build.</returns>
    let getCalcsForContext (logger:ILogger) (context:PobContext) =
        logger.LogDebug("Getting calcs for the loaded build")

        let lua = context.Lua
        lua.GetFunction("getCalcs").Call()
        |> Seq.cast<LuaTable>
        |> Seq.head
        |> luaTableToMap

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
    let getCalcsForContextWithItemOverride (logger:ILogger) (context:PobContext) (itemSlot:ItemSlot) (itemString:string) =
        logger.LogDebug("Getting calcs for the loaded build with overrideSlot: {itemSlot} and override: {itemString}", itemSlot, itemString)

        let lua = context.Lua
        let itemSlotString = itemSlotToString itemSlot
        lua.GetFunction("calculateWithItem").Call(itemSlotString, itemString)
        |> Seq.cast<LuaTable>
        |> Seq.head
        |> luaTableToMap

    /// <summary>Returns the set of possible slots that an item could be placed into for the current build.</summary>
    /// <param name="logger">Logger</param>
    /// <param name="context">The context of the current built</param>
    /// <param name="itemString">The string of the item to substitute with (e.g. copied from the game or trade)</param>
    /// <returns>Array containing all of the possible slots.</returns>
    let getValidSlotsForItem (logger:ILogger) (context:PobContext) (itemString:string) =
        let lua = context.Lua
        let validItemSlots = itemString
                                    |> (fun item -> lua.GetFunction("getValidSlotsForItem").Call(item))
                                    |> Seq.cast<LuaTable>
                                    |> Seq.head
                                    |> (fun n -> n.Values)
                                    |> Seq.cast<string>
                                    |> Seq.map stringToItemSlot
                                    |> Seq.toArray
        logger.LogDebug("The item {itemString} can be placed in the following slots {slots}", itemString, validItemSlots)
        validItemSlots

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
    let setBuildState (logger:ILogger) (context:PobContext) (itemJson:string) (skillJson:string) =
        let lua = context.Lua
        lua.GetFunction("newBuild").Call() |> ignore
        (itemJson, skillJson)
        |> (fun jsonPair -> logger.LogDebug("Loading the build from the following item,skill json pair: {jsonPair}", jsonPair); jsonPair)
        |> (fun (itemJson, skillJson) -> lua.GetFunction("loadBuildFromJSON").Call(itemJson,skillJson))
        |> ignore
        logger.LogDebug("Loaded the build from json pair")
        context