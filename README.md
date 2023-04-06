| NuGet |
| ------|
|[![nuget](https://badgen.net/nuget/v/PobWrapperDotNet?icon=nuget)](https://www.nuget.org/packages/PobWrapperDotNet)|
# PobWrapper

A dotnet wrapper around the core logic within PathOfBulding (https://pathofbuilding.community/)

## Description

The main output of this repo is a library that can be consumed by other dotnet applications in order to make calls to the underlying calculation logic that exists within PathOfBuilding. Interfacing directly with the Lua within PoB can be difficult and this library aims to provide a very thin wrapper layer that will ease development of future applications that rely on the PoB internals but aren't necessarily appropriate to build directly into PoB.

## Getting Started

### Dependencies

* [NLua](https://github.com/NLua/NLua) is being used to bridge dotnet calls to lua

### Installing

* Nuget (https://www.nuget.org/packages/PobWrapperDotNet/)
* PathOfBuilding:
  * The library itself doesn't include or install the needed PathOfBuilding Lua, it just takes in a path to where that lua has been pulled, so if you are consuming this you will need to have some other way of pulling in the PoB repo. See the included Dockerfiles for examples.

### Calling the library


* Before anything can be done we need to create the initial context object. This will load any lua shims, then load all the PoB lua, then load any hooks that will be later called. This generally can take ~60 seconds so try to do it just once if you can.
```
open PobWrapperDotNet.PobWrapper

let logger = NullLogger.Instance
let pobPath = System.Environment.GetEnvironmentVariable("POB_SRC_PATH")
let context = createPobContext logger pobPath
```

* Currently the only way to load in build info is via the Json info from PoE (e.g. https://www.pathofexile.com/character-window/get-passive-skills)
```
let itemJson = "<ResultOfApiCall>"
let skillJson = "<ResultOfApiCall>"
let updatedContext = setBuildState logger context itemJson skillJson
```

* With the context updated with the build we can now get a very loosely typed map of all of the calculations that PoB does.
```
let calcs = getCalcsForContext logger context
printfn "%A" calcs
```

* The above could print calculation outputs that look like:
```
{"AbsorptionCharges":"0","AbsorptionChargesMax":"0","AbsorptionChargesMin":"0","Accuracy":"520","AccuracyHitChance":"100","ActionSpeedMod":"1","ActiveMineLimit":"15","ActiveTrapLimit":"15","AfflictionCharges":"0","AfflictionChargesMax":"0","AfflictionChargesMin":"0","AilmentWarcryEffect":"1","AnyAegis":"False","AnyBypass":"True","AnyGuard":"False","AnySpecificMindOverMatter":"False","AnyTakenReflect":"False","AreaOfEffectMod":"1.4","AreaOfEffectRadius":"14","Armour":"60","ArmourDefense":"0","AttackDodgeChance":"0","AttackDodgeChanceOverCap":"0","AttackTakenHitMult":"1","AverageBlockChance":"15.5","AverageBurstDamage":"30929.581056","AverageBurstHits":"1","AverageDamage":"30929.581056" }
```

## Help
PoB appears to be targeting Lua 5.1 and NLua is running against Lua 5.4, so a shim has been created to fix the many backwards incompatible changes that happened between those versions. Since the shim doesn't cover all breaking changes (just the ones that are in the critical path of the calls that this library makes) updates to PoB will inevitably break this unless appropriate updates are made to the shim lua. Debugging lua issues can be quite painful and as time goes on hopefully some additional tools can be built into this package to alleviate that pain.

## To Do
- [ ] Expose setting of skill group (right now it is using the PoB behavior that auto selects whichever group does the most damage)
- [ ] Expose setting of bandit configs
- [ ] Expose setting of pantheon configs
- [ ] Expose setting of enemy attributes
- [ ] Allow importing of non-json based build info
- [ ] Add examples in C#

## Authors
Adam Kilpatrick (https://github.com/adamkilpatrick)

## Version History


* 0.1
    * Initial Release

## License

This project is licensed under the MIT License - see the LICENSE.md file for details

## Acknowledgments

* [PathOfBuildingCommunityFork](https://pathofbuilding.community/) - The amazing people behind this project are able to consistently turn around and model all the absolutely crazy interactions being regularly added to PoE. Without their work this project would have no purpose and PoE itself would be unplayable.
