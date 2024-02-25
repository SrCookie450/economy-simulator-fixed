
local CoreGui = Game:GetService("CoreGui")
local GuiRoot = CoreGui:FindFirstChild("RobloxGui")
local Modules = GuiRoot:FindFirstChild("Modules")
local ShellModules = Modules:FindFirstChild("Shell")
local PlatformService = nil
pcall(function() PlatformService = game:GetService('PlatformService') end)
local UserInputService = game:GetService('UserInputService')
local GuiService = game:GetService('GuiService')

local Strings = require(ShellModules:FindFirstChild('LocalizedStrings'))
local Utility = require(ShellModules:FindFirstChild('Utility'))

local SortData = require(ShellModules:FindFirstChild('SortData'))
local GamesScrollingGrid = require(ShellModules:FindFirstChild('GamesScrollingGrid'))
local GameCollection = require(ShellModules:FindFirstChild('GameCollection'))
local GameCarouselItem = require(ShellModules:FindFirstChild('GameCarouselItem'))
local GlobalSettings = require(ShellModules:FindFirstChild('GlobalSettings'))
local EventHub = require(ShellModules:FindFirstChild('EventHub'))

local LockedGameCarouselView = require(ShellModules:FindFirstChild('LockedGameCarouselView'))

local AchievementManager = require(ShellModules:FindFirstChild('AchievementManager'))


