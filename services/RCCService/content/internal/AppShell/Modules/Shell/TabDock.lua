-- Written by Kip Turner, Copyright ROBLOX 2015

local GuiService = game:GetService('GuiService')
local ContextActionService = game:GetService("ContextActionService")
local UserInputService = game:GetService("UserInputService")

local CoreGui = game:GetService("CoreGui")
local GuiRoot = CoreGui:FindFirstChild("RobloxGui")
local Modules = GuiRoot:FindFirstChild("Modules")
local ShellModules = Modules:FindFirstChild("Shell")
local Utility = require(ShellModules:FindFirstChild('Utility'))
local GlobalSettings = require(ShellModules:FindFirstChild('GlobalSettings'))
local Analytics = require(ShellModules:FindFirstChild('Analytics'))

local TAB_DOCK_HEIGHT = Utility.ShouldUseVRAppLobby() and 56 or 36

local function CreateTabDock(restPosition, offscreenPosition)
	local this = {}

	local Tabs = {}
	local SelectedTab = nil
	local SizeChangedConns = {}
	this.SelectedTabChanged = Utility.Signal()
	this.SelectedTabClicked = Utility.Signal()
	this.TabItemClickedConn = nil
	local guiServiceChangedCn = nil

	local TabContainer = Utility.Create'ImageButton'
	{
		Size = UDim2.new(1, 0, 0, TAB_DOCK_HEIGHT);
		Position = restPosition;
		BackgroundTransparency = 1;
		Name = 'TabContainer';
	}

	local SelectionBorderObject = Utility.Create'ImageLabel'
	{
		Name = 'SelectionBorderObject';
		Size = UDim2.new(1,0,0,4);
		Position = UDim2.new(0,0,1,5);
		BorderSizePixel = 0;
		BackgroundColor3 = GlobalSettings.TabUnderlineColor;
		-- Image = 'rbxasset://textures/ui/SelectionBox.png';
		-- ScaleType = Enum.ScaleType.Slice;
		-- SliceCenter = Rect.new(19,19,43,43);
		BackgroundTransparency = 0;
	}

	local DownSelector = Utility.Create'ImageButton'
	{
		Size = UDim2.new(1, 0, 0, TAB_DOCK_HEIGHT);
		BackgroundTransparency = 1;
		Name = 'DownSelector';
		Selectable = false;
		Parent = TabContainer;
	}

	DownSelector.SelectionGained:connect(function()
		if SelectedTab then
			Utility.SetSelectedCoreObject(SelectedTab:GetGuiObject())
			this.SelectedTabClicked:fire(SelectedTab)
		end
	end)

	local function onGuiServiceChanged2(prop)
		if prop == 'SelectedCoreObject' then
			if GuiService.SelectedCoreObject == TabContainer then
				local currentTab = this:GetSelectedTab()
				local currentTabItem = currentTab and currentTab:GetGuiObject()
				if currentTabItem then
					GuiService.SelectedCoreObject = currentTabItem
					if Utility.ShouldUseVRAppLobby() then
						return
					end
				end
			end

			local selectedObject = GuiService.SelectedCoreObject
			local focusedTab = this:FindFocusedTabByGuiObject(selectedObject)

			-- They navigated away from the TabDock
			if SelectedTab then
				SelectedTab:SetFocused(focusedTab == SelectedTab)
			end

			if focusedTab then
				this:SetSelectedTab(focusedTab)
				SelectedTab:SetFocused(focusedTab == SelectedTab)

				if this.TabItemClickedConn then
					this.TabItemClickedConn:disconnect()
				end
				this.TabItemClickedConn = SelectedTab.Clicked:connect(function()
					this.SelectedTabClicked:fire(SelectedTab)
				end)
			else
				if this.TabItemClickedConn then
					this.TabItemClickedConn:disconnect()
					this.TabItemClickedConn = nil
				end
			end
		end
	end


	local function onGuiServiceChanged(prop)
		if prop == 'SelectedCoreObject' then

			if GuiService.SelectedCoreObject == TabContainer then
				local currentTab = this:GetSelectedTab()
				local currentTabItem = currentTab and currentTab:GetGuiObject()
				if currentTabItem then
					Utility.SetSelectedCoreObject(currentTabItem)
				end
			end

			local selectedObject = GuiService.SelectedCoreObject
			local focusedTab = this:FindFocusedTabByGuiObject(selectedObject)

			if SelectedTab and selectedObject ~= SelectedTab:GetGuiObject() then
				SelectedTab:OnClickRelease()
			end

			if focusedTab then
				this:SetSelectedTab(focusedTab)

				for _, inputObject in pairs(UserInputService:GetGamepadState(Enum.UserInputType.Gamepad1)) do
					if inputObject.KeyCode == Enum.KeyCode.ButtonA and inputObject.UserInputState == Enum.UserInputState.Begin then
						if SelectedTab then
							SelectedTab:OnClick()
						end
					end
				end

				ContextActionService:UnbindCoreAction("OnClickSelectedTab")

				ContextActionService:BindCoreAction("OnClickSelectedTab",
					function(actionName, inputState, inputObject)
						if inputState == Enum.UserInputState.Begin then
							if SelectedTab then
								SelectedTab:OnClick()
							end
						elseif inputState == Enum.UserInputState.End then
							if SelectedTab then
								SelectedTab:OnClickRelease()
							end
							this.SelectedTabClicked:fire(SelectedTab)
						end
					end,
					false,
					Enum.KeyCode.ButtonA)
			else
				ContextActionService:UnbindCoreAction("OnClickSelectedTab")
			end
		end
	end

	--Never shown--
	function this:GetAnalyticsInfo()
		return {[Analytics.WidgetNames('WidgetId')] = Analytics.WidgetNames('TabDockId')}
	end

	function this:FindFocusedTabByGuiObject(selectedObject)
		-- NOTE: This is a sort of cheater way of culling look-up checks
		if selectedObject and selectedObject:IsDescendantOf(TabContainer) then
			for _, currTab in pairs(Tabs) do
				local guiObject = currTab and currTab:GetGuiObject()
				if guiObject and guiObject == selectedObject then
					return currTab
				end
			end
		end
	end

	function this:IsFocused()
		local selectedObject = GuiService.SelectedCoreObject
		return self:FindFocusedTabByGuiObject(selectedObject) ~= nil
	end

	function this:SetSelectedTab(newSelectedTab)
		if newSelectedTab ~= SelectedTab then
			local prevSelectedTab = SelectedTab
			if SelectedTab then
				SelectedTab:SetSelected(false)
				if Utility.ShouldUseVRAppLobby() then
					SelectedTab:SetFocused(false)
				else
					SelectedTab:OnClickRelease()
				end
			end

			SelectedTab = newSelectedTab

			if SelectedTab then
				SelectedTab:SetSelected(true)
				if self:IsFocused() then
					local currentTabItem = SelectedTab and SelectedTab:GetGuiObject()
					if currentTabItem then
						Utility.SetSelectedCoreObject(currentTabItem)
					end
				end
			end
			if Utility.ShouldUseVRAppLobby() then
				-- Fire tab selection event with the name of the new tab selection
				this.SelectedTabChanged:fire(prevSelectedTab, SelectedTab)
			else
				this.SelectedTabChanged:fire(SelectedTab)
			end
		end
	end

	function this:Focus()
		self:Show()
		if SelectedTab then
			SelectedTab:SetSelected(true)
			local currentTabItem = SelectedTab and SelectedTab:GetGuiObject()
			if currentTabItem then
				Utility.SetSelectedCoreObject(currentTabItem)
			end
		end
	end

	function this:GetSelectedTab()
		return SelectedTab
	end

	local arrangeCount = 0
	function this:ArrangeTabs()
		arrangeCount = arrangeCount + 1
		local currentCount = arrangeCount

		local x = 0
		for i, tabItem in pairs(Tabs) do
			local tabItemSize = tabItem:GetSize()
			local xSize = tabItemSize.X.Offset

			local spacing = GlobalSettings.TabItemSpacing
			if i == 1 then
				spacing = 0
			end
			-- Stop recursion in its tracks
			if currentCount == arrangeCount then
				tabItem:SetPosition(UDim2.new(0, x + spacing, 0, 0))

				local tabItemGuiObject = tabItem:GetGuiObject()
				if tabItemGuiObject then
					local prevItemGuiObject = Tabs[i-1] and Tabs[i-1]:GetGuiObject()
					local nextItemGuiObject = Tabs[i+1] and Tabs[i+1]:GetGuiObject()
					tabItemGuiObject.NextSelectionLeft = prevItemGuiObject
					tabItemGuiObject.NextSelectionRight = nextItemGuiObject
				end

			else
				return
			end
			x = x + spacing + xSize
		end
	end

	function this:FindTabIndex(tab)
		for i, currTab in pairs(Tabs) do
			if tab == currTab then
				return i
			end
		end
	end

	function this:ContainsTab(tab)
		return self:FindTabIndex(tab) ~= nil
	end

	function this:GetTab(index)
		return Tabs[index]
	end

	function this:GetNextTab()
		if SelectedTab then
			local index = this:FindTabIndex(SelectedTab)
			return index and Tabs[index + 1]
		end
	end

	function this:GetPreviousTab()
		if SelectedTab then
			local index = this:FindTabIndex(SelectedTab)
			return index and Tabs[index - 1]
		end
	end

	function this:AddTab(newTab)
		local existingIndex = self:FindTabIndex(newTab)
		if existingIndex then
			print("Not adding tab:" , newTab:GetName() , "because that tab already exists.")
			return
		end

		local guiObject = newTab and newTab:GetGuiObject()
		if guiObject then
			if not Utility.ShouldUseVRAppLobby() then
				guiObject.SelectionImageObject = SelectionBorderObject
			end
			guiObject.NextSelectionDown = DownSelector
		end

		table.insert(Tabs, newTab)
		newTab:SetParent(TabContainer)

		Utility.DisconnectEvent(SizeChangedConns[newTab])
		SizeChangedConns[newTab] = newTab.SizeChanged:connect(function()
			self:ArrangeTabs()
		end)

		self:ArrangeTabs()

		return newTab
	end

	function this:RemoveTab(tab)
		local removeIndex = self:FindTabIndex(tab)

		if removeIndex then
			table.remove(Tabs, removeIndex)
			if tab == SelectedTab then
				this:SetSelectedTab(nil)
			end
			tab:SetParent(nil)
			Utility.DisconnectEvent(SizeChangedConns[tab])
			self:ArrangeTabs()
			return true
		end

		return false
	end

	function this:SetParent(newParent)
		TabContainer.Parent = newParent
	end

	function this:ConnectEvents()
		if Utility.ShouldUseVRAppLobby() then
			onGuiServiceChanged2('SelectedCoreObject')
			guiServiceChangedCn = GuiService.Changed:connect(onGuiServiceChanged2)
		else
			onGuiServiceChanged('SelectedCoreObject')
			guiServiceChangedCn = GuiService.Changed:connect(onGuiServiceChanged)
		end

		if Utility.ShouldUseVRAppLobby() then
			ContextActionService:BindCoreAction("CycleTabDock",
				function(actionName, inputState, inputObject)
					if inputState == Enum.UserInputState.End then
						if inputObject.KeyCode == Enum.KeyCode.ButtonL1 then
							local prevTab = self:GetPreviousTab()
							if prevTab then
								self:SetSelectedTab(prevTab)
							end
						elseif inputObject.KeyCode == Enum.KeyCode.ButtonR1 then
							local nextTab = self:GetNextTab()
							if nextTab then
								self:SetSelectedTab(nextTab)
							end
						end
					end
				end,
				false,
				Enum.KeyCode.ButtonL1, Enum.KeyCode.ButtonR1)

			local seenBButtonBegin = false
			ContextActionService:BindCoreAction("CloseAppHub",
				function(actionName, inputState, inputObject)
					if inputState == Enum.UserInputState.Begin then
						seenBButtonBegin = true
					elseif inputState == Enum.UserInputState.End then
						if seenBButtonBegin then
							local currentlySelectedTab = self:GetSelectedTab()
							if currentlySelectedTab ~= self:GetTab(1) then
								self:SetSelectedTab(self:GetTab(1))
							end
						end
					end
				end,
				false,
				Enum.KeyCode.ButtonB)
		end
	end

	function this:DisconnectEvents()
		if guiServiceChangedCn then
			guiServiceChangedCn:disconnect()
			guiServiceChangedCn = nil
		end

		if Utility.ShouldUseVRAppLobby() then
			ContextActionService:UnbindCoreAction("CycleTabDock")
			ContextActionService:UnbindCoreAction("CloseAppHub")
		end
	end

	local motionDuration = GlobalSettings.TabDockTweenDuration

	local function FadeText(element, tweens, a, b, duration)
		if element == nil then return end
		if element:IsA('TextLabel') or element:IsA('TextBox') or element:IsA('TextButton') then
			table.insert(tweens, Utility.PropertyTweener(element, 'TextTransparency', a, b, duration, Utility.EaseOutQuad))
		end
		for _, child in pairs(element:GetChildren()) do
			FadeText(child, tweens, a, b, duration)
		end
	end

	local tweens = {}

	local showing = false;

	function this:Hide()
		if not showing then return end
		showing = false;

		Utility.CancelTweens(tweens)

		local positionTweener = Utility.PropertyTweener(
			TabContainer,
			'Position',
			restPosition,
			offscreenPosition,
			motionDuration,
			Utility.SCurveUDim2
		)
		table.insert(tweens, positionTweener)

		FadeText(TabContainer, tweens, 0, 1, motionDuration)
	end

	function this:Show()
		if showing then return end
		showing = true;

		Utility.CancelTweens(tweens)

		local positionTweener = Utility.PropertyTweener(
			TabContainer,
			'Position',
			offscreenPosition,
			restPosition,
			motionDuration,
			Utility.SCurveUDim2
		)
		table.insert(tweens, positionTweener)

		FadeText(TabContainer, tweens, 1, 0, motionDuration)
	end

	return this
end

return CreateTabDock
