--


local coreGui = game:GetService("CoreGui")
local robloxGui = coreGui:FindFirstChild("RobloxGui")
local scriptContext = game:GetService("ScriptContext")
local serverStorage = game:GetService('ServerStorage')
local starterGui = game:GetService('StarterGui')
local screenGuiV1 = serverStorage:WaitForChild('ScreenGuiV1')
local screenGuiV2 = serverStorage:WaitForChild('ScreenGuiV2')


local enabledAvatarEditorV2 = false
do
	local success, flagValue = pcall(function()
		return settings():GetFFlag("EnabledAvatarEditorV2")
	end)
	if success then
		enabledAvatarEditorV2 = flagValue
	end
end



if enabledAvatarEditorV2 then
	screenGuiV2.Name = 'ScreenGui'
	screenGuiV2.Parent = starterGui
	scriptContext:AddCoreScriptLocal("AvatarStarterScriptV2", robloxGui)
else
	screenGuiV1.Name = 'ScreenGui'
	screenGuiV1.Parent = starterGui
	scriptContext:AddCoreScriptLocal("AvatarStarterScriptV1", robloxGui)
end







