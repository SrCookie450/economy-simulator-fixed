-- Written by Kip Turner, Copyright ROBLOX 2015

-- Achievement Manager

local PlatformService = nil
pcall(function() PlatformService = game:GetService('PlatformService') end)
local ThirdPartyUserService = nil
pcall(function() ThirdPartyUserService = game:GetService('ThirdPartyUserService') end)

local CoreGui = Game:GetService("CoreGui")
local GuiRoot = CoreGui:FindFirstChild("RobloxGui")
local Modules = GuiRoot:FindFirstChild("Modules")
local ShellModules = Modules:FindFirstChild("Shell")

local EventHub = require(ShellModules:FindFirstChild('EventHub'))
local Http = require(ShellModules:FindFirstChild('Http'))
local UserData = require(ShellModules:FindFirstChild('UserData'))
local PlatformInterface = require(ShellModules:FindFirstChild('PlatformInterface'))
local GameCollection = require(ShellModules:FindFirstChild('GameCollection'))
local Alerts = require(ShellModules:FindFirstChild('Alerts'))
local ErrorOverlay = require(ShellModules:FindFirstChild('ErrorOverlay'))
local ScreenManager = require(ShellModules:FindFirstChild('ScreenManager'))

--[[ ACHIEVEMENT NAMES --]]
	-- "Award10DayRoll"
	-- "Award20DayRoll"
	-- "Award3DayRoll"
	-- "AwardDeepDiver"
	-- "AwardFoursCompany"
	-- "AwardOneNameManyFaces"
	-- "AwardPollster"
	-- "AwardSampler"
	-- "AwardStrengthInNumbers"
	-- "AwardWorldTraveler"
	-- "AwardYouDidIt"
	-- "GameProgress"
	-- "MultiplayerRoundEnd"
	-- "MultiplayerRoundStart"
	-- "PlayerSessionEnd"
	-- "PlayerSessionPause"
	-- "PlayerSessionResume"
	-- "PlayerSessionStart"
	-- "Test_XPresses"
--[[ END OF ACHIEVEMENT NAMES --]]

local VIEW_GAMETYPE_ENUM =
{
	AppShell = 0;
	Game = 1;
}

local GAMES_FOR_YOU_DID_IT = 1
local GAMES_FOR_AWARD_SAMPLER = 5

local DAYS_FOR_3DAYROLL = 3
local DAYS_FOR_10DAYROLL = 10
local DAYS_FOR_20DAYROLL = 20

local GAMES_RATED_FOR_POLLSTER = 5

local PLAY_SECONDS_FOR_DEEP_DIVER = 60 * 60

local NUMBER_OF_FRIENDS_REQUIRED_FOR_FOURS_COMPANY = 3


local SECONDS_BETWEEN_FOURS_COMPANY_CHECKS = 30


local AchievementManager = {}

local CurrentView = VIEW_GAMETYPE_ENUM['AppShell']

AchievementManager.AchivementId = {
	Scout = "2";
	Explorer = "3";
	Trailblazer = "4";
	Pollster = "5";
	Marathon = "6";
	OneNameManyFaces = "7";
	ThreeDayRoll = "8";
	TenDayRoll = "9";
	TwentyDayRoll = "10";
	StrengthInNumbers = "11";
	FoursCompany = "12";
}
-- Map ID to trigger name so we can do a look up in SessionAchievementState
local AchievementIdToTrigger = {
	["2"] = "AwardYouDidIt";
	["3"] = "AwardSampler";
	["4"] = "AwardWorldTraveler";
	["5"] = "AwardPollster";
	["6"] = "AwardDeepDiver";
	["7"] = "AwardOneNameManyFaces";
	["8"] = "Award3DayRoll";
	["9"] = "Award10DayRoll";
	["10"] = "Award20DayRoll";
	["11"] = "AwardStrengthInNumbers";
	["12"] = "AwardFoursCompany";
}

local SessionAchievementState = {}

local function GetTotalNumberOfGamesOnXbox()
	-- TODO: is there a programmatic way of figuring this out?
	-- local SortData = require(SortDataModule)
	-- local recentlyPlayedSortData = SortData.GetSort(1, false)
	return 15
end

local function FilterInGameFriends(onlineFriends, playersInGame)
	local result = {}

	if onlineFriends and playersInGame then
		-- Create reverse lookup for speed
		local playersInGameReverseLookup = {}
		for _, playerInGame in pairs(playersInGame) do
			-- TODO: Figure out what the actual lookup
			if playerInGame['robloxuid'] then
				playersInGameReverseLookup[playerInGame['robloxuid']] = true
			end
		end

		for _, friend in pairs(onlineFriends) do
			if playersInGameReverseLookup[friend['robloxuid']] then
				table.insert(result, friend)
			end
		end
	end

	return result
