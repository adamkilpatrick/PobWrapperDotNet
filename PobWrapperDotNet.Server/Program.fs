open System
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Hosting
open PobWrapperDotNet.PobWrapper
open Microsoft.Extensions.Logging
open System.Net.Http
open System.Threading.Tasks
open Serilog

let getSkillsJson (httpClient:HttpClient) (charName:string) (acctName:string) =
    let url = sprintf "https://www.pathofexile.com/character-window/get-passive-skills?accountName=%s&character=%s&realm=pc" acctName charName
    httpClient.GetStringAsync(url)
    |> Async.AwaitTask

let getItemJson (httpClient:HttpClient) (charName:string) (acctName:string) =
    let url = sprintf "https://www.pathofexile.com/character-window/get-items?accountName=%s&character=%s&realm=pc" acctName charName
    httpClient.GetStringAsync(url)
    |> Async.AwaitTask

let getCalcsForBuild (httpClient:HttpClient)
                        (startingContext:PobContext)
                        (setBuild:(PobContext->string->string->PobContext)) 
                        (getCalcs:(PobContext->Map<string,obj>)) 
                        (acctName:string) 
                        (charName:string) =
    async {
        let! skillJson = getSkillsJson httpClient charName acctName
        let! itemJson =  getItemJson httpClient charName acctName
        
        return setBuild startingContext itemJson skillJson
        |> getCalcs
        // Json serializer will fail if we get a double/int NaN so for simplicity we are just putting everything as string
        |> Map.map (fun key v -> string(v))
    }
    |> Async.StartAsTask
    

[<EntryPoint>]
let main args =
    let builder = WebApplication.CreateBuilder(args)
    builder.Logging.ClearProviders() |> ignore
    builder.Host.UseSerilog(fun context services config ->
                                config.ReadFrom.Configuration(context.Configuration).Enrich.FromLogContext()
                                |> ignore
    ) |> ignore
    let app = builder.Build()
    app.UseSerilogRequestLogging() |> ignore

    // Setup any config resources we will need
    let pobPath = builder.Configuration.Item("POB_SRC_PATH")
    let logger = app.Logger
    
    // Create long lived objects (iirc we should be using an HttpClientFactory but we will keep this example short)
    let httpClient = new HttpClient() // fsharplint:disable-line
    httpClient.Timeout <- TimeSpan.FromSeconds(30)
    let context = createPobContext logger pobPath

    // Wire up the calc handler
    let getCalcs = getCalcsForContext logger
    let setBuild = setBuildState logger
    let getCalcsHandler = getCalcsForBuild httpClient context setBuild getCalcs
    
    // Bind to the routing
    app.MapGet("/getCalcs", 
        Func<string,string,Task<Map<string,string>>>(fun charName accountName -> getCalcsHandler accountName charName)) 
    |> ignore

    app.Run()

    0 // Exit code

