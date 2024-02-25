
local CoreGui = Game:GetService("CoreGui")
local GuiRoot = CoreGui:FindFirstChild("RobloxGui")
local Modules = GuiRoot:FindFirstChild("Modules")
local ShellModules = Modules:FindFirstChild("Shell")
local PlatformService = nil
pcall(function() PlatformService = game:GetService('PlatformService') end)
local UserInputService = game:GetService('UserInputService')
local GuiService = game:GetService('GuiService')

local FriendsData = require(ShellModules:FindFirstChild('FriendsData'))
local FriendPresenceItem = require(ShellModules:FindFirstChild('FriendPresenceItem'))
local SideBarModule = require(ShellModules:FindFirstChild('SideBar'))
local LazyScrollingGrid = require(ShellModules:FindFirstChild('LazyScrollingGrid'))
local Strings = require(ShellModules:FindFirstChild('LocalizedStrings'))
local GameJoinModule = require(ShellModules:FindFirstChild('GameJoin'))
local Errors = require(ShellModules:FindFirstChild('Errors'))
local ErrorOverlayModule = require(ShellModules:FindFirstChild('ErrorOverlay'))
local EventHub = require(ShellModules:FindFirstChild('EventHub'))
local ScreenManager = require(ShellModules:FindFirstChild('ScreenManager'))
local Utility = require(ShellModules:FindFirstChild('Utility'))
local Analytics = require(ShellModules:FindFirstChild('Analytics'))

local SIDE_BAR_ITEMS = {
	JoinGame =  string.upper(Strings:LocalizedString("JoinGameWord"));
	ViewDetails = string.upper(Strings:LocalizedString("ViewGameDetailsWord"));
	ViewProfile = string.upper(Strings:LocalizedString("ViewGamerCardWord"));
}

-- side bar is shared between all views
local SideBar = SideBarModule()

local function getPlatformName()
	if UserInputService:GetPlatform() == Enum.Platform.XBoxOne then
		return "Xbox"
	end

	if UserInputService:GetPlatform() == Enum.Platform.PS4 then
		return "PS4"
	end

	return ""
end

