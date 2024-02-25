
local CoreGui = Game:GetService("CoreGui")
local GuiService = Game:GetService('GuiService')
local GuiRoot = CoreGui:FindFirstChild("RobloxGui")
local Modules = GuiRoot:FindFirstChild("Modules")
local ShellModules = Modules:FindFirstChild("Shell")

local GlobalSettings = require(ShellModules:FindFirstChild('GlobalSettings'))
local Utility = require(ShellModules:FindFirstChild('Utility'))
local LoadingWidget = require(ShellModules:FindFirstChild('LoadingWidget'))

local CarouselView = require(ShellModules:FindFirstChild('CarouselView'))
local CarouselController = require(ShellModules:FindFirstChild('CarouselController'))


local function GameCarouselItem(size, sortName, getGameCollection, onNewGameSelected, hasResultsChanged)
	local this = {}

	local myCarouselController = nil
	local controllerMutex = false
	local myCarouselView = nil
	local TEXT_OFFSET = 112
	local CAROUSEL_OFFSET = 110
	local hasResults = true
	local lockInPUP = false
	--Initially is unlocked
	local locked = false
	local currentLoader = nil

	local noSelectionObject = Utility.Create'ImageLabel'
	{
		Name = 'NoSelectionObject';
		BackgroundTransparency = 1;
	}

	local container = Utility.Create'ImageButton'
	{
		Name = sortName.."ImageButton";
		Size = size;
		Position = UDim2.new(0, 0, 0, 0);
		BorderSizePixel = 0;
		BackgroundTransparency = 1;
		ClipsDescendants = false;
		AutoButtonColor = false;
		SelectionImageObject = noSelectionObject;
		Selectable = false;
	}

	local selectionHolder = Utility.Create'ImageLabel'
	{
		Name = sortName.."SelectionHolder";
		BackgroundTransparency = 1;
		Size = UDim2.new(0, 400, 1, 0);
		Position = UDim2.new(0, 50, 0, 50);
		SelectionImageObject = noSelectionObject;
		Selectable = true;
		Parent = container;
	}

	selectionHolder.NextSelectionLeft = selectionHolder
	selectionHolder.NextSelectionRight = selectionHolder

	local nameLabel = Utility.Create'TextLabel'
	{
		Name = "NameLabel";
		Text = "Loading...";
		Size = UDim2.new(0, 0, 0, 0);
		Position = UDim2.new(0, TEXT_OFFSET, 0, 22);
		TextXAlignment = Enum.TextXAlignment.Left;
		TextColor3 = GlobalSettings.WhiteTextColor;
		Font = GlobalSettings.RegularFont;
		FontSize = Enum.FontSize.Size36; -- GlobalSettings.TitleSize;
		BackgroundTransparency = 1;
		Selectable = false;
		Parent = container;
	}

	local function loadGameCollection(carouselController, carouselView, container)
		local loaderImgTransparency = 0
		if currentLoader then
			loaderImgTransparency = Utility.Clamp(0, 1, currentLoader:GetTransparency())
			currentLoader:Cleanup()
		end
		nameLabel.Text = "Loading..."
		selectionHolder.Selectable = true

		spawn(function()
			local loader = LoadingWidget(
				{ Parent = container; Position = UDim2.new(0.5, CAROUSEL_OFFSET, 0.5, 50); ImageTransparency = loaderImgTransparency },
				{
					function()
						--The InitializeAsync will also remove old items if exist
						carouselController:InitializeAsync(getGameCollection())
						--The Connect will also remove old connections if exist
						carouselController:Connect()
						carouselController.NewItemSelected:connect(onNewGameSelected)

						--Make sure this thread is the latest one, if so,
						--make the callback and update selection
						if this and myCarouselController == carouselController and myCarouselView == carouselView then
							selectionHolder.Selectable = false
							hasResults = carouselController:HasResults()
							hasResultsChanged(this)
							if hasResults then
								nameLabel.Text = sortName
								if this:IsFocused() then
									carouselView:Focus()
									Utility.SetSelectedCoreObject(carouselView:GetAvailableItem())
								end
							else
								nameLabel.Text = ""
							end
						else
							if carouselView then
								carouselView:SetParent(nil)
							end
						end
					end
				}
			)

			if currentLoader then
				currentLoader:SetParent(nil)
			end

			currentLoader = loader
			loader:AwaitFinished()
			loader:Cleanup()
		end)
	end

	function this:Init(Transparency)
		myCarouselView = CarouselView()
		Transparency = Transparency and Transparency or 0
		myCarouselView:SetTransparency(Transparency)
		myCarouselView:SetSize(UDim2.new(0, 1700, 0, 232))
		myCarouselView:SetPosition(UDim2.new(0, CAROUSEL_OFFSET, 0, 50))
		myCarouselView:SetPadding(20)
		myCarouselView:SetItemSizePercentOfContainer(0.84)
		myCarouselView:SetSelectionImageObject(
			Utility.Create'ImageLabel'
			{
				BackgroundTransparency = 1;
			}
		)
		myCarouselView:SetParent(container)
		myCarouselController = CarouselController(myCarouselView, true)
		myCarouselController:SetLoadBuffer(15)
		loadGameCollection(myCarouselController, myCarouselView, container)
	end

	function this:Refresh()
		if myCarouselView then
			--Reset the old CarouselView
			myCarouselView:RemoveFocus()
			myCarouselView:RemoveAllItems()
			myCarouselView:SetParent(nil)
			if this:IsFocused() then
				Utility.SetSelectedCoreObject(selectionHolder)
				--Clear game data
				onNewGameSelected(nil)
			end
			--Init with previous Transparency
			this:Init(myCarouselView:GetTransparency())
		end
	end

	function this:HasResults()
		return hasResults
	end

	function this:GetContainer()
		return container
	end

	function this:ContainsItem(item)
		return myCarouselView:ContainsItem(item)
	end

	function this:GetCarouselView()
		return myCarouselView
	end

	function this:GetSortName()
		return sortName
	end

	function this:GetNameText()
		return nameLabel.Text
	end

	function this:SetNameText(name)
		nameLabel.Text = name
	end

	local inFocus = false;

	function this:IsFocused()
		return inFocus
	end

	-- Important subtle detail: the center of this needs to be in about the same
	-- x-coordinate as the first item in each carousel so that selection doesn't
	-- move laterally when you arrow up and down.
	local LockOverlay = Utility.Create'ImageLabel'
	{
		Name = "LockOverlay";
		Size = UDim2.new(0, 400, 1, 0);
		Position = UDim2.new(0, 50, 0, 50);
		BackgroundTransparency = 1;
		Selectable = true;
		SelectionImageObject = Utility.Create'ImageLabel'
		{
			BackgroundTransparency = 1;
		};
		ZIndex = 3;
	}

	LockOverlay.NextSelectionRight = LockOverlay
	LockOverlay.NextSelectionLeft = LockOverlay

	function this:Lock()
		locked = true
		LockOverlay.Parent = container;
		myCarouselView:SetSelectable(false)
		myCarouselView:SetClipsDescendants(true)
	end

	function this:Unlock()
		locked = false
		LockOverlay.Parent = nil;
		myCarouselView:SetSelectable(true)
		myCarouselView:SetClipsDescendants(false)
	end

	function this:IsLocked()
		return locked
	end

	function this:SetLockInPUP(State)
		lockInPUP = State
	end

	function this:GetLockInPUP()
		return lockInPUP
	end

	function this:Focus()
		if inFocus then return end
		if locked then
			onNewGameSelected(myCarouselController:GetFrontGameData(), true)
			Utility.SetSelectedCoreObject(LockOverlay)
			return
		end

		inFocus = true
		myCarouselView:Focus()
		if myCarouselView:GetAvailableItem() then
			Utility.SetSelectedCoreObject(myCarouselView:GetAvailableItem())
		else
			Utility.SetSelectedCoreObject(selectionHolder)
			--Clear game data
			onNewGameSelected(nil)
		end
	end

	function this:GetDefaultSelection()
		return myCarouselView:GetAvailableItem()
	end

	function this:RemoveFocus()
		if not inFocus then return end

		inFocus = false
		myCarouselView:RemoveFocus()
	end


	local fadeDuration = 0.2
	local targetTextTransparency = 0
	local textTransparencyTweens = {}

	local function setTextTransparency(value, duration, refresh)
		if not refresh and value == targetTextTransparency then return end

		if duration then
			targetTextTransparency = Utility.Clamp(0, 1, targetTextTransparency)
			if not refresh and value == targetTextTransparency then return end
		else
			duration = fadeDuration
		end

		Utility.CancelTweens(textTransparencyTweens)

		table.insert(textTransparencyTweens,
			Utility.PropertyTweener(
				nameLabel,
				'TextTransparency',
				targetTextTransparency,
				value,
				duration,
				Utility.EaseOutQuad,
				true))

		targetTextTransparency = value
	end

	function this:SetTransparency(imageTransparency, textTransparency, duration, refresh)
		setTextTransparency(textTransparency, duration, refresh)
		myCarouselView:SetTransparency(imageTransparency, duration, refresh)
		if currentLoader then
			currentLoader:SetTransparency(Utility.Clamp(0, 1, imageTransparency))
		end
	end

	function this:Destroy()
		container:Destroy()
		this = nil
	end

	function this:IsSelected()
		if GuiService.SelectedCoreObject then
			return GuiService.SelectedCoreObject == selectionHolder or myCarouselView:ContainsItem(GuiService.SelectedCoreObject) or
				GuiService.SelectedCoreObject == LockOverlay
		end

		return false
	end

	function this:handleSelectionChanged()
		if myCarouselController:HasResults() then
			if self:IsSelected() then
				myCarouselView:Focus()
				Utility.SetSelectedCoreObject(myCarouselView:GetAvailableItem())
			else
				myCarouselView:RemoveFocus()
			end
		end
	end

	return this
end

return GameCarouselItem
