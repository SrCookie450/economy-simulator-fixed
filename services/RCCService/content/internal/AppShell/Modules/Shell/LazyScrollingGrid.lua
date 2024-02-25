
local CoreGui = Game:GetService("CoreGui")
local GuiRoot = CoreGui:FindFirstChild("RobloxGui")
local Modules = GuiRoot:FindFirstChild("Modules")
local ShellModules = Modules:FindFirstChild("Shell")

local Utility = require(ShellModules:FindFirstChild('Utility'))
local Analytics = require(ShellModules:FindFirstChild('Analytics'))

local GuiService = game:GetService('GuiService')


local function LazyScrollingGrid(config)

	local this = {}

	this.Enum =
	{
		ScrollDirection = {["Vertical"] = 1; ["Horizontal"] = 2;};
	}

	this.CellSize = config.CellSize or Vector2.new(446, 114)
	this.Spacing = config.Spacing or Vector2.new(50, 10)
	this.Padding = config.Padding or Vector2.new(0, 0)
	this.ScrollDirection = (config.ScrollDirection and this.Enum.ScrollDirection[config.ScrollDirection]) or self.Enum.ScrollDirection.Vertical

	local ContainerDefaults =
	{
		Size = UDim2.new(1, 0, 1, 0);
		Position = UDim2.new(0, 0, 0, 0);
		Name = "Container";
		BackgroundTransparency = 1;
		ClipsDescendants = true;
		Visible = true;
		ScrollingEnabled = false;
		ScrollBarThickness = 0;
		Selectable = false;
	}

	local container = Utility.Create'ScrollingFrame'(ContainerDefaults)
	this.Container = container

	for name, value in pairs(config) do
		if ContainerDefaults[name] ~= nil then
			this.Container[name] = value
		end
	end

	this.Container.CanvasSize = UDim2.new(1, 2147483647, 1, 2147483647)
	--Set canvas size to default before setting the changed-callback, lest it get called right after
	this.Container.Changed:connect(function(prop)
		if prop == 'AbsoluteSize' then
			this:Recalc()
		end
	end)


	this.GridItems = {}
	this.ItemSet = {}
	this.currentTargetCanvasPosition = Vector2.new(0, 0);

	function this:GetPadding()
		return self.Padding
	end

	function this:GetSpacing()
		return self.Spacing
	end

	function this:GetCellSize()
		return self.CellSize
	end

	function this:GetScrollDirection()
		return self.ScrollDirection
	end

	function this:GetClipping()
		return self.Container.ClipsDescendants
	end

	function this:GetVisible()
		return self.Container.Visible
	end

	function this:SetVisible(isVisible)
		self.Container.Visible = isVisible;
	end

	function this:GetSize()
		return self.Container.Size
	end

	function this:GetPosition()
		return self.Container.Position
	end

	function this:SetItemCallback(callback)
		self.getIndexedItem = callback;
	end

	function this:GetParent()
		return self.Container.Parent
	end

	function this:SetParent(parent)
		self.Container.Parent = parent
	end

	function this:SetScrollViewParent(parent)
		scrollView.Parent = parent
	end

	function this:GetGuiObject()
		return self.Container
	end

	function this:ContainsItem(gridItem)
		return self.ItemSet[gridItem] ~= nil
	end

	function this:GetTopLevelWidth()
		return self.Container.AbsoluteSize.X
	end

	function this:GetTopLevelHeight()
		return self.Container.AbsoluteSize.Y
	end

	function this:ComputeActiveCellIndices(x, y, windowWidth, windowHeight)
		local cellSize = self:GetCellSize()
		local spacing = self:GetSpacing()
		local padding = self:GetPadding()

		local cellWidth = (spacing.X + cellSize.X)
		local cellHeight = (spacing.Y + cellSize.Y)

		local left = math.floor( x / cellWidth ) + 1
		local right = math.ceil( (x + windowWidth) / cellWidth )

		local top = math.floor( y / cellHeight ) + 1
		local bottom = math.ceil( (y + windowHeight) / cellHeight )

		local firstIndex = self:GetIndexFrom2D(top, left)
		local lastIndex = self:GetIndexFrom2D(bottom, right)

		local rows, columns = self:GetNumRowsColumns()

		-- add a row/column of padding on either side
		if self.ScrollDirection == self.Enum.ScrollDirection.Vertical then
			lastIndex = lastIndex + columns
			firstIndex = firstIndex - columns
		else
			lastIndex = lastIndex + rows
			firstIndex = firstIndex - rows
		end

		-- clamp to 1...
		if firstIndex < 1 then firstIndex = 1 end
		if lastIndex < 1 then lastIndex = 1 end

		return firstIndex, lastIndex
	end

	function this:GetCellIndicesFromScreenPosition(x, y)
		local cellSize = self:GetCellSize()
		local spacing = self:GetSpacing()
		local padding = self:GetPadding()

		local cellWidth = (spacing.X + cellSize.X)
		local cellHeight = (spacing.Y + cellSize.Y)

		return math.floor(y / cellHeight) + 1, math.floor(x / cellWidth) + 1
	end

	function this:Add(index, gridItem)
		self.GridItems[index] = gridItem
		gridItem.Parent = self.Container
		self.ItemSet[gridItem] = true
	end

	function this:Remove(index)
		local gridItem = self.GridItems[index]
		self.GridItems[index] = nil
		if gridItem then
			gridItem.Parent = nil
			if GuiService.SelectedCoreObject == gridItem then
				Utility.SetSelectedCoreObject(nil)
			end
			self.ItemSet[gridItem] = nil
		end
	end

	function this:Rewindow(x, y, windowWidth, windowHeight)
		local removeMe = {}
		local moveMe = {}

		for index, gridItem in pairs(self.GridItems) do
			removeMe[gridItem] = index
		end

		local addMe = {}
		local firstIndex, lastIndex = self:ComputeActiveCellIndices(x, y, windowWidth, windowHeight)
		for index = firstIndex, lastIndex do
			local gridItem = self.getIndexedItem and self.getIndexedItem(index)
			if gridItem then
				local prevIndex = removeMe[gridItem]
				if prevIndex then   --It's already there.
					removeMe[gridItem] = nil
					if prevIndex ~= index then
						moveMe[prevIndex] = index
					end
				else
					addMe[gridItem] = index
				end
			end
		end

		for gridItem, index in pairs(removeMe) do
			self:Remove(index)
		end

		for fromIndex, toIndex in pairs(moveMe) do
			local gridItem = self.GridItems[fromIndex]
			addMe[gridItem] = toIndex
			self.GridItems[fromIndex] = nil
		end

		for gridItem, index in pairs(addMe) do
			self:Add(index, gridItem)
		end

		for index, gridItem in pairs(self.GridItems) do
			local row, column = self:Get2DGridIndex(index)
			gridItem.Position = self:GetGridPosition(row, column)
		end
	end


	function this:Recalc()
		local selectedObjectBefore = self:FindAncestorGridItem(GuiService.SelectedCoreObject)
		local wasSelectedBefore =
			GuiService.SelectedCoreObject == this.Container or
			(selectedObjectBefore and self:ContainsItem(selectedObjectBefore))

		self:Rewindow(
			self.currentTargetCanvasPosition.x, self.currentTargetCanvasPosition.y,
			self:GetTopLevelWidth(), self:GetTopLevelHeight())

		local selectedObject = self:FindAncestorGridItem(GuiService.SelectedCoreObject)

		if selectedObject and self:ContainsItem(selectedObject) then
			local thisPos = self:GetCanvasPositionForOffscreenItem(selectedObject)
			if thisPos then
				Utility.PropertyTweener(self.Container, 'CanvasPosition', thisPos, thisPos, 0.0, Utility.EaseOutQuad, true,
				function()
					self:Rewindow(
						self.currentTargetCanvasPosition.x, self.currentTargetCanvasPosition.y,
						self:GetTopLevelWidth(), self:GetTopLevelHeight())
				end)
				self.currentTargetCanvasPosition = thisPos
			end
		elseif wasSelectedBefore then
			self:SelectAvailableItem()
		end
	end


	function this:Get2DGridIndex(index)
		-- 0 base index
		local zerobasedIndex = index - 1
		local rows, columns = self:GetNumRowsColumns()
		local row, column;

		if self.ScrollDirection == self.Enum.ScrollDirection.Vertical then
			row = math.floor(zerobasedIndex / columns)
			column = zerobasedIndex % columns
		else
			column = math.floor(zerobasedIndex / rows)
			row = zerobasedIndex % rows
		end

		return row, column
	end

	function this:GetIndexFrom2D(row, column)
		local rows, columns = self:GetNumRowsColumns()

		local result = 0;

		if self.ScrollDirection == self.Enum.ScrollDirection.Vertical then
			result = (row-1) * columns + column
		else
			result = (column-1) * rows + row
		end

		return result
	end

	function this:GetNumRowsColumns()
		local rows, columns = 0, 0

		local windowSize = self.Container.AbsoluteWindowSize
		local padding = self:GetPadding()
		local cellSize = self:GetCellSize()
		local cellSpacing = self:GetSpacing()
		local adjustedWindowSize = Utility.ClampVector2(Vector2.new(0, 0), windowSize - padding, windowSize - padding)
		local absoluteCellSize = Utility.ClampVector2(Vector2.new(1,1), cellSize + cellSpacing, cellSize + cellSpacing)
		local windowSizeCalc = (adjustedWindowSize + cellSpacing) / absoluteCellSize

		local GridItemsCount = 0
		for k, item in pairs(self.GridItems) do
			if item then
				GridItemsCount = GridItemsCount + 1
			end
		end

		if self.ScrollDirection == self.Enum.ScrollDirection.Vertical then
			columns = math.max(1, math.floor(windowSizeCalc.x))
			rows = math.ceil(math.max(1, GridItemsCount) / columns)
		else
			rows = math.max(1, math.floor(windowSizeCalc.y))
			columns = math.ceil(math.max(1, GridItemsCount) / rows)
		end

		return rows, columns
	end

	function this:GetGridPosition(row, column)
		local cellSize = self:GetCellSize()
		local spacing = self:GetSpacing()
		local padding = self:GetPadding()
		return UDim2.new(0, padding.X + column * cellSize.X + column * spacing.X,
						 0, padding.Y + row * cellSize.Y + row * spacing.Y)
	end

	function this:GetGridItemSize()
		return self.CellSize
	end

	function this:GetCanvasPositionForOffscreenItem(selectedObject)
		-- NOTE: using <= and >= instead of < and > because scrollingframe
		-- code may automatically bump it while we are observing the change
		if selectedObject and self.Container and self:ContainsItem(selectedObject) then
			if self.ScrollDirection == self.Enum.ScrollDirection.Vertical then
				if selectedObject.AbsolutePosition.Y <= self.Container.AbsolutePosition.Y then
					return Utility.ClampCanvasPosition(self.Container, Vector2.new(0, selectedObject.Position.Y.Offset)) -- - selectedObject.AbsoluteSize.Y/2))
				elseif selectedObject.AbsolutePosition.Y + selectedObject.AbsoluteSize.Y >= self.Container.AbsolutePosition.Y + self.Container.AbsoluteWindowSize.Y then
					return Utility.ClampCanvasPosition(self.Container, Vector2.new(0, -(self.Container.AbsoluteWindowSize.Y - selectedObject.Position.Y.Offset - selectedObject.AbsoluteSize.Y)  )) --+ selectedObject.AbsoluteSize.Y/2))
				end
			else
				if selectedObject.AbsolutePosition.X <= self.Container.AbsolutePosition.X then
					return Utility.ClampCanvasPosition(self.Container, Vector2.new(selectedObject.Position.X.Offset, 0))
				elseif selectedObject.AbsolutePosition.X + selectedObject.AbsoluteSize.X >= self.Container.AbsolutePosition.X + self.Container.AbsoluteWindowSize.X then
					return Utility.ClampCanvasPosition(self.Container, Vector2.new(-(self.Container.AbsoluteWindowSize.X - selectedObject.Position.X.Offset - selectedObject.AbsoluteSize.X), 0))
				end
			end
		end
	end

	function this:GetItemFrom2D(row, column)
		local rows, columns = self:GetNumRowsColumns()

		if row > rows or row < 1 or column > columns or column < 1 then
			return nil
		end

		return self.GridItems[self:GetIndexFrom2D(row, column)] or nil
	end

	function this:SelectAvailableItem()
		local width = self:GetTopLevelWidth()
		local height = self:GetTopLevelHeight()
		local centerRow, centerColumn = self:GetCellIndicesFromScreenPosition(
			self.Container.CanvasPosition.X + width / 2,
			self.Container.CanvasPosition.Y + height / 2)

		-- Find the item closest to the top left
		local bestGridItem = nil
		local score = 1e100

		for i, gridItem in pairs(self.GridItems) do
			local row, column = self:Get2DGridIndex(i)
			local newScore = math.abs(column) + math.abs(row)

			-- Favor on-screen items
			if gridItem.Position.X.Offset < this.currentTargetCanvasPosition.X or
			   gridItem.Position.Y.Offset < this.currentTargetCanvasPosition.Y then
				newScore = newScore + 10000
			end

			if newScore < score then
				bestGridItem = gridItem
				score = newScore
			end
		end

		Utility.SetSelectedCoreObject(bestGridItem or self.Container)
	end

	function this:GetAnalyticsInfo()
		return {[Analytics.WidgetNames('WidgetId')] = Analytics.WidgetNames('LazyScrollingGridId')}
	end

	function this:Focus()
		local selectedObject = self:FindAncestorGridItem(GuiService.SelectedCoreObject)

		if not( selectedObject and self:ContainsItem(selectedObject) ) then
			self:SelectAvailableItem()
		end
	end

	function this:RemoveFocus()
		if this:ContainsItem(GuiService.SelectedCoreObject) then
			Utility.SetSelectedCoreObject(nil)
		end
	end

	function this:Destroy()
		if self.Container then
			self.Container:Destroy()
		end
	end

	function this:handleSelectionChanged()
		local selectedObject = self:FindAncestorGridItem(GuiService.SelectedCoreObject)

		if selectedObject and self:ContainsItem(selectedObject) then
			local width = self:GetTopLevelWidth()
			local height = self:GetTopLevelHeight()
			local row, column = self:GetCellIndicesFromScreenPosition(selectedObject.Position.X.Offset, selectedObject.Position.Y.Offset)
			local centerRow, centerColumn = self:GetCellIndicesFromScreenPosition( self.Container.CanvasPosition.X + width / 2, self.Container.CanvasPosition.Y + height / 2)

			local nextPos = nil

			local cellSize = self:GetCellSize()
			local spacing = self:GetSpacing()
			local padding = self:GetPadding()

			local cellWidth = (spacing.X + cellSize.X)
			local cellHeight = (spacing.Y + cellSize.Y)

			if self.ScrollDirection == self.Enum.ScrollDirection.Vertical then
				local maxaway = math.floor( (math.floor(height / cellHeight) - 1) / 2 );

				if row > centerRow + maxaway then
					local newCenter = row - maxaway
					nextPos = Vector2.new(0, (newCenter-0.5) * cellHeight - height / 2)
				elseif row < centerRow - maxaway then
					local newCenter = row + maxaway
					nextPos = Vector2.new(0, (newCenter-0.5) * cellHeight - height / 2)
				end
			else
				local maxaway = math.floor( (math.floor(width / cellWidth) - 1) / 2 );

				if column > centerColumn + maxaway then
					local newCenter = column - maxaway
					nextPos = Vector2.new((newCenter-0.5) * cellWidth - width / 2, 0)
				elseif column < centerColumn - maxaway then
					local newCenter = column + maxaway
					nextPos = Vector2.new((newCenter-0.5) * cellWidth - width / 2, 0)
				end
			end

			if nextPos.X == self.currentTargetCanvasPosition.X and nextPos.Y == self.currentTargetCanvasPosition.Y then
				return
			end

			-- If there are too many other tasks, the animation pipeline can get behind, setting
			-- the canvas position to the last target helps the percieved speed of the scroll in that case.
			self.Container.CanvasPosition = self.currentTargetCanvasPosition

			if nextPos then
				nextPos = Vector2.new( math.max(0, nextPos.x), math.max(0, nextPos.y) )
				self:Rewindow(nextPos.x, nextPos.y, self:GetTopLevelWidth(), self:GetTopLevelHeight())

				Utility.PropertyTweener(self.Container, 'CanvasPosition', self.Container.CanvasPosition, nextPos, 0.2, Utility.EaseOutQuad, true)
				self.currentTargetCanvasPosition = nextPos
			end
		end
	end

	function this:FindAncestorGridItem(object)
		if object ~= nil then
			if self:ContainsItem(object) then
				return object
			end
			return self:FindAncestorGridItem(object.Parent)
		end
	end

	GuiService.Changed:connect(function(prop)
		if prop == 'SelectedCoreObject' then
			this:handleSelectionChanged()
		end
	end)

	return this
end

return LazyScrollingGrid
