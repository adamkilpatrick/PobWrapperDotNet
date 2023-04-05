namespace PobWrapperDotNet

module PobWrapper =
    open NLua
    open System.IO
    open Microsoft.Extensions.Logging
    
    type PobContext = {Lua:Lua}

    let private luaTableToMap (luaTable:LuaTable) =
        luaTable
        |> (fun n -> (n.Keys|>Seq.cast<string>, n.Values |> Seq.cast<obj>))
        |> (fun (a,b) ->  Seq.zip a b )
        |> Map.ofSeq

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