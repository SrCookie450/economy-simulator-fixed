
local CoreGui = game:GetService("CoreGui")
local GuiRoot = CoreGui:FindFirstChild("RobloxGui")
local Modules = GuiRoot:FindFirstChild("Modules")
local ShellModules = Modules:FindFirstChild("Shell")


local GlobalSettings = require(ShellModules:FindFirstChild('GlobalSettings'))
local SoundManager = require(ShellModules:FindFirstChild('SoundManager'))
local Strings = require(ShellModules:FindFirstChild('LocalizedStrings'))
local Utility = require(ShellModules:FindFirstChild('Utility'))
local ScrollingGrid = require(ShellModules:FindFirstChild('ScrollingGrid'))

--CONSTANTS
local SPACING = 33

local function createSettingsScreenBase(controller)
	local this = {}

	local sortCatagories = nil
	local imageSize = UDim2.new(0, 298, 0, 298)
	local spacing = Vector2.new(14, 14)
	local rows, columns = 2, 2

	local noSelectionObject = Utility.Create'ImageLabel'
	{
		BackgroundTransparency = 1;
	}

	-- UI Elements
	local GamePaneContainer = Utility.Create'Frame'
	{
		Name = 'GamePane';
		Size = UDim2.new(1, 0, 1, 0);
		BackgroundTransparency = 1;
		Parent = parent;
		SelectionImageObject = noSelectionObject;
		Visible = false;
	}

	local gameSortsPane = ScrollingGrid()
	gameSortsPane:SetPosition(UDim2.new(0, 0, 0, 0))
	gameSortsPane:SetCellSize(Vector2.new(610, 646))
	gameSortsPane:SetSize(UDim2.new(1, GlobalSettings.TitleSafeInset.X.Offset, 1, 0))
	gameSortsPane:SetScrollDirection(gameSortsPane.Enum.ScrollDirection.Horizontal)
	gameSortsPane:SetParent(GamePaneContainer)
	gameSortsPane:SetClipping(false)
	gameSortsPane:SetSpacing(Vector2.new(SPACING, SPACING))
	gameSortsPane.Container.Visible = false



	end)

	local function createView(gameCollection, name)
		local sortPage = gameCollection:GetSortAsync(0, 5)

		if sortPage and sortPage.Count > 0 then
			local view = GameSort:CreateGridView(UDim2.new(), imageSize, spacing, rows, columns)
			local iconIds = sortPage:GetPageIconIds()
			local names = sortPage:GetPagePlaceNames()
			local placeIds = sortPage:GetPagePlaceIds()
			view:SetImages(iconIds)
			view:ConnectInput(placeIds, names, iconIds, name, gameCollection)
			view:SetTitle(name)
			return view
		end
		return nil
	end

	coroutine.resume(coroutine.create(function()
		local sortCategories = controller:GetSortCategoriesAsync()
		for _, collectionData in pairs(controller:GetSortCollectionData()) do
			createView(collectionData, sortCategories)
		end
		controller.SortCollectionAdded:connect(function(newSortData)

		end)
	end) )()

	function this:GetDefaultSelection()
		for i = 1, #views do
			if views[i]:GetDefaultSelection() then
				return views[i]:GetDefaultSelection()
			end
		end
		return GamePaneContainer
	end

	return this
end

return createSettingsScreenBase
