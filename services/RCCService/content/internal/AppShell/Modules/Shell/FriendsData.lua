--[[
			// FriendsData.lua

			// Caches the current friends pagination to used by anyone in the app
			// polls every POLL_DELAY and gets the latest pagination

			// TODO:
				Need polling to update friends. How are we going to handle all the cases
					like the person you're selecting going offline, etc..
]]
local CoreGui = game:GetService("CoreGui")
local GuiRoot = CoreGui:FindFirstChild("RobloxGui")
local Modules = GuiRoot:FindFirstChild("Modules")
local ShellModules = Modules:FindFirstChild("Shell")
local Players = game:GetService('Players')
local HttpService = game:GetService('HttpService')
local PlatformService = nil
pcall(function() PlatformService = game:GetService('PlatformService') end)
local UserInputService = game:GetService('UserInputService')

local Http = require(ShellModules:FindFirstChild('Http'))
local Utility = require(ShellModules:FindFirstChild('Utility'))
local UserData = require(ShellModules:FindFirstChild('UserData'))
local SortData = require(ShellModules:FindFirstChild('SortData'))
local Strings = require(ShellModules:FindFirstChild('LocalizedStrings'))
local Analytics = require(ShellModules:FindFirstChild('Analytics'))

local FriendService = nil
if Utility.IsFastFlagEnabled("XboxFriendsEvents") or Utility.IsFastFlagEnabled("XboxPollUsingFriendService") then
	pcall(function() FriendService = game:GetService('FriendService') end)
end

-- NOTE: This is just required for fixing Usernames in auto-generatd games
local GameData = require(ShellModules:FindFirstChild('GameData'))
local ConvertMyPlaceNameInXboxAppFlag = Utility.IsFastFlagEnabled("ConvertMyPlaceNameInXboxApp")

local FriendsData = {}

local pollDelay = Utility.GetFastVariable("XboxFriendsPolling")
if pollDelay then
	pollDelay = tonumber(pollDelay)
end

local POLL_DELAY = pollDelay or 30
local STATUS = {
	UNKNOWN = "Unknown";
	ONLINE = "Online";
	OFFLINE = "Offline";
	AWAY = "Away";
}

local isOnlineFriendsPolling = false
local myCurrentFriendsData = nil
local updateEventCns = {}

