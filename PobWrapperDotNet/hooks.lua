dofile("HeadlessWrapper.lua")
calcs = LoadModule("Modules/Calcs")
dofile("Classes/Item.lua")

function calculateWithItem(slotString, itemString)
    local testItem = new("Item", itemString)
    return calculator({repSlotName=slotString, repItem=testItem})
end

function setCustomMods(mods)
    build.configTab.input.customMods = mods
end

function getCalcs()
    local calculator = calcs.getMiscCalculator(build)
    print("BAZBAR")
    return calculator()
    -- build.calcsTab:getMiscCalculator()
end