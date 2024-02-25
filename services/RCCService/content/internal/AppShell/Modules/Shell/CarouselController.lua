--[[
				// CarouselController.lua

				// Controls how the data is updated for a carousel view
]]
local CoreGui = Game:GetService("CoreGui")
local GuiRoot = CoreGui:FindFirstChild("RobloxGui")
local Modules = GuiRoot:FindFirstChild("Modules")
local ShellModules = Modules:FindFirstChild("Shell")

local ContextActionService = game:GetService('ContextActionService')
local GuiService = game:GetService('GuiService')
local UserInputService = game:GetService('UserInputService')

local EventHub = require(ShellModules:FindFirstChild('EventHub'))
local GlobalSettings = require(ShellModules:FindFirstChild('GlobalSettings'))
local SoundManager = require(ShellModules:FindFirstChild('SoundManager'))
local ThumbnailLoader = require(ShellModules:FindFirstChild('ThumbnailLoader'))
local Utility = require(ShellModules:FindFirstChild('Utility'))

local function createCarouselController(view, disableBumperBinds)
	disableBumperBinds = disableBumperBinds or false
	local this = {}

	local PAGE_SIZE = 25
	local MAX_PAGES = 2
	local LOAD_AMOUNT = 100

	local sortCollection = nil
	local sortData = nil
	local currentFocusData = nil
	local absoluteDataIndex = 1
	local guiServiceChangedCn = nil
	local prevButtonCn = nil
	local nextButtonCn = nil

	local loadBuffer = 10

	local pages = {}
	local pageViews = {}
	local isLoading = false
	local frontPageIndex = 0
	local endPageIndex = 0

	-- Events
	this.NewItemSelected = Utility.Signal()

	local function getNewItem(data)
		local item = Utility.Create'ImageButton'
		{
			Name = "CarouselViewImage";
			BackgroundColor3 = GlobalSettings.ModalBackgroundColor;
			BorderSizePixel = 0;
			SoundManager:CreateSound('MoveSelection');
		}

		local overrideSelection = view:GetSelectionImageObject()
		if overrideSelection then
			item.SelectionImageObject = overrideSelection
		end

		local thumbnailLoader = ThumbnailLoader:Create(item, data.IconId, ThumbnailLoader.Sizes.Medium, ThumbnailLoader.AssetType.Icon)
		spawn(function()
			thumbnailLoader:LoadAsync(true, false, nil)
		end)

		itemInfo = {
			item = item;
			thumbnailLoader = thumbnailLoader;
		}

		local transparencyTweens = {}

		function itemInfo:AnimateTransparency(initial, final, duration)
			Utility.CancelTweens(transparencyTweens)

			local finished = nil
			if initial < final then
				thumbnailLoader:SetTransparency(final)
			else
				finished = function()
					thumbnailLoader:SetTransparency(final)
				end
			end

			table.insert(transparencyTweens,
				Utility.PropertyTweener(
					item,
					'BackgroundTransparency',
					initial,
					final,
					duration,
					Utility.EaseOutQuad,
					true))

			table.insert(transparencyTweens,
				Utility.PropertyTweener(
					item,
					'ImageTransparency',
					initial,
					final,
					duration,
					Utility.EaseOutQuad,
					true,
					finished))
		end

		return itemInfo
	end

	-- TODO: Remove this when caching is finished. Doing it this way is going to leave the old
	-- data issues in place with the carousel
	local function setInternalData(page)
		if not page then
			return {}
		end

		local newData = {}

		local placeIds = page:GetPagePlaceIds()
		local names = page:GetPagePlaceNames()
		local voteData = page:GetPageVoteData()
		local iconIds = page:GetPageIconIds()
		local creatorNames = page:GetCreatorNames()
		local creatorUserIds = page:GetPageCreatorUserIds()

		for i = 1, #page.Data do
			local gameEntry = {
				Title = names[i];
				PlaceId = placeIds[i];
				IconId = iconIds[i];
				VoteData = voteData[i];
				CreatorName = creatorNames[i];
				CreatorUserId = creatorUserIds[i];
				-- Description and IsFavorites needs to be queried for each game when needed
				Description = nil;
				IsFavorited = nil;
				GameData = nil;
			}
			table.insert(newData, gameEntry)
		end

		return newData
	end

	local onViewFocusChanged;

	local function createPageView(page)
		local pageView = {}
		local viewItems = {}

		for i = 1, #page do
			local itemData = page[i]
			local viewItemInfo = getNewItem(itemData)
			viewItemInfo.item.MouseButton1Click:connect(function()
				-- If we are in VR or if we click on the focused game we should just launch details
				if UserInputService.VREnabled and absoluteDataIndex ~= view:GetItemIndex(viewItemInfo.item) then
					onViewFocusChanged(viewItemInfo.item)
				else
					if itemData then
						EventHub:dispatchEvent(EventHub.Notifications["OpenGameDetail"], itemData.PlaceId, itemData.Title, itemData.IconId, itemData.GameData)
					end
				end
			end)
			table.insert(viewItems, viewItemInfo)
		end

		function pageView:GetCount()
			return #viewItems
		end
		function pageView:GetItems()
			return viewItems
		end
		function pageView:Destroy()
			for i,itemInfo in pairs(viewItems) do
				viewItems[i] = nil
				itemInfo.item:Destroy()
			end
		end

		return pageView
	end

	local function getPageAsync(pageIndex)
		if pageIndex < 1 then
			return nil
		end

		if not pages[pageIndex] then
			local startIndex = (pageIndex - 1) * PAGE_SIZE

			local newPageData = sortCollection:GetSortAsync(startIndex, PAGE_SIZE)

			if not newPageData then
				return nil
			end

			if newPageData.Count > 0 then
				local newPage = setInternalData(newPageData)
				table.insert(pages, newPage)
			end
		end

		return pages[pageIndex]
	end

	-- this is a helper function to resolve an issue with the new infinite games grid. There is an issue where selecting
	-- off the end of a carousel view will try to select something in a carousel below.
	local function insertPageViewBack(pageView)
		-- clear out previous back item selection override
		local backItem = view:GetBack()
		if backItem then
			backItem.NextSelectionRight = nil
		end
		view:InsertCollectionBack(pageView:GetItems())

		-- set new back item selection right to itself to prevent selection on views below/
		backItem = view:GetBack()
		if backItem then
			backItem.NextSelectionRight = backItem
		end
	end

	local previousFocusItem = nil
	function onViewFocusChanged(newFocusItem)
		local offset = 0
		if previousFocusItem then
			offset = view:GetItemIndex(newFocusItem) - view:GetItemIndex(previousFocusItem)
		end

		local visibleItemCount = view:GetVisibleCount()
		local itemCount = view:GetCount()

		if offset > 0 then
			-- scrolled right
			local firstVisibleItemIndex = view:GetFirstVisibleItemIndex()
			if not isLoading and firstVisibleItemIndex + visibleItemCount + loadBuffer >= itemCount then
				isLoading = true
				spawn(function()
					local page = getPageAsync(endPageIndex + 1)
					if page then
						local newView = createPageView(page)
						endPageIndex = endPageIndex + 1
						insertPageViewBack(newView)
						table.insert(pageViews, newView)

						if view:GetCount() > PAGE_SIZE * MAX_PAGES then
							frontPageIndex = frontPageIndex + 1
							local frontPageView = table.remove(pageViews, 1)
							view:RemoveAmountFromFront(frontPageView:GetCount())
							frontPageView:Destroy()
						end
					end
					isLoading = false
				end)
			end
		elseif offset < 0 then
			-- scrolled left
			local lastVisibleItemIndex = view:GetLastVisibleItemIndex()
			if not isLoading and lastVisibleItemIndex - visibleItemCount - loadBuffer < 0 then
				isLoading = true
				spawn(function()
					local page = getPageAsync(frontPageIndex - 1)
					if page then
						local newView = createPageView(page)
						frontPageIndex = frontPageIndex - 1
						view:InsertCollectionFront(newView:GetItems())
						table.insert(pageViews, 1, newView)

						if view:GetCount() > PAGE_SIZE * MAX_PAGES then
							endPageIndex = endPageIndex - 1
							local endPageView = table.remove(pageViews)
							view:RemoveAmountFromBack(endPageView:GetCount())
							endPageView:Destroy()
						end
					end
					isLoading = false
				end)
			end
		end

		absoluteDataIndex = absoluteDataIndex + offset
		previousFocusItem = newFocusItem
		view:ChangeFocus(newFocusItem)

		local pageNumber = math.ceil(absoluteDataIndex/PAGE_SIZE)
		local pageDataIndex = absoluteDataIndex - ((pageNumber - 1) * PAGE_SIZE)
		currentFocusData = pages[pageNumber][pageDataIndex]
		this.NewItemSelected:fire(currentFocusData)
	end

	--[[ Public API ]]--
	function this:GetFrontGameData()
		if pages[1] then
			return pages[1][1]
		end
	end

	function this:GetCurrentFocusGameData()
		return currentFocusData
	end

	function this:SelectFront()
		local frontViewItem = view:GetFront()
		if frontViewItem then
			onViewFocusChanged(frontViewItem)
		end
	end

	function this:SetLoadBuffer(newValue)
		loadBuffer = newValue
	end

	function this:InitializeAsync(gameCollection)
		view:RemoveAllItems()
		frontPageIndex = 1
		endPageIndex = 0
		absoluteDataIndex = 1
		currentFocusData = nil
		pages = {}
		pageViews = {}
		sortCollection = gameCollection

		for i = 1, MAX_PAGES do
			local page = getPageAsync(i)
			if page then
				local newView = createPageView(page)
				endPageIndex = endPageIndex + 1
				insertPageViewBack(newView)
				table.insert(pageViews, newView)
			end
		end

		local frontViewItem = view:GetFront()
		if frontViewItem then
			previousFocusItem = frontViewItem
			view:SetFocus(frontViewItem)
		end
	end

	function this:Connect()
		guiServiceChangedCn = Utility.DisconnectEvent(guiServiceChangedCn)
		guiServiceChangedCn = GuiService.Changed:connect(function(property)
			if property ~= 'SelectedCoreObject' then
				return
			end
			if Utility.ShouldUseVRAppLobby() then
				return
			end

			local newSelection = GuiService.SelectedCoreObject
			if newSelection and view:ContainsItem(newSelection) then
				onViewFocusChanged(newSelection)
			end
		end)

		local function getItemNextShiftItem(direction)
            local currentFocusIndex = view:GetItemIndex(previousFocusItem)
            local shiftAmount = view:GetFullVisibleItemCount()
            local nextItem = view:GetItemAt(currentFocusIndex + shiftAmount * direction)
            if not nextItem then
                nextItem = direction == 1 and view:GetBack() or view:GetFront()
            end

            return nextItem
        end

		local function shiftRight()
			local nextItem = getItemNextShiftItem(1)
			if nextItem then
				if Utility.ShouldUseVRAppLobby() and UserInputService.VREnabled then
					onViewFocusChanged(nextItem)
				else
					GuiService.SelectedCoreObject = nextItem
				end
			end
		end

		local function shiftLeft()
			local nextItem = getItemNextShiftItem(-1)
			if nextItem then
				if Utility.ShouldUseVRAppLobby() and UserInputService.VREnabled then
					onViewFocusChanged(nextItem)
				else
					GuiService.SelectedCoreObject = nextItem
				end
			end
		end

		prevButtonCn = Utility.DisconnectEvent(prevButtonCn)
		nextButtonCn = Utility.DisconnectEvent(nextButtonCn)
		if view.PrevButton then
			prevButtonCn = view.PrevButton.MouseButton1Click:connect(shiftLeft)
		end
		if view.NextButton then
			nextButtonCn = view.NextButton.MouseButton1Click:connect(shiftRight)
		end

		local seenRightBumper = false
		local function onBumperRight(actionName, inputState, inputObject)
			if inputState == Enum.UserInputState.Begin then
				seenRightBumper = true
			elseif seenRightBumper and inputState == Enum.UserInputState.End then
				local currentSelection = GuiService.SelectedCoreObject
				if currentSelection and view:ContainsItem(currentSelection) then
					shiftRight()
				end
				seenRightBumper = false
			end
		end

		local seenLeftBumper = false
		local function onBumperLeft(actionName, inputState, inputObject)
			if inputState == Enum.UserInputState.Begin then
				seenLeftBumper = true
			elseif seenLeftBumper and inputState == Enum.UserInputState.End then
				local currentSelection = GuiService.SelectedCoreObject
				if currentSelection and view:ContainsItem(currentSelection) then
					shiftLeft()
				end
				seenRightBumper = false
			end
		end

		-- Bumper Binds
		if not disableBumperBinds then
			ContextActionService:UnbindCoreAction("BumperRight")
			ContextActionService:UnbindCoreAction("BumperLeft")
			ContextActionService:BindCoreAction("BumperRight", onBumperRight, false, Enum.KeyCode.ButtonR1)
			ContextActionService:BindCoreAction("BumperLeft", onBumperLeft, false, Enum.KeyCode.ButtonL1)
		end
	end

	function this:Disconnect()
		guiServiceChangedCn = Utility.DisconnectEvent(guiServiceChangedCn)
		prevButtonCn = Utility.DisconnectEvent(prevButtonCn)
		nextButtonCn = Utility.DisconnectEvent(nextButtonCn)
		if not disableBumperBinds then
			ContextActionService:UnbindCoreAction("BumperRight")
			ContextActionService:UnbindCoreAction("BumperLeft")
		end
	end

	function this:HasResults()
		return #pages > 0
	end

	return this
end

return createCarouselController
