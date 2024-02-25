-- Written by Kip Turner, Copyright ROBLOX 2015

-- App's Main
local CoreGui = Game:GetService("CoreGui")
local ContentProvider = Game:GetService("ContentProvider")
local RobloxGui = CoreGui:FindFirstChild("RobloxGui")
local Modules = RobloxGui:FindFirstChild("Modules")
local ShellModules = Modules:FindFirstChild("Shell")

-- TODO: Will use for re-auth when finished
local UserInputService = game:GetService('UserInputService')
local PlatformService = nil
pcall(function() PlatformService = game:GetService('PlatformService') end)
local ThirdPartyUserService = nil
pcall(function() ThirdPartyUserService = game:GetService('ThirdPartyUserService') end)

local GuiRoot = CoreGui:FindFirstChild("RobloxGui")

local Utility = require(ShellModules:FindFirstChild('Utility'))
local GlobalSettings = require(ShellModules:FindFirstChild('GlobalSettings'))
local AppHubModule = require(ShellModules:FindFirstChild('AppHub'))
local ScreenManager = require(ShellModules:FindFirstChild('ScreenManager'))
local Errors = require(ShellModules:FindFirstChild('Errors'))
local ErrorOverlay = require(ShellModules:FindFirstChild('ErrorOverlay'))
local EventHub = require(ShellModules:FindFirstChild('EventHub'))
local GameGenreScreen = require(ShellModules:FindFirstChild('GameGenreScreen'))
local GameDetailModule = require(ShellModules:FindFirstChild('GameDetail'))
local EngagementScreenModule = require(ShellModules:FindFirstChild('EngagementScreen'))
local BadgeScreenModule = require(ShellModules:FindFirstChild('BadgeScreen'))

local FriendsData = require(ShellModules:FindFirstChild('FriendsData'))
local UserData = require(ShellModules:FindFirstChild('UserData'))
local SoundManager = require(ShellModules:FindFirstChild('SoundManager'))
local AchievementManager = require(ShellModules:FindFirstChild('AchievementManager'))
local HeroStatsManager = require(ShellModules:FindFirstChild('HeroStatsManager'))
local ControllerStateManager = require(ShellModules:FindFirstChild('ControllerStateManager'))
local Alerts = require(ShellModules:FindFirstChild('Alerts'))

local UseVRAppShellTech = Utility.ShouldUseVRAppLobby()

local AccountAgeWidget = require(ShellModules:FindFirstChild('AccountAgeWidget'))
local Strings = require(ShellModules:FindFirstChild('LocalizedStrings'))

local CameraManager;
if not UseVRAppShellTech then
	CameraManager = require(ShellModules:FindFirstChild('CameraManager'))
end

local ShowAccountAgeInAppShell = Utility.IsFastFlagEnabled("XboxShowAccountAgeInAppShell")


-- Change this to update if we are allowing VREnabled to change
if UseVRAppShellTech then
	settings().Rendering.QualityLevel = 10
	local Panel3D = require(RobloxGui.Modules.VR.Panel3D)

	local appShellVrPanel = Panel3D.Get("AppShellVR")
	appShellVrPanel:ResizePixels(1920, 1080, 200)
	appShellVrPanel:SetType(Panel3D.Type.Fixed, { CFrame = CFrame.new(0, 0, -8.3) })
	-- NOTE: Need to keep the camera from moving when in VR
	-- otherwise you may induce nausea
	workspace.CurrentCamera.CameraType = Enum.CameraType.Scriptable

	appShellVrPanel:SetVisible(true)
	appShellVrPanel:SetCanFade(false)

	local recenterNeeded = true
	local panelLocalCF = CFrame.new(0, 0, 8.3)

	function appShellVrPanel:PreUpdate(cameraCF, cameraRenderCF, userHeadCF, lookRay)
		if not appShellVrPanel.isLookedAt and recenterNeeded then

			local headForwardCF = Panel3D.GetHeadLookXZ()
			local panelOriginCF = CFrame.new(userHeadCF.p) * headForwardCF
			self.localCF = panelOriginCF * panelLocalCF

			recenterNeeded = false
		end
	end

	GuiRoot = appShellVrPanel:GetGUI()
	GuiRoot.AlwaysOnTop = false
end