end


local function OnPlayedGamesChanged()
	EventHub:dispatchEvent(EventHub.Notifications["PlayedGamesChanged"])
	spawn(function()
		local myUserId = UserData:GetRbxUserId()
		if myUserId then
			local recentCollection = GameCollection:GetUserRecent()
			-- TODO: is this the right way of getting num of played games?
			local recentlyPage1 = recentCollection and recentCollection:GetSortAsync(0, GetTotalNumberOfGamesOnXbox())
			local gamesPlayed = recentlyPage1 and #recentlyPage1:GetPagePlaceIds() or 0
			print("You have played:" , gamesPlayed , "games" )
			if gamesPlayed >= GAMES_FOR_YOU_DID_IT then
				AchievementManager:SendAchievementEventAsync("AwardYouDidIt")
			end
			if gamesPlayed >= GAMES_FOR_AWARD_SAMPLER then
				local hasExplorerAchievement = AchievementManager:HasAchievementAsync(AchievementManager.AchivementId.Explorer)
				AchievementManager:SendAchievementEventAsync("AwardSampler")

				-- if we didn't have it before then let's unlock UGC
				if not hasExplorerAchievement then
					EventHub:dispatchEvent(EventHub.Notifications["UnlockedUGC"])
				end
			end
			if gamesPlayed >= GetTotalNumberOfGamesOnXbox() then
				AchievementManager:SendAchievementEventAsync("AwardWorldTraveler")
			end
		end
	end)
end

local function OnJoinedGame()
	spawn(function()
		print("OnJoinGame: AwardStrengthInNumbers check")
		local partyMembers = PlatformInterface:GetPartyMembersAsync()
		if partyMembers then
			if PlatformInterface:IsInAParty(partyMembers) then
				AchievementManager:SendAchievementEventAsync("AwardStrengthInNumbers")
			end
		end
	end)
	spawn(function()
		print("OnJoinGame: Fours Company check")

		if PlatformService then
			local lastCheck = 0
			while CurrentView == VIEW_GAMETYPE_ENUM['Game'] do
				local now = tick()
				if now - lastCheck > SECONDS_BETWEEN_FOURS_COMPANY_CHECKS then

					local friendsData = require(ShellModules:FindFirstChild('FriendsData'))
					local onlineFriends = friendsData.GetOnlineFriendsAsync()

					-- TODO: add actually API
					local inGamePlayers = PlatformService:GetInGamePlayers()
					if inGamePlayers and onlineFriends then
						local inGameFriends = FilterInGameFriends(onlineFriends, inGamePlayers)

						if #inGameFriends >= NUMBER_OF_FRIENDS_REQUIRED_FOR_FOURS_COMPANY then
							AchievementManager:SendAchievementEventAsync("AwardFoursCompany")
							return
						end

					end
					lastCheck = now
				end
				wait(1)
			end
		end
	end)
	spawn(function()
		local startTime = tick()
		while tick() - startTime < PLAY_SECONDS_FOR_DEEP_DIVER do
			if CurrentView ~= VIEW_GAMETYPE_ENUM['Game'] then
				return
			end
			wait(1)
		end
		if CurrentView == VIEW_GAMETYPE_ENUM['Game'] then
			AchievementManager:SendAchievementEventAsync("AwardDeepDiver")
		end
	end)
end

function AchievementManager:SendAchievementEventAsync(achievementName)
	print("Achievement Manager - Awarding achievement:" , achievementName)
	local achievementStatus = nil
	local success, msg = pcall(function()
		-- NOTE: Yielding function
		if not UserSettings().GameSettings:InStudioMode() or game:GetService('UserInputService'):GetPlatform() == Enum.Platform.Windows then
			achievementStatus = PlatformService:BeginAwardAchievement(achievementName)
		end
	end)
	if not success then
		-- NOTE: very likely this function ever throws an error but returns error codes
		print("Achievement Manager - Unable to award achievement:" , achievementName , "for reason:" , msg)
	end

	-- Always set session state to awarded, we don't want to block session related achievement progress (UGC for example)
	-- see: https://forums.xboxlive.com/articles/56661/achievements-and-when-they-arent-unlocking-1.html
	-- there is an issue with xbox live granting achievements
	SessionAchievementState[achievementName] = true

	print("Achievement Manager - Achievement:" , achievementName , "event status:" , achievementStatus)