local function filterXboxFriends(friendsData)
	local titleId = PlatformService and tostring(PlatformService:GetTitleId()) or ''
	local onlineRobloxFriendsData = {}
	local onlineRobloxFriendsUserIds = {}
	local onlineFriendsData = {}
	-- filter into two list, those online in roblox and those online
	-- for those online, also get their roblox userId
	for i = 1, #friendsData do
		local data = friendsData[i]
		if data["status"] and data["status"] == STATUS.ONLINE then
			if data["rich"] then
				-- get rich presence table, last entry is most recent activity from user
				local richTbl = data["rich"]
				richTbl = richTbl[#richTbl]
				if richTbl["titleId"] == titleId then
					table.insert(onlineRobloxFriendsData, data)
					table.insert(onlineRobloxFriendsUserIds, data["robloxuid"])
				else
					table.insert(onlineFriendsData, data)
				end
			end
		end
	end

	-- now get roblox friends presence in roblox
	local robloxPresence = nil
	if #onlineRobloxFriendsUserIds > 0 then
		local jsonTable = {}
		jsonTable["userIds"] = onlineRobloxFriendsUserIds
		local jsonPostBody = HttpService:JSONEncode(jsonTable)
		robloxPresence = Http.GetUsersOnlinePresenceAsync(jsonPostBody)
		if robloxPresence and robloxPresence["UserPresences"] then
			robloxPresence = robloxPresence["UserPresences"]
		end
	end

	-- now append roblox presence data to each users data
	if robloxPresence then
		for i = 1, #onlineRobloxFriendsData do
			if robloxPresence[i] then
				local data = onlineRobloxFriendsData[i]
				-- make sure we have the right person
				for j = 1, #robloxPresence do
					local rbxPresenceData = robloxPresence[j]
					local rbxUserId = rbxPresenceData["VisitorId"]
					if rbxUserId == data["robloxuid"] then
						if rbxPresenceData["IsOnline"] == true then
							local placeId = rbxPresenceData["PlaceId"]
							local lastLocation = rbxPresenceData["LastLocation"]

							-- If the lastLocation for a user is some user place with a GeneratedUsername in it
							-- then replace it with the actual creator name!
							if ConvertMyPlaceNameInXboxAppFlag and placeId and lastLocation and GameData:ExtractGeneratedUsername(lastLocation) then
								local gameCreator = GameData:GetGameCreatorAsync(placeId)
								if gameCreator then
									lastLocation = GameData:GetFilteredGameName(lastLocation, gameCreator)
								end
							end

							data["PlaceId"] = placeId
							data["LastLocation"] = lastLocation
						end
						-- remove from list and gtfo
						table.remove(robloxPresence, j)
						break
					end
				end
			end
		end
	end

	-- now sort those in roblox
	table.sort(onlineRobloxFriendsData, function(a, b)
		if a["PlaceId"] and b["PlaceId"] then
			return a["display"] < b["display"]
		end
		if a["PlaceId"] then
			return true
		end
		if b["PlaceId"] then
			return false
		end

		return a["display"] < b["display"]
	end)

	-- now sort all other friends
	table.sort(onlineFriendsData, function(a, b)
		return a["display"] < b["display"]
	end)

	-- now concat tables
	for i = 1, #onlineFriendsData do
		onlineRobloxFriendsData[#onlineRobloxFriendsData + 1] = onlineFriendsData[i]
	end

	return onlineRobloxFriendsData
end

local function filterFriends(friendsData)
	for i = 1, #friendsData do
		local data = friendsData[i]

		if data["PlaceId"] == 0 then
			data["PlaceId"] = nil
		end

		if data["LastLocation"] == "" then
			data["LastLocation"] = nil
		end

		local placeId = data["PlaceId"]
		local lastLocation = data["LastLocation"]

		-- If the lastLocation for a user is some user place with a GeneratedUsername in it
		-- then replace it with the actual creator name!
		if ConvertMyPlaceNameInXboxAppFlag and placeId and lastLocation and GameData:ExtractGeneratedUsername(lastLocation) then
			local gameCreator = GameData:GetGameCreatorAsync(placeId)
			if gameCreator then
				lastLocation = GameData:GetFilteredGameName(lastLocation, gameCreator)
			end
		end

		data["PlaceId"] = placeId
		data["LastLocation"] = lastLocation
	end

	return friendsData
end


--[[
		// Returns Array of xbox friends
		// Keys
			// xuid - string, xbox user id
			// gamertage - string, users gamertag
			// robloxuid - number, users roblox userId
			// status - string (Online, Away, Offline, Unknown)
			// rich - array of rich presence, can be empty
				// titleId - string, the titleId for the game/app they are using
				// title - string, name of game/app they are using
				// presence - string, rich presence string from that title
]]
local function fetchXboxFriendsAsync()
	local success, result = pcall(function()
		if PlatformService then
			if Utility.IsFastFlagEnabled("XboxPollUsingFriendService") then
				return FriendService:GetPlatformFriends()
			else
				return PlatformService:BeginFetchFriends2(Enum.UserInputType.Gamepad1)
			end
		end
	end)
	if success then
		return result
	else
		print("fetchXboxFriends failed because", result)
	end
end

local function sortFriendsData(tempFriendsData)
	table.sort(tempFriendsData, function(a, b)
		if a["PlaceId"] and b["PlaceId"] then
			return a["display"] < b["display"]
		end
		if a["PlaceId"] then
			return true
		end
		if b["PlaceId"] then
			return false
		end

		return a["display"] < b["display"]
	end)
end

local function uploadFriendsAnalytics(friendsData)
	local numPlaying = 0
	for xuid, data in pairs(friendsData) do
		if data["PlaceId"] then
			numPlaying = numPlaying + 1
		end
	end

	Analytics.UpdateHeartbeatObject({
		FriendsPlaying = numPlaying;
		FriendsOnline = #friendsData;
	});
end

local function getOnlineFriends()
	if UserSettings().GameSettings:InStudioMode() or game:GetService('UserInputService'):GetPlatform() == Enum.Platform.Windows then
-- Roblox Friends - leaving this in for testing purposes in studio
		local result = Http.GetOnlineFriendsAsync()
		if not result then
			-- TODO: Error code
			return nil
		end

		local myOnlineFriends = {}

		for i = 1, #result do
			local data = result[i]
			local friend = {
				Name = Players:GetNameFromUserIdAsync(data["VisitorId"]);
				UserId = data["VisitorId"];
				LastLocation = data["LastLocation"];
				PlaceId = data["PlaceId"];
				LocationType = data["LocationType"];
				GameId = data["GameId"];
			}
			table.insert(myOnlineFriends, friend)
		end

		local function sortFunc(a, b)
			if a.LocationType == b.LocationType then
				return a.Name:lower() < b.Name:lower()
			end
			return a.LocationType > b.LocationType
		end

		table.sort(myOnlineFriends, sortFunc)

		return myOnlineFriends
	elseif game:GetService('UserInputService'):GetPlatform() == Enum.Platform.XBoxOne then
-- Xbox Friends
		local myXboxFriends = fetchXboxFriendsAsync()
		local myOnlineFriends = {}
		if myXboxFriends then
			if Utility.IsFastFlagEnabled("XboxImpartFriendGameTitle") then
				myOnlineFriends = filterFriends(myXboxFriends)
				sortFriendsData(myOnlineFriends)
			else
				myOnlineFriends = filterXboxFriends(myXboxFriends)
			end
		end

if Utility.IsFastFlagEnabled("XboxFriendsAnalytics") then
		uploadFriendsAnalytics(myOnlineFriends)
end

		return myOnlineFriends
	end
end

local xuidToFriendsMap = {}
local cachedFriendsData = {}

local function rehashCachedFriendsData()
	cachedFriendsData = {}

	local tempFriendsData = {}
	for xuid, data in pairs(xuidToFriendsMap) do
		table.insert(tempFriendsData, data)
	end

	sortFriendsData(tempFriendsData)

	cachedFriendsData = tempFriendsData

if Utility.IsFastFlagEnabled("XboxFriendsAnalytics") then
	uploadFriendsAnalytics(cachedFriendsData)
end

end

local function onAddFriends(friendsDataList)
	for i=1,#friendsDataList do
		local data = friendsDataList[i]
		xuidToFriendsMap[data.xuid] = data
	end

	rehashCachedFriendsData()
	FriendsData.OnFriendsDataAdded:fire(friendsDataList)
end

local function onRemoveFriends(friendsDataList)
	for i=1,#friendsDataList do
		local data = friendsDataList[i]
		xuidToFriendsMap[data.xuid] = nil
	end

	rehashCachedFriendsData()
	FriendsData.OnFriendsDataRemoved:fire(friendsDataList)
end

local function updateCallback(info, friendsDataList)
	local dispatch = {
		add = onAddFriends;
		remove = onRemoveFriends;
	};

	dispatch[info.action](filterFriends(friendsDataList))
end

local receivingFriendsEvents = false
local function startFriendServiceCallback()
	if not receivingFriendsEvents then
		FriendService.FriendsEvent:connect(updateCallback)
		FriendService:StartReceivingPlatformFriendsEvents()
		receivingFriendsEvents = true
	end
end


--[[ Public API ]]--

FriendsData.OnFriendsDataUpdated = Utility.Signal()

local isFetchingFriends = false

function FriendsData.GetOnlineFriendsAsync()
if Utility.IsFastFlagEnabled("XboxFriendsEvents") then
	startFriendServiceCallback()
	return cachedFriendsData
else
	-- can only make one call into PlatformService:BeginFetchFriends() at a time
	while isFetchingFriends do
		wait()
	end
	-- we have current data, this will be updated when polling
	if myCurrentFriendsData then
		return myCurrentFriendsData
	end
	isFetchingFriends = true

	myCurrentFriendsData = getOnlineFriends()
	-- spawn polling on first request
	if not isOnlineFriendsPolling then
		FriendsData.BeginPolling()
	end

	isFetchingFriends = false

	return myCurrentFriendsData
end
end

FriendsData.OnFriendsDataAdded = Utility.Signal()
FriendsData.OnFriendsDataRemoved = Utility.Signal()

function FriendsData.ConnectAddEvent(cbFunc)
	startFriendServiceCallback()
	local cn = FriendsData.OnFriendsDataAdded:connect(cbFunc)
	table.insert(updateEventCns, cn)
end

function FriendsData.ConnectRemoveEvent(cbFunc)
	startFriendServiceCallback()
	local cn = FriendsData.OnFriendsDataRemoved:connect(cbFunc)
	table.insert(updateEventCns, cn)
end

function FriendsData.BeginPolling()
	if not isOnlineFriendsPolling then
		isOnlineFriendsPolling = true
		local requesterId = UserData:GetRbxUserId()
		spawn(function()
			wait(POLL_DELAY)
			while requesterId == UserData:GetRbxUserId() do
				myCurrentFriendsData = getOnlineFriends()
				FriendsData.OnFriendsDataUpdated:fire(myCurrentFriendsData)
				wait(POLL_DELAY)
			end
		end)
	end
end

-- we make connections through this function so we can clean them all up upon
-- clearing the friends data
function FriendsData.ConnectUpdateEvent(cbFunc)
	local cn = FriendsData.OnFriendsDataUpdated:connect(cbFunc)
	table.insert(updateEventCns, cn)
end

function FriendsData.Reset()
if Utility.IsFastFlagEnabled("XboxFriendsEvents") then
	xuidToFriendsMap = {}
	cachedFriendsData = {}
	receivingFriendsEvents = false
else
	isOnlineFriendsPolling = false
	myCurrentFriendsData = nil
	for index,cn in pairs(updateEventCns) do
		cn = Utility.DisconnectEvent(cn)
		updateEventCns[index] = nil
	end
	print('FriendsData: Cleared last users FriendsData')
end
end

return FriendsData
