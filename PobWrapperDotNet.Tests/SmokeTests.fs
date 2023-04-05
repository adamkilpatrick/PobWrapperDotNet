module PobWrapperDotNet.Tests

open NUnit.Framework
open PobWrapperDotNet.PobWrapper
open Microsoft.Extensions.Logging.Abstractions


[<SetUp>]
let Setup () =
    ()

[<Test>]
let WhenContextCreatedFromPathThenContextLoaded () =
    let logger = NullLogger.Instance
    let pobPath = System.Environment.GetEnvironmentVariable("POB_SRC_PATH")
    let context = createPobContext logger pobPath
    Assert.NotNull(context)