end

EventHub:addEventListener(EventHub.Notifications["UnlockedUGC"], "ShowUnlockedUGCOverlay",
	function()
		ScreenManager:OpenScreen(ErrorOverlay(Alerts.UnlockedUGC), false)
	end)


EventHub:addEventListener(EventHub.Notifications["TestXButtonPressed"], "AchievementManager",
	function()
		-- AchievementManager:SendAchievementEventAsync("Test_XPresses")
	end)

EventHub:addEventListener(EventHub.Notifications["AuthenticationSuccess"], "AchievementManager",
	function()
		spawn(function()
			local myUserId = UserData:GetRbxUserId()
			local function stillLoggedIn()
				local newUserId = UserData:GetRbxUserId()
				return newUserId ~= nil and myUserId == newUserId
			end

			if myUserId ~= nil then
				local loggedInResult = Http.GetConsecutiveDaysLoggedInAsync()
				local daysLoggedIn = loggedInResult and loggedInResult['count']
				-- local Utility = require(ShellModules:FindFirstChild('Utility'))
				-- print("AchievementManager - Checking number of days Logged in:" , Utility.PrettyPrint(loggedInResult))
				if daysLoggedIn then
					if daysLoggedIn >= DAYS_FOR_3DAYROLL and stillLoggedIn() then
						AchievementManager:SendAchievementEventAsync("Award3DayRoll")
					end
					if daysLoggedIn >= DAYS_FOR_10DAYROLL and stillLoggedIn() then
						AchievementManager:SendAchievementEventAsync("Award10DayRoll")
					end
					if daysLoggedIn >= DAYS_FOR_20DAYROLL and stillLoggedIn() then
						AchievementManager:SendAchievementEventAsync("Award20DayRoll")
					end
				end
				OnPlayedGamesChanged()
			end
		end)
	end)

EventHub:addEventListener(EventHub.Notifications["DonnedDifferentPackage"], "AchievementManager",
	function(assetId)
		AchievementManager:SendAchievementEventAsync("AwardOneNameManyFaces")
	end)

EventHub:addEventListener(EventHub.Notifications["VotedOnPlace"], "AchievementManager",
	function()
		spawn(function()
			local voteCount = UserData:GetVoteCount()
			print("Vote Check: with vote count" , voteCount)
			if voteCount >= GAMES_RATED_FOR_POLLSTER then
				AchievementManager:SendAchievementEventAsync("AwardPollster")
			end
		end)
	end)

if PlatformService then
	PlatformService.ViewChanged:connect(function(newView)
		print("ViewChanged:" , newView)
		CurrentView = newView
		if newView == VIEW_GAMETYPE_ENUM['AppShell'] then
			print("New view is appshell")
			OnPlayedGamesChanged()
		elseif newView == VIEW_GAMETYPE_ENUM['Game'] then
			print("New view is game")
			OnJoinedGame()
		end
	end)
end

if ThirdPartyUserService then
	ThirdPartyUserService.ActiveUserSignedOut:connect(function()
		SessionAchievementState = {}
	end)
end

function AchievementManager:HasAchievementAsync(achievementId)
	if UserSettings().GameSettings:InStudioMode() or game:GetService('UserInputService'):GetPlatform() == Enum.Platform.Windows then
		return true
	end

	if SessionAchievementState[AchievementIdToTrigger[achievementId]] == true then
		return true
	end

	local success, result = pcall(function()
		return PlatformService:BeginHasAchievement(achievementId)
	end)
	if not success then
		return false
	end

	--[[
		If the result is false or the API fails, we need to use our session state as backup. There is an edge case where locally we
		grant the achievement, but xbox does not update/grant the achievement for some period of time (like xbox services being down).
		We want to avoid ever locking the user from UGC if they have in fact unlocked it.
		Two other cases remain. During the retro check, if xbox services are down, but we have in fact unlocked UGC because we've played
		5 games, we still unlock. The user will get the notification again however.
		If the user unlocks UGC then unlinks their account and creates a new Roblox account, and if xbox services are down, since we
		check games played, UGC will be locked for them. It will remain locked until they play 5 games on the new account, or until
		xbox services come back up and we can correctly get the state of the achievement. Dan says this is both of these are OK.
	]]
	return result or SessionAchievementState[AchievementIdToTrigger[achievementId]]
end

function AchievementManager:AllGamesUnlocked()
	return AchievementManager:HasAchievementAsync(AchievementManager.AchivementId.Explorer)
end

return AchievementManager
