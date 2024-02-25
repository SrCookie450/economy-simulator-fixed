--[[
				// GameCollection.lua

				// Used to get a collection of games
]]
local CoreGui = Game:GetService("CoreGui")
local GuiRoot = CoreGui:FindFirstChild("RobloxGui")
local Modules = GuiRoot:FindFirstChild("Modules")
local ShellModules = Modules:FindFirstChild("Shell")

local ThirdPartyUserService = nil
pcall(function() ThirdPartyUserService = game:GetService('ThirdPartyUserService') end)

local SortData = require(ShellModules:FindFirstChild('SortData'))
local UserData = require(ShellModules:FindFirstChild('UserData'))
local Utility = require(ShellModules:FindFirstChild('Utility'))
local Analytics = require(ShellModules:FindFirstChild('Analytics'))

local GameCollection = {}

GameCollection.DefaultSortId = {
	Popular = 1;
	Featured = 3;
	TopEarning = 8;
	TopRated = 11;
}

local function createBaseCollection()
	local this = {}

	function this:GetSortAsync(startIndex, pageSize)
		print("GameCollection GetSortAsync() must be implemented by sub class")
	end

	return this
end

local SortCollections = {}
function GameCollection:GetSort(sortId)
	if SortCollections[sortId] then
		return SortCollections[sortId]
	end

	local collection = createBaseCollection()

	-- Override
	function collection:GetSortAsync(startIndex, pageSize)
		local sort = SortData.GetSort(sortId)
		local timeFilter = nil
		-- top rated is time filtered to show most recent top rated
		if sortId == GameCollection.DefaultSortId.TopRated then
			timeFilter = 2
		end
		return sort:GetPageAsync(startIndex, pageSize, timeFilter)
	end

	SortCollections[sortId] = collection

	return collection
end

local UserFavoriteCollection = nil
function GameCollection:GetUserFavorites()
	if UserFavoriteCollection then
		return UserFavoriteCollection
	end

	UserFavoriteCollection = createBaseCollection()

	-- Override
	function UserFavoriteCollection:GetSortAsync(startIndex, pageSize)
		local sort = SortData.GetUserFavorites()
		return sort:GetPageAsync(startIndex, pageSize)
	end

	return UserFavoriteCollection
end

local UserRecentCollection = nil
function GameCollection:GetUserRecent()
	if UserRecentCollection then
		return UserRecentCollection
	end

	UserRecentCollection = createBaseCollection()

	-- Override
	function UserRecentCollection:GetSortAsync(startIndex, pageSize)
		local sort = SortData.GetUserRecent()
		return sort:GetPageAsync(startIndex, pageSize)
	end

	return UserRecentCollection
end

local UserPlacesCollection = nil
function GameCollection:GetUserPlaces()
	if UserPlacesCollection then
		return UserPlacesCollection
	end

	UserPlacesCollection = createBaseCollection()

	-- Override
	function UserPlacesCollection:GetSortAsync(startIndex, pageSize)
		local userId = UserData:GetRbxUserId()
		if userId then
			local sort = SortData.GetUserPlaces(userId)
			return sort:GetPageAsync(startIndex, pageSize)
		end
	end

	return UserPlacesCollection
end

function GameCollection:GetGameSearchCollection(searchWord)
	local collection = createBaseCollection()
	Analytics.SetRBXEventStream("GameSearch", {SearchWord = searchWord})
	-- Override
	function collection:GetSortAsync(startIndex, pageSize)
		local sort = SortData.GetGameSearch(searchWord)
		return sort:GetPageAsync(startIndex, pageSize)
	end

	return collection
end

if ThirdPartyUserService then
	ThirdPartyUserService.ActiveUserSignedOut:connect(function()
		SortCollections = {}
		UserFavoritCollection = nil
		UserRecentCollection = nil
		UserPlacesCollection = nil
	end)
end

return GameCollection
