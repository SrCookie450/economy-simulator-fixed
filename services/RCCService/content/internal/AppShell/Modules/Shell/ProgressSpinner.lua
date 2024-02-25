
local CoreGui = Game:GetService("CoreGui")
local GuiRoot = CoreGui:FindFirstChild("RobloxGui")
local Modules = GuiRoot:FindFirstChild("Modules")
local ShellModules = Modules:FindFirstChild("Shell")

local Utility = require(ShellModules:FindFirstChild('Utility'))


local function CreateProgressSpinner(properties)
	properties = properties or {}

	local this = {}

	local stillGoing = true

	local loadIcon = Utility.Create'ImageLabel'
	{
		Name = "LoadIcon";
		BackgroundTransparency = 1;
		Image = 'rbxasset://textures/ui/Shell/Icons/LoadingSpinner@1080.png';
		Size = properties.Size or UDim2.new(0,99,0,100);
		ZIndex = properties.ZIndex or 7;
		Parent = properties.Parent;
		AnchorPoint = Vector2.new(0.5, 0.5);
		Position = properties.Position or UDim2.new(0.5, 0, 0.5, 0);
	}

	if properties.Visible == false then
		loadIcon.Visible = false
	end

	function this:Kill()
		stillGoing = false
		loadIcon.Parent = nil
		loadIcon:Destroy()
	end

	spawn(function()
		local t = tick()
		while stillGoing do
			local now = tick()
			local rotation = (now - t) * 360
			if loadIcon.Parent then
				loadIcon.Rotation = loadIcon.Rotation + rotation
			end
			t = now
			wait()
		end
	end)

	return this
end

return CreateProgressSpinner
