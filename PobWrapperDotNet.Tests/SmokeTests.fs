module PobWrapperDotNet.Tests

open NUnit.Framework
open PobWrapperDotNet.PobWrapper
open Microsoft.Extensions.Logging.Abstractions
open Microsoft.Extensions.Logging

let mutable context:PobContext option = None
let mutable logger:ILogger option = None

[<OneTimeSetUp>]
let Setup () =
    let pobPath = System.Environment.GetEnvironmentVariable("POB_SRC_PATH")
    logger <- Some NullLogger.Instance
    context <- (createPobContext logger.Value pobPath) |> Some
    ()

[<Test>]
let WhenContextCreatedFromPathThenContextLoaded () =
    Assert.NotNull(context)

[<Test>]
let WhenCalcWithItemOverrideThenCalcsOutputDiffersFromBase () =
    let itemString = """
        Item Class: Thrusting One Hand Swords
        Rarity: Magic
        Wyrmbone Rapier of the Boxer
        --------
        One Handed Sword
        Physical Damage: 13-51
        Critical Strike Chance: 5.50%
        Attacks per Second: 1.50
        Weapon Range: 14
        --------
        Requirements:
        Level: 37
        Dex: 122
        --------
        Sockets: G-B 
        --------
        Item Level: 40
        --------
        +25% to Global Critical Strike Multiplier (implicit)
        --------
        10% reduced Enemy Stun Threshold (fractured)
        --------
        Fractured Item
    """

    let baseCalcs = getCalcsForContext logger.Value context.Value
    let overrideCalcs = getCalcsForContextWithItemOverride logger.Value context.Value Weapon1 itemString
    let diffCalcs = baseCalcs
                        |> Map.filter(fun key value -> Map.containsKey key overrideCalcs)
                        |> Map.filter(fun key value -> not((Map.find key overrideCalcs)=value))

    Assert.NotNull(diffCalcs)
    Assert.Greater(diffCalcs |> Map.keys |> Seq.cast |> Seq.length, 0)

[<Test>]
let WhenCheckingForValidItemSlotsThenCorrectSlotsReturned () =
    let itemString = """
        Item Class: Thrusting One Hand Swords
        Rarity: Magic
        Wyrmbone Rapier of the Boxer
        --------
        One Handed Sword
        Physical Damage: 13-51
        Critical Strike Chance: 5.50%
        Attacks per Second: 1.50
        Weapon Range: 14
        --------
        Requirements:
        Level: 37
        Dex: 122
        --------
        Sockets: G-B 
        --------
        Item Level: 40
        --------
        +25% to Global Critical Strike Multiplier (implicit)
        --------
        10% reduced Enemy Stun Threshold (fractured)
        --------
        Fractured Item
    """

    let validSlots = getValidSlotsForItem logger.Value context.Value itemString
    Assert.AreEqual(2, validSlots |> Seq.length)