local TIME_BETWEEN_BACKGROUND_TRANSITIONS = 30
local CROSSFADE_DURATION = 1.5

local BACKGROUND_ASSETS =
{
	'Home_screen_01.png';
	'Home_screen_02.png';
	'Home_screen_03.png';
	'Home_screen_04.png';
}

local AppHomeContainer = Utility.Create'Frame'
{
	Size = UDim2.new(1, 0, 1, 0);
	BackgroundTransparency = 0;
	BorderSizePixel = 0;
	BackgroundColor3 = Color3.new(0,0,0);
	ClipsDescendants = true;
	Name = 'AppHomeContainer';
	Parent = GuiRoot;
}


local BackgroundAssetIndex = 1
local Background = Utility.Create'ImageLabel'
{
	Size = UDim2.new(1, 0, 1, 0);
	BackgroundTransparency = 0;
	BorderSizePixel = 0;
	Name = 'Background';
	Image = 'rbxasset://textures/ui/Shell/Background/' .. BACKGROUND_ASSETS[BackgroundAssetIndex];
	Parent = AppHomeContainer;
}
local CrossfadeBackground = Utility.Create'ImageLabel'
{
	Size = UDim2.new(1, 0, 1, 0);
	BackgroundTransparency = 1;
	BorderSizePixel = 0;
	Name = 'CrossfadeBackground';
	Image = '';
	Parent = Background;
}

Background.ImageTransparency = 0

-- TODO: the fastflag has been killed, you can remove the check
--     - Max
local Is3DBackgroundEnabled = true


-- print ("BACKGROUND")
-- print (Background3DSuccess)
-- print (Background3DValue)

local soundHandle = SoundManager:Play('BackgroundLoop', 0.33, true)
if soundHandle then
	local bgmLoopConn = nil
	bgmLoopConn = soundHandle.DidLoop:connect(function(soundId, loopCount)
		-- print("DidLoop BackgroundLoop" , soundId, "times:", loopCount)
		if loopCount >= 1 then
			bgmLoopConn = Utility.DisconnectEvent(bgmLoopConn)
			if soundHandle then
				-- print("Volume fade out")
				SoundManager:TweenSound(soundHandle, 0.1, 3)
				-- Utility.PropertyTweener(soundHandle, 'Volume', soundHandle.Volume, 0.1, 3, Utility.EaseInOutQuad, true)
			end
		end
	end)
end

-- TODO: Can we remove this flag now? Ask dan and create task
--
-- TODO: the fastflag has been killed, the intro scene is always on. You can remove this check.
--     - Max
if Is3DBackgroundEnabled then
	spawn(function()

		while true do
			local queueSize = ContentProvider.RequestQueueSize

			if queueSize == 0 then
				if not UseVRAppShellTech then
					CameraManager:EnableCameraControl()
				end
				spawn(function()
					if not UseVRAppShellTech then
						CameraManager:CameraMoveToAsync()
					end
				end)


				Background.BackgroundTransparency = 1;
				Background.ImageTransparency = 1
				CrossfadeBackground.ImageTransparency = 1
				if UseVRAppShellTech then
					AppHomeContainer.BackgroundTransparency = 0.5
					Background.ImageTransparency = 1
				else
					AppHomeContainer.BackgroundTransparency = 1
					Utility.PropertyTweener(Background, 'ImageTransparency', 0, 1, CROSSFADE_DURATION,  Utility.EaseInOutQuad, true)
				end

				break
			end

			wait(0.01)
		end

	end)
