-- Written by Kip Turner, Copyright ROBLOX 2015

local CoreGui = Game:GetService("CoreGui")
local GuiRoot = CoreGui:FindFirstChild("RobloxGui")
local Modules = GuiRoot:FindFirstChild("Modules")
local ShellModules = Modules:FindFirstChild("Shell")

local Utility = require(ShellModules:FindFirstChild('Utility'))

local GuiService = game:GetService('GuiService')

local DEFAULT_WINDOW_SIZE = UDim2.new(1,0,1,0)


local function ScrollingGrid()

	local this = {}

	this.Enum =
	{
		ScrollDirection = {["Vertical"] = 1; ["Horizontal"] = 2;};
		StartCorner = {["UpperLeft"] = 1; ["UpperRight"] = 2; ["BottomLeft"] = 3; ["BottomRight"] = 4;};
		OverflowMode = {["Scrolling"] = 1; ["Paging"] = 2;};
		--ChildAlignment = {["UpperLeft"] = 1; ["UpperRight"] = 2; ["BottomLeft"] = 3; ["BottomRight"] = 4;};
	}


	this.GridItems = {}
	this.ItemSet = {}

	this.ScrollDirection = this.Enum.ScrollDirection.Vertical

	this.StartCorner = this.Enum.StartCorner.UpperLeft

	this.OverflowMode = this.Enum.OverflowMode.Scrolling

	this.FixedRowColumnCount = nil

	--this.ChildAlignment = nil

	this.CellSize = Vector2.new(100,100)
	this.Padding = Vector2.new(0,0)
	this.Spacing = Vector2.new(0,0)


	function this:GetOverflowMode()
		return self.OverflowMode
	end

	function this:SetOverflowMode(newOverflowMode)
		if newOverflowMode ~= self.OverflowMode then
			local oldOverflowMode = self.OverflowMode
			self.OverflowMode = newOverflowMode
			self.PrevButton.Visible = (newOverflowMode == self.Enum.OverflowMode.Paging)
			self.NextButton.Visible = (newOverflowMode == self.Enum.OverflowMode.Paging)

			if oldOverflowMode == self.Enum.OverflowMode.Paging then
				for i = #self.GridItems, 1, -1 do
					local item = self.GridItems[i]
					if item then
						item.Visible = true
					end
				end
			elseif newOverflowMode == self.Enum.OverflowMode.Paging then
				self:HideClippedItems()
			end
			self:RecalcLayout()
		end
	end


	function this:GetPadding()
		return self.Padding
	end

	function this:SetPadding(newPadding)
		if newPadding ~= self.Padding then
			self.Padding = newPadding
			self:RecalcLayout()
		end
	end

	function this:GetSpacing()
		return self.Spacing
	end

	function this:SetSpacing(newSpacing)
		if newSpacing ~= self.Spacing then
			self.Spacing = newSpacing
			self:RecalcLayout()
		end
	end

	function this:GetCellSize()
		return self.CellSize
	end

	function this:SetCellSize(cellSize)
		if cellSize ~= self.CellSize then
			self.CellSize = cellSize
			self:RecalcLayout()
		end
	end

	function this:GetScrollDirection()
		return self.ScrollDirection
	end

	function this:SetScrollDirection(newDirection)
		if newDirection ~= self.ScrollDirection then
			self.ScrollDirection = newDirection
			self:RecalcLayout()
		end
	end

	function this:GetStartCorner()
		return self.StartCorner
	end

	function this:SetStartCorner(newStartCorner)
		if newStartCorner ~= self.StartCorner then
			self.StartCorner = newStartCorner
			self:RecalcLayout()
		end
	end

	function this:GetRowColumnConstraint()
		return self.FixedRowColumnCount
	end

	function this:SetRowColumnConstraint(fixedRowColumnCount)
		if fixedRowColumnCount < 1 then
			fixedRowColumnCount = nil
		end
		if fixedRowColumnCount ~= self.FixedRowColumnCount then
			self.FixedRowColumnCount = fixedRowColumnCount
			self:RecalcLayout()
		end
	end


	function this:GetClipping()
		return self.Container.ClipsDescendants
	end

	function this:SetClipping(clippingEnabled)
		self.Container.ClipsDescendants = clippingEnabled;
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

	function this:SetSize(size)
		self.Container.Size = size
	end

	function this:GetPosition()
		return self.Container.Position
	end

	function this:SetPosition(position)
		self.Container.Position = position
	end

	function this:GetParent()
		return self.Container.Parent
	end

	function this:SetParent(parent)
		self.Container.Parent = parent
	end

	function this:GetGuiObject()
		return self.Container
	end

	-- Default selection handles the case of removing the last item in the grid while it is selected
	-- Set to nil if do not want a default selection
	function this:SetDefaultSelection(selectionObject)
		self.DefaultSelection = selectionObject
	end

	function this:ResetDefaultSelection()
		self.DefaultSelection = self.Container
	end

	----


	function this:GetNumberOfItemsFitInFrame()
		if self.ScrollDirection == self.Enum.ScrollDirection.Horizontal then
			return math.floor(self.Container.AbsoluteSize.X  / (self:GetGridItemSize() + self.Spacing + self.Padding).X)
		else
			return math.floor(self.Container.AbsoluteSize.Y  / (self:GetGridItemSize() + self.Spacing + self.Padding).Y)
		end
	end

	function this:HideClippedItems()
		local firstVisibleIndex = #self.GridItems
		for i = #self.GridItems, 1, -1 do
			local item = self.GridItems[i]
			if item then
				if item.Position.X.Offset < self.ScrollingArea.CanvasPosition.X or 
						item.Position.X.Offset + item.AbsoluteSize.X > self.ScrollingArea.CanvasPosition.X + self.ScrollingArea.AbsoluteWindowSize.X then
					item.Visible = false
				else
					firstVisibleIndex = i
					item.Visible = true
				end
			end
		end
		local rowColumnCount = this:GetRowColumnConstraint() or 1
		self.CurrentPage = math.floor(firstVisibleIndex / (self:GetNumberOfItemsFitInFrame() * rowColumnCount))
	end

	function this:ContainsItem(gridItem)
		return self.ItemSet[gridItem] ~= nil
	end

	function this:SortItems(sortFunc)
		table.sort(self.GridItems, sortFunc)
		self:RecalcLayout()

		local selectedObject = self:FindAncestorGridItem(GuiService.SelectedCoreObject)
		if selectedObject and self:ContainsItem(selectedObject) then
			local thisPos = self:GetCanvasPositionForOffscreenItem(selectedObject)
			if thisPos then
				Utility.PropertyTweener(self.ScrollingArea, 'CanvasPosition', thisPos, thisPos, 0, Utility.EaseOutQuad, true)
			end
		end
	end

	function this:AddItem(gridItem)
		if not self:ContainsItem(gridItem) then
			table.insert(self.GridItems, gridItem)
			self.ItemSet[gridItem] = true
			gridItem.Parent = self.ScrollingArea
			if GuiService.SelectedCoreObject == self.DefaultSelection then
				Utility.SetSelectedCoreObject(gridItem)
			end
			self:RecalcLayout()
		end
	end

	function this:RemoveItem(gridItem)
		if self:ContainsItem(gridItem) then
			for i, otherItem in pairs(self.GridItems) do
				if otherItem == gridItem then
					table.remove(self.GridItems, i)
					-- Assign a new selection
					if GuiService.SelectedCoreObject == gridItem then
						GuiService.SelectedCoreObject = self.GridItems[i] or self.GridItems[i-1] or self.GridItems[1] or self.DefaultSelection
					end
					-- Clean-up
					self.ItemSet[gridItem] = nil
					gridItem.Parent = nil
					self:RecalcLayout()
					return
				end
			end
		end
	end

	function this:RemoveAllItems()
		local wasSelected = false
		do
			local currentSelection = GuiService.SelectedCoreObject
			while currentSelection ~= nil and wasSelected == false do
	  			wasSelected = wasSelected or self:ContainsItem(currentSelection)
	  			currentSelection = currentSelection.Parent
	  		end
		end
		for i = #self.GridItems, 1, -1 do
			local removed = table.remove(self.GridItems, i)
			self.ItemSet[removed] = nil
			removed.Parent = nil
		end

		if wasSelected then
			GuiService.SelectedCoreObject = self.Container
		end

		self:RecalcLayout()
		self.ScrollingArea.CanvasPosition = Vector2.new(0, 0)
	end

	function this:Get2DGridIndex(index)
		-- 0 base index
		local zerobasedIndex = index - 1
		local rows, columns = self:GetNumRowsColumns()
		local row, column;

		-- TODO: implement StartCorner here
		if self.ScrollDirection == self.Enum.ScrollDirection.Vertical then
			row = math.floor(zerobasedIndex / columns)
			column = zerobasedIndex % columns
		else
			column = math.floor(zerobasedIndex / rows)
			row = zerobasedIndex % rows
		end

		return row, column
	end

	function this:GetNumRowsColumns()
		local rows, columns = 0, 0

		local windowSize = self.ScrollingArea.AbsoluteWindowSize
		local padding = self:GetPadding()
		local cellSize = self:GetCellSize()
		local cellSpacing = self:GetSpacing()
		local adjustedWindowSize = Utility.ClampVector2(Vector2.new(0, 0), windowSize - padding, windowSize - padding)
		local absoluteCellSize = Utility.ClampVector2(Vector2.new(1,1), cellSize + cellSpacing, cellSize + cellSpacing)
		local windowSizeCalc = (adjustedWindowSize + cellSpacing) / absoluteCellSize

		if self.ScrollDirection == self.Enum.ScrollDirection.Vertical then
			columns = math.max(1, self:GetRowColumnConstraint() or math.floor(windowSizeCalc.x))
			rows = math.ceil(math.max(1, #self.GridItems) / columns)
		else
			rows = math.max(1, self:GetRowColumnConstraint() or math.floor(windowSizeCalc.y))
			columns = math.ceil(math.max(1, #self.GridItems) / rows)
		end

		return rows, columns
	end

	function this:GetGridPosition(row, column, gridItemSize)
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
		if selectedObject and self.ScrollingArea and self:ContainsItem(selectedObject) then
			if self.ScrollDirection == self.Enum.ScrollDirection.Vertical then
				if selectedObject.AbsolutePosition.Y <= self.ScrollingArea.AbsolutePosition.Y then
					return Utility.ClampCanvasPosition(self.ScrollingArea, Vector2.new(0, selectedObject.Position.Y.Offset)) -- - selectedObject.AbsoluteSize.Y/2))
				elseif selectedObject.AbsolutePosition.Y + selectedObject.AbsoluteSize.Y >= self.ScrollingArea.AbsolutePosition.Y + self.ScrollingArea.AbsoluteWindowSize.Y then
					return Utility.ClampCanvasPosition(self.ScrollingArea, Vector2.new(0, -(self.ScrollingArea.AbsoluteWindowSize.Y - selectedObject.Position.Y.Offset - selectedObject.AbsoluteSize.Y)  )) --+ selectedObject.AbsoluteSize.Y/2))
				end
			else -- Horizontal
				if selectedObject.AbsolutePosition.X <= self.ScrollingArea.AbsolutePosition.X then
					return Utility.ClampCanvasPosition(self.ScrollingArea, Vector2.new(selectedObject.Position.X.Offset, 0))
				elseif selectedObject.AbsolutePosition.X + selectedObject.AbsoluteSize.X >= self.ScrollingArea.AbsolutePosition.X + self.ScrollingArea.AbsoluteWindowSize.X then
					return Utility.ClampCanvasPosition(self.ScrollingArea, Vector2.new(-(self.ScrollingArea.AbsoluteWindowSize.X - selectedObject.Position.X.Offset - selectedObject.AbsoluteSize.X), 0))
				end
			end
		end
	end

	function this:RecalcLayout()
		local padding = self:GetPadding()
		local cellSpacing = self:GetSpacing()
		local gridItemSize = self:GetGridItemSize()
		local rows, columns = self:GetNumRowsColumns()


		if self.ScrollDirection == self.Enum.ScrollDirection.Vertical then
			self.ScrollingArea.CanvasSize = UDim2.new(self.ScrollingArea.Size.X.Scale, self.ScrollingArea.Size.X.Offset, 0, padding.Y * 2 + rows * gridItemSize.Y + (math.max(0, rows - 1)) * cellSpacing.Y)
		else
			self.ScrollingArea.CanvasSize = UDim2.new(0, padding.X * 2 + columns * gridItemSize.X + (math.max(0, columns - 1)) * cellSpacing.X, self.ScrollingArea.Size.Y.Scale, self.ScrollingArea.Size.Y.Offset)
		end

		if self.OverflowMode == self.Enum.OverflowMode.Paging and Utility.ShouldUseVRAppLobby() then
			self.ScrollingArea.Size = UDim2.new(0, self:GetNumberOfItemsFitInFrame() * (this:GetGridItemSize() + this.Spacing + this.Padding).X - this.Spacing.X - this.Padding.X, 1, 0)
			self.ScrollingArea.Position = UDim2.new(0, (this.Container.AbsoluteSize.X - self.ScrollingArea.Size.X.Offset) / 2, 0, 0)
		else
			self.ScrollingArea.Size = UDim2.new(1, 0, 1, 0)
			self.ScrollingArea.Position = UDim2.new(0, 0, 0, 0)
		end

		local grid2DtoIndex = {}
		for i = 1, #self.GridItems do
			local row, column = self:Get2DGridIndex(i)
			local gridItem = self.GridItems[i]

			gridItem.Size = UDim2.new(0, gridItemSize.X, 0, gridItemSize.Y)
			gridItem.Position = self:GetGridPosition(row, column, gridItemSize)

			grid2DtoIndex[row] = grid2DtoIndex[row] or {}
			grid2DtoIndex[row][column] = gridItem
		end

		for rowNum, row in pairs(grid2DtoIndex) do
			for columnNum, column in pairs(row) do
				local gridItem = grid2DtoIndex[rowNum][columnNum]
				if gridItem then
					if self.ScrollDirection == self.Enum.ScrollDirection.Vertical then
						gridItem.NextSelectionUp = grid2DtoIndex[rowNum - 1] and grid2DtoIndex[rowNum - 1][columnNum] or nil
						gridItem.NextSelectionDown = grid2DtoIndex[rowNum + 1] and grid2DtoIndex[rowNum + 1][columnNum] or nil
						if gridItem.NextSelectionDown == nil and grid2DtoIndex[rowNum + 1] ~= nil then
							gridItem.NextSelectionDown = self.GridItems[#self.GridItems]
						end
						gridItem.NextSelectionLeft = nil
						gridItem.NextSelectionRight = nil
					else
						gridItem.NextSelectionLeft = grid2DtoIndex[rowNum] and grid2DtoIndex[rowNum][columnNum - 1] or nil
						gridItem.NextSelectionRight = grid2DtoIndex[rowNum] and grid2DtoIndex[rowNum][columnNum + 1] or nil
						if gridItem.NextSelectionRight == nil and grid2DtoIndex[0] and grid2DtoIndex[0][columnNum + 1] then
								gridItem.NextSelectionRight = self.GridItems[#self.GridItems]
						end
						gridItem.NextSelectionUp = nil
						gridItem.NextSelectionDown = nil
					end
				end
			end
		end
	end


	function this:Destroy()
		if self.Container then
			self.Container:Destroy()
		end
	end

	do
		local container = Utility.Create'Frame'
		{
			Size = DEFAULT_WINDOW_SIZE;
			Name = "Container";
			BackgroundTransparency = 1;
			ClipsDescendants = true;
		}
		local scrollingArea = Utility.Create'ScrollingFrame'
		{
			Size = UDim2.new(1,0,1,0);
			Name = "ScrollingArea";
			BackgroundTransparency = 1;
			ScrollingEnabled = false;
			ScrollBarThickness = 0;
			Selectable = false;
			Parent = container;
		}

		this.Container = container
		this.ScrollingArea = scrollingArea

		this.DefaultSelection = this.Container

		this.ScrollingArea.Changed:connect(function(prop)
			if prop == 'AbsoluteSize' then
				this:RecalcLayout()
			end
		end)

		local prevButton = Utility.Create'ImageButton'
		{
			Size = UDim2.new(0,56, 0, 56);
			Position = UDim2.new(0.5,-56 - 150/2,1,-56);
			Name = "PrevButton";
			BackgroundTransparency = 1;
			Image = 'rbxasset://textures/ui/Lobby/Buttons/scroll_button.png';
			Visible = false;
			Parent = container;
		}
		local nextButton = Utility.Create'ImageButton'
		{
			Size = UDim2.new(0,56, 0, 56);
			Position = UDim2.new(0.5,150/2,1,-56);
			Name = "NextButton";
			Visible = false;
			BackgroundTransparency = 1;
			Image = 'rbxasset://textures/ui/Lobby/Buttons/scroll_button.png';
			Parent = container;
		}
		do
			local prevIcon = Utility.Create'ImageLabel'
			{
				Size = UDim2.new(0,24, 0, 24);
				Position = UDim2.new(0.5,-12,0.5,-12);
				Name = "PrevIcon";
				BackgroundTransparency = 1;
				Image = 'rbxasset://textures/ui/Lobby/Buttons/scroll_left.png';
				Parent = prevButton;
			}
			local nextIcon = prevIcon:Clone()
			nextIcon.Image = 'rbxasset://textures/ui/Lobby/Buttons/scroll_right.png';
			nextIcon.Parent = nextButton
		end

		local function UpdateScrollButtonVisibility()
			-- Only show the buttons if we are in Paging mode
			if this:GetOverflowMode() == this.Enum.OverflowMode.Paging then
				-- Do logic for the Prev Button
				if (this.ScrollDirection == this.Enum.ScrollDirection.Vertical and scrollingArea.CanvasPosition.Y ~= 0) or
						(this.ScrollDirection == this.Enum.ScrollDirection.Horizontal and scrollingArea.CanvasPosition.X ~= 0) then
					prevButton.Visible = true
				else
					prevButton.Visible = false
				end

				-- Do logic the next button
				if (this.ScrollDirection == this.Enum.ScrollDirection.Vertical and Utility.ClampCanvasPosition(scrollingArea, scrollingArea.CanvasPosition + Vector2.new(0, 1)) ~= scrollingArea.CanvasPosition) or
						(this.ScrollDirection == this.Enum.ScrollDirection.Horizontal and Utility.ClampCanvasPosition(scrollingArea, scrollingArea.CanvasPosition + Vector2.new(1, 0)) ~= scrollingArea.CanvasPosition) then
					nextButton.Visible = true
				else
					nextButton.Visible = false
				end
				this:HideClippedItems()
			-- In the case there we are not in paging mode
			else
				prevButton.Visible = false
				nextButton.Visible = false
			end
		end

		local function GetCanvasPositionByToCenterIndex(index)
			local item = this.GridItems[index]
			if item then
				if this.ScrollDirection == this.Enum.ScrollDirection.Horizontal then
					return Vector2.new(item.Position.X.Offset, 0)
				else
					return Vector2.new(0, item.Position.Y.Offset)
				end
			else
				return scrollingArea.CanvasPosition
			end
		end

		local function ScrollInDirection(direction)
			local rowColumnCount = this:GetRowColumnConstraint() or 1
			local maxPageIndex = math.ceil(#this.GridItems / (this:GetNumberOfItemsFitInFrame() * rowColumnCount))
			local newPageIndex = Utility.Clamp(0, maxPageIndex, this.CurrentPage + direction)
			-- Add one for the 1-based indexing
			local itemIndexByPage = (this:GetNumberOfItemsFitInFrame() * rowColumnCount * newPageIndex) + 1

			this.ScrollingArea.CanvasPosition = Utility.ClampCanvasPosition(this.ScrollingArea,
				GetCanvasPositionByToCenterIndex(itemIndexByPage))
		end

		prevButton.MouseButton1Click:connect(function()
			ScrollInDirection(-1)
		end)
		nextButton.MouseButton1Click:connect(function()
			ScrollInDirection(1)
		end)

		if Utility.ShouldUseVRAppLobby() then
			scrollingArea.Changed:connect(function(prop)
				if prop == "CanvasSize" or prop == "CanvasPosition" or prop == "AbsoluteWindowSize" then
					UpdateScrollButtonVisibility()
				end
			end)
		end

		this.PrevButton = prevButton
		this.NextButton = nextButton

		this:RecalcLayout()
		if Utility.ShouldUseVRAppLobby() then
			UpdateScrollButtonVisibility()
		end

		function this:FindAncestorGridItem(object)
			if object ~= nil then
				if self:ContainsItem(object) then
					return object
				end
				return self:FindAncestorGridItem(object.Parent)
			end
		end

		local lastSelectedObject = nil
		GuiService.Changed:connect(function(prop)
			-- Only perform scrolling grid movement if we are in scrolling grid mode
			if this:GetOverflowMode() ~= this.Enum.OverflowMode.Scrolling then
				return
			end
			if prop == 'SelectedCoreObject' then
				local selectedObject = this:FindAncestorGridItem(GuiService.SelectedCoreObject)
				if selectedObject and this:ContainsItem(selectedObject) then
					-- print(selectedObject.NextSelectionUp, selectedObject.NextSelectionDown, selectedObject.NextSelectionLeft, selectedObject.NextSelectionRight)

					local upDirection = (this.ScrollDirection == this.Enum.ScrollDirection.Vertical) and 'NextSelectionUp' or 'NextSelectionLeft'
					local downDirection = (this.ScrollDirection == this.Enum.ScrollDirection.Vertical) and 'NextSelectionDown' or 'NextSelectionRight'
					local upObject = selectedObject[upDirection]
					local downObject = selectedObject[downDirection]

					local nextPos, upPos, downPos;


					local gridItemSize = this:GetGridItemSize()
					local thisPos = this:GetCanvasPositionForOffscreenItem(selectedObject)

					if lastSelectedObject then
						local lastUpObject = lastSelectedObject[upDirection]
						local lastDownObject = lastSelectedObject[downDirection]

						if upObject and lastUpObject == selectedObject then
							upPos = this:GetCanvasPositionForOffscreenItem(upObject)
							upPos = upPos and upPos + gridItemSize / 2
						elseif downObject and lastDownObject == selectedObject then
							downPos = this:GetCanvasPositionForOffscreenItem(downObject)
							downPos = downPos and downPos - gridItemSize / 2
						end
					end

					if upPos and (upPos.Y < this.ScrollingArea.CanvasPosition.Y or upPos.X < this.ScrollingArea.CanvasPosition.X) then
						nextPos = upPos
						-- print('up' , nextPos , selectedObject.Position, lastSelectedObject and lastSelectedObject.Position)
					elseif downPos and (downPos.Y > this.ScrollingArea.CanvasPosition.Y or downPos.X > this.ScrollingArea.CanvasPosition.X) then
						nextPos = downPos
						-- print('down' , nextPos , selectedObject.Position, lastSelectedObject and lastSelectedObject.Position)
					else
						nextPos = thisPos
						-- print('this' , selectedObject.Name , nextPos , selectedObject.Position, lastSelectedObject and lastSelectedObject.Position, selectedObject.AbsolutePosition, this.ScrollingArea.AbsolutePosition)
					end

					if nextPos then
						-- print("nextPos" , selectedObject.Name , nextPos)
						nextPos = Utility.ClampCanvasPosition(this.ScrollingArea, nextPos)
						if thisPos then --and thisPos ~= nextPos then
							-- Sort of a hack to not snap on the last one
							if (upObject and downObject) then
								Utility.PropertyTweener(this.ScrollingArea, 'CanvasPosition', thisPos, thisPos, 0, Utility.EaseOutQuad, true)
							end
						end
						Utility.PropertyTweener(this.ScrollingArea, 'CanvasPosition', this.ScrollingArea.CanvasPosition, nextPos, 0.2, Utility.EaseOutQuad, true)
					end
					lastSelectedObject = selectedObject
				else
					lastSelectedObject = nil
				end
			end
		end)
	end

	return this
end

return ScrollingGrid
