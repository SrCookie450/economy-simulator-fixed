--[[
			// VoteFrame.lua
			// Creates a vote frame for a game
]]
local CoreGui = Game:GetService("CoreGui")
local GuiRoot = CoreGui:FindFirstChild("RobloxGui")
local Modules = GuiRoot:FindFirstChild("Modules")
local ShellModules = Modules:FindFirstChild("Shell")

local AssetManager = require(ShellModules:FindFirstChild('AssetManager'))
local GlobalSettings = require(ShellModules:FindFirstChild('GlobalSettings'))
local Utility = require(ShellModules:FindFirstChild('Utility'))

local CreateVoteFrame = function(parent, position)
	local this = {}

	-- Assume 1080p
	local MAX_SIZE = 203

	local currentRedColor = GlobalSettings.RedTextColor
	local currentGreenColor = GlobalSettings.GreenTextColor

	local voteContainer = Utility.Create'Frame'
	{
		Name = "VoteContainer";
		Size = UDim2.new(0, MAX_SIZE, 0, 16);
		Position = position;
		BackgroundTransparency = 1;
		Parent = parent;
	}

	local greenContainer = Utility.Create'Frame'
	{
		Name = "VoteContainer";
		BackgroundTransparency = 1;
		Size = UDim2.new(0.5, 0, 1, 0);
		Position = UDim2.new(0, 0, 0, 0);
		ClipsDescendants = true;
		Parent = voteContainer;
	}

	local redContainer = Utility.Create'Frame'
	{
		Name = "VoteContainer";
		BackgroundTransparency = 1;
		Size = UDim2.new(1, 0, 1, 0);
		Position = UDim2.new(0.5, 0, 0, 0);
		ClipsDescendants = true;
		Parent = voteContainer;
	}

	local batteryImageRed = Utility.Create'ImageLabel'
	{
		Name = "BatteryImageRed";
		BackgroundTransparency = 1;
		ImageColor3 = currentRedColor;
		Parent = redContainer;
	}

	AssetManager.LocalImage(batteryImageRed,
		'rbxasset://textures/ui/Shell/Icons/RatingBar', {['720'] = UDim2.new(0,134,0,11); ['1080'] = UDim2.new(0,203,0,16);})
	local batteryImageGreen = batteryImageRed:Clone()
	batteryImageGreen.ImageColor3 = currentGreenColor
	batteryImageGreen.ZIndex = 2
	batteryImageGreen.Parent = greenContainer

	--[[ Public API ]]--
	function this:SetPercentFilled(percent)
		batteryImageRed.ImageColor3 =  percent and currentRedColor or GlobalSettings.GreyTextColor
		percent = Utility.Round(percent or 0, 0.1)
		greenContainer.Size = UDim2.new(percent, 0, 1, 0)
		redContainer.Position = UDim2.new(percent, 0, 0, 0)
		batteryImageRed.Position = UDim2.new(-percent, 0, 0, 0)
	end

	function this:SetImageColorTint(value)
		currentRedColor = Color3.new(GlobalSettings.RedTextColor.r * value,
			GlobalSettings.RedTextColor.g * value,
			GlobalSettings.RedTextColor.b * value)

		currentGreenColor = Color3.new(GlobalSettings.GreenTextColor.r * value,
			GlobalSettings.GreenTextColor.g * value,
			GlobalSettings.GreenTextColor.b * value)

		batteryImageRed.ImageColor3 = currentRedColor
		batteryImageGreen.ImageColor3 = currentGreenColor
	end

	function this:TweenTransparency(value, duration)
		Utility.PropertyTweener(batteryImageRed, 'ImageTransparency', batteryImageRed.ImageTransparency,
			value, duration, Utility.Linear, true)
		Utility.PropertyTweener(batteryImageGreen, 'ImageTransparency', batteryImageGreen.ImageTransparency,
			value, duration, Utility.Linear, true)
	end

	function this:SetVisible(value)
		voteContainer.Visible = value
	end

	function this:GetContainer()
		return voteContainer
	end

	function this:Destroy()
		voteContainer:Destroy()
		this = nil
	end

	return this
end

return CreateVoteFrame