local function setPresenceData(item, data)
	if UserSettings().GameSettings:InStudioMode() or game:GetService('UserInputService'):GetPlatform() == Enum.Platform.Windows then
		item:SetAvatarImage(data.UserId)
		item:SetNameText(data.Name)
		item:SetPresence(data.LastLocation, data.PlaceId ~= nil)
	elseif 	UserInputService:GetPlatform() == Enum.Platform.XBoxOne or
			UserInputService:GetPlatform() == Enum.Platform.PS4 then
		local rbxuid = data["robloxuid"]
		item:SetAvatarImage(rbxuid)
		item:SetNameText(data["display"])
		if data["PlaceId"] and data["LastLocation"] then
			item:SetPresence(data["LastLocation"], true)
		elseif data["rich"] then
			local richTbl = data["rich"]
			if #richTbl > 0 then
				local presence = richTbl[#richTbl]
				if presence["title"] == "ROBLOX" then
					item:SetPresence("ROBLOX", false)
				else
					item:SetPresence(getPlatformName(), false)
				end
			end
		end
	end
end


-- viewGridConfig - table containing init parameters for scrolling grid
-- updateFunc - function that will be called when FriendData update
local createLazyFriendsView = function(viewGridConfig, updateFunc)
	local viewGridContainer = LazyScrollingGrid(viewGridConfig)
	local this = {}

	local presenceItemToSidebarEvent = {}

	local function connectSideBar(item, data)
		Utility.DisconnectEvent(presenceItemToSidebarEvent[item])
		local container = item:GetContainer()
		presenceItemToSidebarEvent[item] = container.MouseButton1Click:connect(function()
			-- rebuild side bar based on current data
			SideBar:RemoveAllItems()
			function SideBar:GetAnalyticsInfo()
				return {[Analytics.WidgetNames('WidgetId')] = "FriendsSideBar"}
			end
			local inGame = data["PlaceId"] ~= nil
			if inGame and not data["IsPrivateSession"] then
				SideBar:AddItem(SIDE_BAR_ITEMS.JoinGame, function()
					if Utility.IsFastFlagEnabled("XboxFriendsAnalytics") then
						Analytics.SetRBXEventStream("FriendSidebarJoinGame")
					end
					GameJoinModule:StartGame(GameJoinModule.JoinType.Follow, data["robloxuid"])
				end)
				SideBar:AddItem(SIDE_BAR_ITEMS.ViewDetails, function()
					-- pass nil for iconId, gameDetail will fetch
					EventHub:dispatchEvent(EventHub.Notifications["OpenGameDetail"], data["PlaceId"], data["LastLocation"], nil)
				end)
			end
			SideBar:AddItem(SIDE_BAR_ITEMS.ViewProfile, function()
				if PlatformService and data["xuid"] then
					local success, result = pcall(function()
						PlatformService:PopupProfileUI(Enum.UserInputType.Gamepad1, data["xuid"])
					end)
					-- NOTE: This will try to pop up the xbox system gamer card, failure will be handled
					-- by the xbox.
					if not success then
						print("PlatformService:PopupProfileUI failed because,", result)
					end
				end
			end)
			ScreenManager:OpenScreen(SideBar, false)
		end)
	end

	local numFriends = 0
	local presenceItems  = {}
	local presenceItemDirty = {}

	local function getPresenceItemForData(data)
		local idStr = tostring(data.xuid)
		local presenceItem = presenceItems[idStr]
		if presenceItem == nil then
			presenceItem = FriendPresenceItem(UDim2.new(0, 446, 0, 114), idStr)
			presenceItemDirty[presenceItem] = true
			presenceItems[idStr] = presenceItem
		end

		if presenceItemDirty[presenceItem] then
			setPresenceData(presenceItem, data)
			connectSideBar(presenceItem, data)
			presenceItemDirty[presenceItem] = nil
		end

		return presenceItem
	end

	local function getPresenceItemByIndex(i)
		local currentFriendsData = FriendsData.GetOnlineFriendsAsync()
		local data = currentFriendsData[i]
		return data and getPresenceItemForData(data):GetContainer()
	end

	local function invalidatePresenceItemForData(data)
		local idStr = tostring(data.xuid)
		local presenceItem = presenceItems[idStr]
		if presenceItem then
			presenceItemDirty[presenceItem] = true
		end
	end

	local function finishFriendsEventHandler(friendsDataList)
		for i = 1, #friendsDataList do
			local data = friendsDataList[i]
			invalidatePresenceItemForData(data)
		end

		spawn(function() viewGridContainer:Recalc() end)

		if updateFunc then
			updateFunc( numFriends )
		end
	end

	local function onFriendsAdded(friendsDataList)
		viewGridContainer:SetItemCallback( getPresenceItemByIndex )
		numFriends = numFriends + #friendsDataList
		finishFriendsEventHandler(friendsDataList)
	end

	local function onFriendsRemoved(friendsDataList)
		viewGridContainer:SetItemCallback( getPresenceItemByIndex )
		numFriends = numFriends - #friendsDataList
		finishFriendsEventHandler(friendsDataList)
	end

	FriendsData.ConnectAddEvent(onFriendsAdded)
	FriendsData.ConnectRemoveEvent(onFriendsRemoved)

	function this:GetAnalyticsInfo()
		return {[Analytics.WidgetNames('WidgetId')] = Analytics.WidgetNames('LazyFriendsViewId')}
	end

	function this:GetDefaultFocusItem()
		return nil
	end

	function this:SetParent(parent)
		viewGridContainer:SetParent(parent)
	end

	function this:Focus()
		viewGridContainer:Focus()
	end

	function this:RemoveFocus()
		viewGridContainer:RemoveFocus()
	end

	return this
end

return createLazyFriendsView