else
	spawn(function()
		while true do
			wait(TIME_BETWEEN_BACKGROUND_TRANSITIONS)

			-- 1-based modulo indexing
			BackgroundAssetIndex = ((BackgroundAssetIndex) % #BACKGROUND_ASSETS) + 1

			CrossfadeBackground.Image = Background.Image
			CrossfadeBackground.ImageTransparency = 0
			Background.Image = 'rbxasset://textures/ui/Shell/Background/' .. BACKGROUND_ASSETS[BackgroundAssetIndex];
			Background.ImageTransparency = 1

			Utility.PropertyTweener(Background, 'ImageTransparency', 1, 0, CROSSFADE_DURATION,  Utility.EaseInOutQuad, true)
			wait(CROSSFADE_DURATION)
			Utility.PropertyTweener(CrossfadeBackground, 'ImageTransparency', 0, 1, CROSSFADE_DURATION,  Utility.EaseInOutQuad, true)
		end
	end)
end

local AspectRatioProtector = Utility.Create'Frame'
{
	Size = UDim2.new(1, 0, 1, 0);
	Position = UDim2.new(0,0,0,0);
	BackgroundTransparency = 1;
	Name = 'AspectRatioProtector';
	Parent = CrossfadeBackground;
}

local function OnAbsoluteSizeChanged()
	local newSize = Utility.CalculateFit(AppHomeContainer, Vector2.new(16,9))
	if newSize ~= AspectRatioProtector.Size then
		AspectRatioProtector.Size = newSize
		AspectRatioProtector.AnchorPoint = Vector2.new(0.5, 0.5)
		AspectRatioProtector.Position = UDim2.new(0.5, 0, 0.5, 0)
	end
end

AppHomeContainer.Changed:connect(function(prop)
	if prop == 'AbsoluteSize' then
		OnAbsoluteSizeChanged()
	end
end)
OnAbsoluteSizeChanged()

local ActionSafeContainer = Utility.Create'Frame'
{
	Size = UDim2.new(1, 0, 1, 0) - (GlobalSettings.ActionSafeInset + GlobalSettings.ActionSafeInset);
	Position = UDim2.new(0,0,0,0) + GlobalSettings.ActionSafeInset;
	BackgroundTransparency = 1;
	Name = 'ActionSafeContainer';
	Parent = AspectRatioProtector;
}

local titleActionSafeDelta = GlobalSettings.TitleSafeInset -- - GlobalSettings.ActionSafeInset

local TitleSafeContainer = Utility.Create'Frame'
{
	Size = UDim2.new(1, 0, 1, 0) - (titleActionSafeDelta + titleActionSafeDelta);
	Position = UDim2.new(0,0,0,0) + titleActionSafeDelta;
	BackgroundTransparency = 1;
	Name = 'TitleSafeContainer';
	Parent = ActionSafeContainer;
}

local EngagementScreen = EngagementScreenModule()
EngagementScreen:SetParent(TitleSafeContainer)

-- Account Age View
local accountAgeWidget = AccountAgeWidget()

local userInputConn = nil

local function returnToEngagementScreen()
	if ScreenManager:ContainsScreen(EngagementScreen) then
		while ScreenManager:GetTopScreen() ~= EngagementScreen do
			ScreenManager:CloseCurrent()
		end
	else
		while ScreenManager:GetTopScreen() do
			ScreenManager:CloseCurrent()
		end
		ScreenManager:OpenScreen(EngagementScreen)
	end
end

local AppHub = nil
local function onAuthenticationSuccess(isNewLinkedAccount)
	-- Set UserData
	UserData:Initialize()

	-- Unwind Screens if needed - this will be needed once we put in account linking
	returnToEngagementScreen()

	AppHub = AppHubModule()
	AppHub:SetParent(TitleSafeContainer)

	-- Account Age Setting
	if ShowAccountAgeInAppShell then
		spawn(function()
			local text = Strings:LocalizedString("AccountUnder13Phrase")

			local result = UserData:IsAccountOver13Async()
			if result == UserData.AccountAgeResult.Over13 then
				text = Strings:LocalizedString("AccountOver13Phrase")
			end

			if result ~= UserData.AccountAgeResult.Unknown then
				accountAgeWidget:SetText(text)
				accountAgeWidget:SetParent(TitleSafeContainer)
			end
		end)
	end

	EventHub:addEventListener(EventHub.Notifications["OpenGameDetail"], "gameDetail",
		function(placeId, placeName, iconId, gameData)
			local gameDetail = GameDetailModule(placeId, placeName, iconId, gameData)
			gameDetail:SetParent(TitleSafeContainer);
			ScreenManager:OpenScreen(gameDetail);
		end);
	EventHub:addEventListener(EventHub.Notifications["OpenGameGenre"], "gameGenre",
		function(sortName, gameCollection)
			local gameGenre = GameGenreScreen(sortName, gameCollection)
			gameGenre:SetParent(TitleSafeContainer);
			ScreenManager:OpenScreen(gameGenre);
		end);
	EventHub:addEventListener(EventHub.Notifications["OpenBadgeScreen"], "gameBadges",
		function(badgeData, previousScreenName)
			local badgeScreen = BadgeScreenModule(badgeData, previousScreenName)
			badgeScreen:SetParent(TitleSafeContainer);
			ScreenManager:OpenScreen(badgeScreen);
		end)
if not Utility.IsFastFlagEnabled("XboxFriendsEvents") then
	EventHub:addEventListener(EventHub.Notifications["OpenSocialScreen"], "socialScreen",
		function(socialScreen)
			socialScreen:SetParent(TitleSafeContainer);
			ScreenManager:OpenScreen(socialScreen);
		end)
end
	EventHub:addEventListener(EventHub.Notifications["OpenSettingsScreen"], "settingsScreen",
		function(settingsScreen)
			settingsScreen:SetParent(TitleSafeContainer);
			ScreenManager:OpenScreen(settingsScreen);
		end)

	Utility.DisconnectEvent(userInputConn)
	userInputConn = UserInputService.InputBegan:connect(function(input, processed)
		if input.KeyCode == Enum.KeyCode.ButtonX then
			-- EventHub:dispatchEvent(EventHub.Notifications["TestXButtonPressed"])
		end
	end)

	-- disable control control
	if not UseVRAppShellTech then
		CameraManager:DisableCameraControl()
	end

	print("User and Event initialization finished. Opening AppHub")
	ScreenManager:OpenScreen(AppHub);

	-- show info popup to users on newly linked accounts
	if isNewLinkedAccount == true then
		ScreenManager:OpenScreen(ErrorOverlay(Alerts.PlatformLink), false)
	end

	if Utility.IsFastFlagEnabled("XboxFriendsEvents") then
		FriendsData.GetOnlineFriendsAsync()
	end
end

local function onReAuthentication(reauthenticationReason)
	print("Beging Reauth, cleaning things up")
	-- unwind ScreenManager

	-- Account Age View
	if ShowAccountAgeInAppShell then
		accountAgeWidget:SetParent(nil)
		accountAgeWidget:SetText("")
	end

	returnToEngagementScreen()

	UserData:Reset()
	FriendsData.Reset()
	AppHub = nil
	EventHub:removeEventListener(EventHub.Notifications["OpenGameDetail"], "gameDetail")
	EventHub:removeEventListener(EventHub.Notifications["OpenGameGenre"], "gameGenre")
	EventHub:removeEventListener(EventHub.Notifications["OpenBadgeScreen"], "gameBadges")
if not Utility.IsFastFlagEnabled("XboxFriendsEvents") then
	EventHub:removeEventListener(EventHub.Notifications["OpenSocialScreen"], "socialScreen")
	EventHub:removeEventListener(EventHub.Notifications["OpenSocialScreen"], "socialScreen")
end
	EventHub:removeEventListener(EventHub.Notifications["OpenSettingsScreen"], "settingsScreen")
	userInputConn = Utility.DisconnectEvent(userInputConn)
	print("Reauth complete. Return to engagement screen.")

	-- show reason overlay
	local alert = Alerts.SignOut[reauthenticationReason] or Alerts.Default

	if alert then
		ScreenManager:OpenScreen(ErrorOverlay(alert), false)
	end

	-- re-enable camera control
	if not UseVRAppShellTech then
		CameraManager:EnableCameraControl()
	end
end

local function onGameJoin(joinResult)
	-- 0 is success, anything else is an error
	local joinSuccess = joinResult == 0
	if not joinSuccess then
		local err = Errors.GameJoin[joinResult] or Errors.Default
		ScreenManager:OpenScreen(ErrorOverlay(err), false)
	end
	EventHub:dispatchEvent(EventHub.Notifications["GameJoin"], joinSuccess)
end

if PlatformService then
	PlatformService.GameJoined:connect(onGameJoin)
end

if ThirdPartyUserService then
	ThirdPartyUserService.ActiveUserSignedOut:connect(onReAuthentication)
end

ControllerStateManager:Initialize()

EventHub:addEventListener(EventHub.Notifications["AuthenticationSuccess"], "authUserSuccess", function(isNewLinkedAccount)
		print("User authenticated, initializing app shell")
		onAuthenticationSuccess(isNewLinkedAccount)
	end);
ScreenManager:OpenScreen(EngagementScreen)

UserInputService.MouseIconEnabled = false

return {}
