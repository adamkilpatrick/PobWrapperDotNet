dofile("HeadlessWrapper.lua")
calcs = LoadModule("Modules/Calcs")
dofile("Classes/Item.lua")
dofile("Classes/ItemsTab.lua")
itemSlots = {"Weapon 1", "Weapon 2", "Helmet", "Body Armour", "Gloves", "Boots", "Amulet", "Ring 1", "Ring 2", "Belt"}

function calculateWithItem(slotString, itemString)
    local calculator = calcs.getMiscCalculator(build)
    local testItem = new("Item", itemString)
    return calculator({repSlotName=slotString, repItem=testItem})
end

function setCustomMods(mods)
    build.configTab.input.customMods = mods
end

function getCalcs()
    local calculator = calcs.getMiscCalculator(build)
    return calculator()
end

function getValidSlotsForItem(itemString)
    local testItem = new("Item", itemString)
    local itemsTab = new("ItemsTab", build)
    local validSlots = {}
    for _ , slot in pairs(itemSlots) do
        if itemsTab:IsItemValidForSlot(testItem, slot) then
            table.insert(validSlots, #validSlots+1, slot)
        end
    end
    return validSlots
end