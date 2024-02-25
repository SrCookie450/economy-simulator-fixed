--[[
				// ControllerStateManager.lua

				// Handles controller state changes
]]
local CoreGui = Game:GetService("CoreGui")
local GuiRoot = CoreGui:FindFirstChild("RobloxGui")
local Modules = GuiRoot:FindFirstChild("Modules")
local ShellModules = Modules:FindFirstChild("Shell")
local PlatformService = nil
pcall(function() PlatformService = game:GetService('PlatformService') end)
local ThirdPartyUserService = nil
pcall(function() ThirdPartyUserService = game:GetService('ThirdPartyUserService') end)
local UserInputService = game:GetService('UserInputService')
local GuiService = game:GetService('GuiService')

local Http = require(ShellModules:FindFirstChild('Http'))
local NoActionOverlay = require(ShellModules:FindFirstChild('NoActionOverlay'))
local ScreenManager = require(ShellModules:FindFirstChild('ScreenManager'))
local Alerts = require(ShellModules:FindFirstChild('Alerts'))
local Utility = require(ShellModules:FindFirstChild('Utility'))
local EventHub = require(ShellModules:FindFirstChild('EventHub'))

local ControllerStateManager = {}

local LostUserGamepadCn = nil
local GainedUserGamepadCn = nil
local DisconnectCn = nil

local currentOverlay = nil

local DATAMODEL_TYPE = {
	APP_SHELL = 0;
	GAME = 1;
}


local PRESENCE_POLL_INTERVAL = Utility.GetFastVariable("XboxPresencePolling")

local AnyButtonBeganConnection = nil
local SelectionChangedConnection = nil
local ViewChangedConnection = nil
local AnyActionDone = false
local LastTimerInfo = {flag = true};

local function restartPresenceUpdateTimer()
	LastTimerInfo.flag = false;

	local info = { flag = true }

	spawn(function()
		AnyActionDone = true
		while info.flag do
			if AnyActionDone then
				Http:RegisterAppPresence()
			end
			AnyActionDone = false
			wait(PRESENCE_POLL_INTERVAL)
		end
	end)

	Utility.DisconnectEvent(AnyButtonBeganConnection)
	AnyButtonBeganConnection = UserInputService.InputBegan:connect(function(inputObject)
		AnyActionDone = true
	end)

	Utility.DisconnectEvent(SelectionChangedConnection)
	SelectionChangedConnection = GuiService.Changed:connect(function(prop)
		if prop == 'SelectedCoreObject' then
			AnyActionDone = true
		end
	end)

	LastTimerInfo = info
end

local function stopPresenceUpdateTimer()
	LastTimerInfo.flag = false;
	Utility.DisconnectEvent(AnyButtonBeganConnection)
	Utility.DisconnectEvent(SelectionChangedConnection)
end

local function closeOverlay(dataModelType)
	if dataModelType == DATAMODEL_TYPE.GAME then
		UserInputService.OverrideMouseIconBehavior = Enum.OverrideMouseIconBehavior.None

		currentOverlay:Hide()
		currentOverlay = nil
	else
		ScreenManager:CloseCurrent()
	end
end

local function showErrorOverlay(alert, dataModelType)
	local noActionOverlay = NoActionOverlay(alert)
	if dataModelType == DATAMODEL_TYPE.GAME then
		UserInputService.OverrideMouseIconBehavior = Enum.OverrideMouseIconBehavior.ForceHide

		currentOverlay = noActionOverlay
		noActionOverlay:Show()
	else
		ScreenManager:OpenScreen(noActionOverlay, false)
	end
end

local function onLostUserGamepad(dataModelType)
	local userDisplayName = ""
	if ThirdPartyUserService then
		userDisplayName = ThirdPartyUserService:GetUserDisplayName()
	end

	-- create alert
	local alert = Alerts.LostConnection["Controller"]
	alert.Msg = string.format(alert.Msg, userDisplayName)
	showErrorOverlay(alert, dataModelType)
end

local function onGainedUserGamepad(dataModelType)
	closeOverlay(dataModelType)
end

local function disconnectEvents()
	LostUserGamepadCn = Utility.DisconnectEvent(LostUserGamepadCn)
	GainedUserGamepadCn = Utility.DisconnectEvent(GainedUserGamepadCn)
end

function initAppPresenceReporting()
	restartPresenceUpdateTimer()

	Utility.DisconnectEvent(ViewChangedConnection)
	ViewChangedConnection = PlatformService.ViewChanged:connect(
		function(value)
			if value == DATAMODEL_TYPE.APP_SHELL then
				restartPresenceUpdateTimer()
			else
				stopPresenceUpdateTimer()
			end
		end
	)
end

function ControllerStateManager:Initialize()
	if not PlatformService then return end

	local dataModelType = PlatformService.DatamodelType

	disconnectEvents()
	if ThirdPartyUserService then
		LostUserGamepadCn = ThirdPartyUserService.ActiveGamepadRemoved:connect(function()
			onLostUserGamepad(dataModelType)
		end)
		GainedUserGamepadCn = ThirdPartyUserService.ActiveGamepadAdded:connect(function()
			onGainedUserGamepad(dataModelType)
		end)
	end

	if Utility.IsFastFlagEnabled("XboxRegisterAppPresence") and PlatformService.DatamodelType == DATAMODEL_TYPE.APP_SHELL then
		EventHub:addEventListener(EventHub.Notifications["AuthenticationSuccess"], "AchievementManager",
			function()
				initAppPresenceReporting();
			end
		)
	end

	-- disconnect based on DataModel type
	if dataModelType == DATAMODEL_TYPE.GAME then
		DisconnectCn = PlatformService.ViewChanged:connect(function(viewType)
			if viewType == 0 then
				disconnectEvents()
			end
		end)
	end
end

function ControllerStateManager:CheckUserConnected()
	if not PlatformService then return end

	local isGamepadConnected = UserInputService:GetGamepadConnected(Enum.UserInputType.Gamepad1)
	local dataModelType = PlatformService.DatamodelType
	if not isGamepadConnected then
		onLostUserGamepad(dataModelType)
	end
end

return ControllerStateManager