local createGamesView = function(viewGridConfig, onNewGameSelected)

	local viewGridContainer = GamesScrollingGrid(viewGridConfig)
	viewGridContainer.Container.Visible = false
	local this = {}

	local carouselItems = {}
	--The local savedCarouselItems which will always hold all the carouselItems even
	--there is no games in it
	local savedCarouselItems = {}

	local selectedCoreObjectChangedCn = nil
	local setLoadDebounce = false
	local inFocus = false
	local lastLoadTime = nil
	local sortCategories = nil
	local savedCarouselItem = nil

	local staticSelectionImage = Utility.Create'ImageLabel'
	{
		-- Values pulled from GuiService.cpp; see constructor
		-- these numbers will need to be updated if positions/sizes change
		Name = "StaticSelectionImage";
		Size = UDim2.new(0, 232 + 14, 0, 232 + 14);
		BackgroundTransparency = 1;
		ScaleType = Enum.ScaleType.Slice;
		SliceCenter = Rect.new(19, 19, 43, 43);
		Image = "rbxasset://textures/ui/SelectionBox.png";
		Selectable = false;
		ZIndex = 3;
	}

	local lockView = LockedGameCarouselView()
	lockView:SetSelectionImageObject(
		Utility.Create'ImageLabel'
		{
			BackgroundTransparency = 1;
		}
	)

	lockView:SetParent(nil)
	lockView:SetZIndex(2)

	local function GetSelectionRectangleFromFirstItem(firstItemContainer)
		return UDim2.new(
				firstItemContainer.Position.X.Scale, firstItemContainer.Position.X.Offset - 7,
				firstItemContainer.Position.Y.Scale, firstItemContainer.Position.Y.Offset + 50 - 7)
	end

	local function showStaticSelectionImage()
		local firstSortItem = #carouselItems > 0 and carouselItems[1] or nil
		if firstSortItem then
			local firstItemContainer = firstSortItem:GetContainer()
			local pos = GetSelectionRectangleFromFirstItem(firstItemContainer)
			staticSelectionImage.Position = pos
			staticSelectionImage.Size = UDim2.new(0, 232 + 14, 0, 232 + 14);
			staticSelectionImage.Parent = viewGridContainer.Container.Parent
		end
		lockView:SetParent(nil)
	end

	local function enterNormalSelectionMode()
		lockView:SetParent(nil)
		local firstSortItem = #carouselItems > 0 and carouselItems[1] or nil
		if firstSortItem then
			local firstItemContainer = firstSortItem:GetContainer()
			local pos = GetSelectionRectangleFromFirstItem(firstItemContainer)
			staticSelectionImage.Position = pos
			staticSelectionImage.Size = UDim2.new(0, 232 + 14, 0, 232 + 14);
		end
	end

	local function enterLockedRowSelectionMode()
		local firstSortItem = #carouselItems > 0 and carouselItems[1] or nil
		if firstSortItem then
			local firstItemView = firstSortItem:GetCarouselView()
			if firstItemView then
				local viewContainerSize = firstItemView:GetContainer().Size

				-- scaling comes from GameCarouselItem
				local lockViewSize = UDim2.new(0, viewContainerSize.X.Offset, 0, viewContainerSize.Y.Offset * 0.84)
				lockView:SetSize(lockViewSize)
				lockView:SetPosition(UDim2.new(0, 8, 0, 8))

				staticSelectionImage.Size = lockViewSize + UDim2.new(0, 16, 0, 16)

				local viewContainerPos = firstItemView:GetContainer().Position
				staticSelectionImage.Position = UDim2.new(
					viewContainerPos.X.Scale,
					viewContainerPos.X.Offset - 118,
					viewContainerPos.Y.Scale,
					viewContainerPos.Y.Offset + 10
				)
			end
		end
		lockView:SetParent(staticSelectionImage)
	end

	local function removeStaticSelectionImage()
		staticSelectionImage.Parent = nil
	end

	local fadableObjets = {}

	local function getItemByIndex(i)
		local item = carouselItems[i]
		return item and item:GetContainer()
	end

	local function getIndexByItem(item)
		for i = 1, #carouselItems do
			if item == carouselItems[i] then
				return i
			end
		end

		return 0
	end

	local function getMappedIndex(item)
		for i = 1, #savedCarouselItems do
			if item == savedCarouselItems[i] then
				return i
			end
		end

		return 0
	end

	local lastSelectedRow = 0
	local function GetSelectedRow()
		for i = 1, #carouselItems do
			local item = carouselItems[i]
			if item:IsSelected() then
				return i
			end
		end
		return 0
	end

	local function GetSelectedItem()
		for i = 1, #carouselItems do
			local item = carouselItems[i]
			if item:IsSelected() then
				return item
			end
		end
		return nil
	end

	local function onSelectedCoreObjectChanged()
		if setLoadDebounce then return end
		local currentSelectedRow = GetSelectedRow()
		if lastSelectedRow == currentSelectedRow then return end
		lastSelectedRow = currentSelectedRow
		local selectedItem = nil

		if currentSelectedRow == 0 then
			for i = 1, #carouselItems do
				local item = carouselItems[i]
				item:RemoveFocus()
				item:SetTransparency(0, 0, 0.5)
			end
		else
			for i = 1, #carouselItems do
				local item = carouselItems[i]
				local imageTransparency = 0
				local textTransparency = 0
				if item:IsSelected() then
					item:Focus()
					showStaticSelectionImage()
					selectedItem = item
				else
					if i < currentSelectedRow then
						-- Making these bigger than one causes the fadeout to happen before the carousel
						-- overlaps with the game info and preview-image above.  It looks better.
						imageTransparency = 4
						textTransparency = 4
					elseif i == currentSelectedRow then
						imageTransparency = 0
						textTransparency = 0
					else
						imageTransparency = 0.5
						textTransparency = 0.5
					end
					item:RemoveFocus()
				end

				item:SetTransparency(imageTransparency, textTransparency, nil)
			end
			if selectedItem then
				if selectedItem:IsLocked() then
					enterLockedRowSelectionMode()
				else
					enterNormalSelectionMode()
				end
			end
		end
	end

	local function UpdateTransparency()
		local currentSelectedRow = GetSelectedRow()
		if currentSelectedRow > 0 then
			for i = 1, #carouselItems do
				local item = carouselItems[i]
				local imageTransparency = 0
				local textTransparency = 0
				if item:IsSelected() then
					selectedItem = item
				else
					if i < currentSelectedRow then
						imageTransparency = 4
						textTransparency = 4
					elseif i == currentSelectedRow then
						imageTransparency = 0
						textTransparency = 0
					else
						imageTransparency = 0.5
						textTransparency = 0.5
					end
				end

				item:SetTransparency(imageTransparency, textTransparency, 0, true)
			end
		end
	end


	local function carouselItemsResultsUpdate(item)
		if not item then return end
		local lastItemIndex = getIndexByItem(item)
		if item:HasResults() then
			--Add a new row
			if lastItemIndex == 0 then
				local Inserted = false
				for i = 1, #carouselItems do
					if getMappedIndex(item) < getMappedIndex(carouselItems[i]) then
						table.insert(carouselItems, i, item)
						Inserted = true
						break
					end
				end
				if not Inserted then
					table.insert(carouselItems, item)
				end
				--Recalc when carouselItems changes
				viewGridContainer:Recalc()
				--Update SelectedRow num as the rowNumber may change
				lastSelectedRow = GetSelectedRow()
			end
		else
			--Remove a row
			if lastItemIndex > 0 then
				local removeItemIndex = getIndexByItem(item)
				item:RemoveFocus()
				--If was in the carouselItems, remove
				table.remove(carouselItems, lastItemIndex)
				viewGridContainer:Recalc()
				if inFocus then
					if not GetSelectedItem() then
						lastSelectedRow = removeItemIndex
						--The selected row get removed, select the same selected row or first row if doesn't exist
						if lastSelectedRow > 0 and lastSelectedRow <= #carouselItems then
							carouselItems[lastSelectedRow]:Focus()
						else
							viewGridContainer:BackToInitial()
							if #carouselItems > 0  then
								carouselItems[1]:Focus()
							end
						end
					end
				end
				lastSelectedRow = GetSelectedRow()
			end
		end

		UpdateTransparency()
	end

	local function createCarouselItem(name, getGameCollection)
		local item = GameCarouselItem(UDim2.new(0, 1900, 0, 250), name, getGameCollection, onNewGameSelected, carouselItemsResultsUpdate)
		table.insert(carouselItems, item)
		table.insert(savedCarouselItems, item)
		return item
	end

	local function createCarouselItems(sortCategories)
		local featuredSortId = 3
		for i = 1, #sortCategories do
			local sort = sortCategories[i]
			local sortCollection = function() return GameCollection:GetSort(sort["Id"]) end
			local item = createCarouselItem(sort["Name"], sortCollection)
			if item and sort["Id"] ~= featuredSortId then
				item:SetLockInPUP(true)
			end
		end

		createCarouselItem(Strings:LocalizedString("FavoritesSortTitle"), function() return GameCollection:GetUserFavorites() end)
		createCarouselItem(Strings:LocalizedString("RecentlyPlayedSortTitle"), function()  return GameCollection:GetUserRecent() end)
		createCarouselItem(Strings:LocalizedString("PlayMyPlaceMoreGamesTitle"), function() return GameCollection:GetUserPlaces() end)

		for i = 1, #savedCarouselItems do
			if savedCarouselItems[i] then
				savedCarouselItems[i]:Init()
			end
		end

		for i = 1, #savedCarouselItems do
			if not AchievementManager:AllGamesUnlocked() and savedCarouselItems[i]:GetLockInPUP() then
				savedCarouselItems[i]:Lock()
			else
				savedCarouselItems[i]:Unlock()
			end
		end

		viewGridContainer:SetItemCallback(getItemByIndex)
		viewGridContainer:Recalc()
	end


	local function LoadCategoryList()
		if setLoadDebounce then return end
		lastLoadTime = tick()
		setLoadDebounce = true
		viewGridContainer.Container.Visible = false

		spawn(function()
			if #savedCarouselItems > 0 or #carouselItems > 0 then
				--Remove all
				for i = 1, #carouselItems do
					if carouselItems[i] then
						carouselItems[i]:Destroy()
						carouselItems[i] = nil
					end
				end

				for i = 1, #savedCarouselItems do
					if savedCarouselItems[i] then
						savedCarouselItems[i]:Destroy()
						savedCarouselItems[i] = nil
					end
				end

				carouselItems = {}
				savedCarouselItems = {}
				viewGridContainer:Recalc()
			end

			if not sortCategories then
				sortCategories = SortData.GetSortCategoriesAsync()
			end

			while not sortCategories do
				wait()
			end

			createCarouselItems(sortCategories)

			setLoadDebounce = false
			if viewGridContainer:GetParent().Visible then
				viewGridContainer:BackToInitial()
				viewGridContainer.Container.Visible = true
				if inFocus then
					if #carouselItems > 0  then
						carouselItems[1]:Focus()
						UpdateTransparency()
					end
				end
			end
		end)
	end

	function this:GetDefaultSelection()
		for i = 1, #carouselItems do
			if carouselItems[i]:GetDefaultSelection() then
				return carouselItems[i]:GetDefaultSelection()
			end
		end
		return viewGridContainer.Container
	end

	local function onGridViewFocused()
		--Focus on viewGridContainer and the selected row
		viewGridContainer:Focus()
		if not setLoadDebounce then
			local selectedRow = 0
			if savedCarouselItem then
				selectedRow = getIndexByItem(savedCarouselItem)
				savedCarouselItem = nil
			else
				selectedRow = GetSelectedRow()
			end

			if selectedRow > 0 and selectedRow <= #carouselItems then
				carouselItems[selectedRow]:Focus()
			else
				viewGridContainer:BackToInitial()
				if #carouselItems > 0  then
					carouselItems[1]:Focus()
				end
			end
			UpdateTransparency()
		end
	end

	local function onGridViewFocusRemoved(notFromScreenManager)
		savedCarouselItem = GetSelectedItem()
		lastSelectedRow = 0

		--If the bumper/button B caused the removing focus, reset to the top of viewGridContainer
		if notFromScreenManager then
			viewGridContainer:BackToInitial(0.5)
			savedCarouselItem = nil
		end
		--Remove image and the focus on all inside elements
		removeStaticSelectionImage()
		viewGridContainer:RemoveFocus()
		for i = 1, #carouselItems do
			local item = carouselItems[i]
			item:RemoveFocus()
			if notFromScreenManager then
				item:SetTransparency(0, 0, 0.5)
			end
		end
	end

	function this:GetDefaultFocusItem()
		return nil
	end

	function this:SetParent(parent)
		viewGridContainer:SetParent(parent)
	end

	function this:Load()
		if not lastLoadTime then  --Init
			LoadCategoryList()
		else
			local now = tick()
			if now - lastLoadTime > GlobalSettings.GamesPaneRefreshInterval then
				LoadCategoryList()
			end
		end
	end

	function this:Focus()
		inFocus = true
		Utility.DisconnectEvent(selectedCoreObjectChangedCn)
		selectedCoreObjectChangedCn = GuiService.Changed:connect(function(prop)
			if prop == 'SelectedCoreObject' then
				onSelectedCoreObjectChanged()
			end
		end)
		onGridViewFocused()
	end

	function this:RemoveFocus(notFromScreenManager)
		inFocus = false
		Utility.DisconnectEvent(selectedCoreObjectChangedCn)
		onGridViewFocusRemoved(notFromScreenManager)
	end


	--Add listener to PlayedGamesChanged and FavoriteToggle to reload these two types timely
	EventHub:addEventListener(EventHub.Notifications["PlayedGamesChanged"], "GamesPaneRecentlyPlayedUpdate",
	function()
		spawn(function()
			local updateItemName = Strings:LocalizedString("RecentlyPlayedSortTitle")
			for i = 1, #savedCarouselItems do
				if updateItemName == savedCarouselItems[i]:GetSortName() then
					if savedCarouselItems[i] then
						savedCarouselItems[i]:Refresh()
						break
					end
				end
			end
		end)
	end)

	EventHub:addEventListener(EventHub.Notifications["FavoriteToggle"], "GamesPaneFavoritesUpdate",
	function(success)
		if success then
			spawn(function()
				local updateItemName = Strings:LocalizedString("FavoritesSortTitle")
				for i = 1, #savedCarouselItems do
					if updateItemName == savedCarouselItems[i]:GetSortName() then
						if savedCarouselItems[i] then
							savedCarouselItems[i]:Refresh()
							break
						end
					end
				end
			end)
		end
	end)

	EventHub:addEventListener(EventHub.Notifications["UnlockedUGC"], "GamesPaneUnlockedUGC",
	function()
		spawn(function()
			for i = 1, #savedCarouselItems do
				if savedCarouselItems[i] then
					savedCarouselItems[i]:Unlock()
				end
			end
		end)
	end)


	--[[ Initialize - Don't Block ]]--
	spawn(function()
		LoadCategoryList()
	end)


	return this
end

return createGamesView