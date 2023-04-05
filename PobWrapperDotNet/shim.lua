arg = {}
bit = {}
bit.rshift = function(a, b) return a >> b end
bit.bor = function(a, b) return a | b end
bit.band = function(a, b) return a & b end
bit.bnot = function(a) return ~a end
loadstring = load
math.pow = function(a, b) return a ^ b end
local stringFormat = string.format
string.format = function(f, ...)
                    local args = {...}
                    for i, v in ipairs(args) do
                        if string.find(tostring(v),"nan") then
                            args[i] = -1
                        elseif tonumber(v) ~= nil and math.tointeger(v) ~= v and string.find(f, "%%d") ~= nil then
                            args[i] = math.floor(v)
                        end
                    end
                    return stringFormat(f, unpack(args))
                end
local tableInsert = table.insert
table.insert = function(t, ...)
                    local args = {...}
                    if #args == 2 and args[1] > (#t+1) then
                        t[args[1]]=args[2]
                    else
                        tableInsert(t, ...)
                    end
                end

function unpack(ret) return table.unpack(ret) end
function setfenv(a,b) return end
-- Needed to work around the single percentage signs included in
-- replace string in commit bb0c57fcee226a81c6019c45c8669aabd66164f4
local l_gsub = string.gsub
string.gsub = function(s, ...)
                    local args = {...}
                    if #args >= 2 and args[2] ~= nil and args[2] ~= '' and type(args[2]) == 'string' 
                                    and string.find(args[2], "%%%(") == 1 
                    then
                        args[2] = l_gsub(args[2], "%%", "")
                    end
                    return l_gsub(s, unpack(args))
                end