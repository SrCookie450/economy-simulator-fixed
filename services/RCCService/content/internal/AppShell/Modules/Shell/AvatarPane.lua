-- Written by Kip Turner, Copyright ROBLOX 2015

local TextService = game:GetService('TextService')

local CoreGui = game:GetService("CoreGui")
local GuiService = game:GetService('GuiService')
local RunService = game:GetService('RunService')

local GuiRoot = CoreGui:FindFirstChild("RobloxGui")
local Modules = GuiRoot:FindFirstChild("Modules")
local ShellModules = Modules:FindFirstChild("Shell")

local PackageData = require(ShellModules:FindFirstChild('PackageData'))
local OutfitData = require(ShellModules:FindFirstChild('OutfitData'))
local ScrollingGridModule = require(ShellModules:FindFirstChild('ScrollingGrid'))
local AvatarTile = require(ShellModules:FindFirstChild('AvatarTile'))
local OutfitTile = require(ShellModules:FindFirstChild('OutfitTile'))
local Utility = require(ShellModules:FindFirstChild('Utility'))
local ScreenManager = require(ShellModules:FindFirstChild('ScreenManager'))
local GlobalSettings = require(ShellModules:FindFirstChild('GlobalSettings'))
local LoadingWidget = require(ShellModules:FindFirstChild('LoadingWidget'))
local ThumbnailLoader = require(ShellModules:FindFirstChild('ThumbnailLoader'))
local UserData = require(ShellModules:FindFirstChild('UserData'))
local EventHub = require(ShellModules:FindFirstChild('EventHub'))

local Analytics = require(ShellModules:FindFirstChild('Analytics'))
local Strings = require(ShellModules:FindFirstChild('LocalizedStrings'))
local SoundManager = require(ShellModules:FindFirstChild('SoundManager'))

local HintActionView = require(ShellModules:FindFirstChild('HintActionView'))

local GLOW_BASE_RPM = 2
local GLOW_TOP_RPM = -0.5
local GLOW_TRANSPARENCY = 0.2
local CATALOG_BELOW_POSITION = 320


local function CreateAvatarPane(parent)
	local this = {}

	local inFocus = false
	local isShown = false

	local AvatarObjects = {}

	local OnGuiServiceChangedConn = nil

	local lastParent = parent

	local LastWearingPackageAssetId = nil
	local LastWearingOutfitId = nil


	local MainContainer = Utility.Create'Frame'
	{
		Name = 'AvatarPane';
		Size = UDim2.new(1, 0, 1, 0);
		BackgroundTransparency = 1;
		Visible = false;
	}



	local MyAvatarContainer = Utility.Create'Frame'
	{
		Name = 'MyAvatarContainer';
		Size = UDim2.new(0.38,0,1,0);
		Position = UDim2.new(0,0,0,0);
		BackgroundTransparency = 1;
		ClipsDescendants = true;
		Parent = MainContainer;
	}
		local MyNameLabel = Utility.Create'TextLabel'
		{
			Name = 'MyNameLabel';
			Text = '';
			Size = UDim2.new(1,0,0,25);
			Position = UDim2.new(0,12,0,0);
			TextXAlignment = 'Left';
			TextColor3 = GlobalSettings.WhiteTextColor;
			Font = GlobalSettings.RegularFont;
			FontSize = GlobalSettings.SubHeaderSize;
			BackgroundTransparency = 1;
			Parent = MyAvatarContainer;
		};
		local ProfileImageContainer = Utility.Create'Frame'
		{
			Name = 'ProfileImageContainer';
			Size = UDim2.new(0.68,0,0.9,-MyNameLabel.Size.Y.Offset);
			Position = UDim2.new(0.16, 0, 0.05, MyNameLabel.Size.Y.Offset);
			BackgroundTransparency = 1;
			Parent = MyAvatarContainer;
		}
			local ProfileImage = Utility.Create'ImageLabel'
			{
				Name = 'ProfileImage';
				Size = UDim2.new(0,780,0,780);
				Position = UDim2.new(0.5, 0, 0.5, 0);
				BackgroundTransparency = 1;
				ZIndex = 3;
				Parent = ProfileImageContainer;
			};
				local CrossfadeProfileImage = Utility.Create'ImageLabel'
				{
					Name = 'CrossfadeProfileImage';
					Size = UDim2.new(1,0,1,0);
					Position = UDim2.new(0, 0, 0, 0);
					BackgroundTransparency = 1;
					ImageTransparency = 1;
					ZIndex = 3;
					Parent = ProfileImage;
				};
					local CharacterGlowBase = Utility.Create'ImageLabel'
					{
						Name = 'CharacterGlowBase';
						Size = UDim2.new(0,1015,0,1002);
						BackgroundTransparency = 1;
						Image = 'rbxasset://textures/ui/Shell/Images/CharacterGlow/CharacterGlowBase.png';
						ImageTransparency = GLOW_TRANSPARENCY;
						Parent = CrossfadeProfileImage;
						AnchorPoint = Vector2.new(0.5, 0.5);
						Position = UDim2.new(0.5, 0, 0.5, 0);
					};
					local CharacterGlowTop = Utility.Create'ImageLabel'
					{
						Name = 'CharacterGlowTop';
						Size = UDim2.new(0,1026,0,1009);
						BackgroundTransparency = 1;
						Image = 'rbxasset://textures/ui/Shell/Images/CharacterGlow/CharacterGlowTop.png';
						ImageTransparency = GLOW_TRANSPARENCY;
						ZIndex = 2;
						Parent = CrossfadeProfileImage;
						AnchorPoint = Vector2.new(0.5, 0.5);
						Position = UDim2.new(0.5, 0, 0.5, 0);
					};
			local function onSizeChanged()
				ProfileImage.Size = Utility.CalculateFill(ProfileImage, Vector2.new(576, 324))
				ProfileImage.AnchorPoint = Vector2.new(0.5, 0.5)
				ProfileImage.Position = UDim2.new(0.5, 0, 0.5, 0)
			end

	-- Hint Action View
	local hintActionView = HintActionView(nil, "AvatarPaneSelectAction")
	hintActionView:SetImage('rbxasset://textures/ui/Shell/ButtonIcons/XButton.png')
	hintActionView:SetText(Strings:LocalizedString('EquipWord'))
	hintActionView:SetTransparency(1)
	hintActionView:SetParent(lastParent.Parent)	-- this assumes parent is HubContainer from AppHub.lua

	local function UpdateEquipButton(IsOwned, IsWearing)
		hintActionView:SetVisibleWithTween( (IsOwned and not IsWearing) and 0 or 1)
	end

	local SelectableAvatarsContainer = Utility.Create'Frame'
	{
		Name = 'SelectableAvatarsContainer';
		Size = UDim2.new(0.6,0,1,0);
		Position = UDim2.new(0.4,0,0,0);
		BackgroundTransparency = 1;
		Parent = MainContainer;
	}

		local NoCatalogStatusMessage = Utility.Create'TextLabel'
		{
			Name = 'NoCatalogStatusMessage';
			Text = Strings:LocalizedString('DefaultErrorPhrase');
			Size = UDim2.new(0.9,0,1,-125);
			Position = UDim2.new(0.05, 0, 0, 0);
			TextColor3 = GlobalSettings.GreyTextColor;
			TextWrapped = true;
			TextTransparency = GlobalSettings.FriendStatusTextTransparency;
			Font = GlobalSettings.BoldFont;
			FontSize = GlobalSettings.DescriptionSize;
			BackgroundTransparency = 1;
			Visible = false;
			Parent = SelectableAvatarsContainer;
		};

		local OutfitsTitle = Utility.Create'TextLabel'
		{
			Name = 'OutfitsTitle';
			Text = Strings:LocalizedString('AvatarOutfitsTitle'):upper();
			Size = UDim2.new(1,0,0,40);
			TextXAlignment = 'Left';
			TextColor3 = GlobalSettings.WhiteTextColor;
			Font = GlobalSettings.RegularFont;
			FontSize = GlobalSettings.SubHeaderSize;
			BackgroundTransparency = 1;
			Visible = false;
			Parent = SelectableAvatarsContainer;
		};

			local OutfitsScroller = ScrollingGridModule()
			OutfitsScroller:SetSize(UDim2.new(1,0,1,-OutfitsTitle.Size.Y.Offset - 40))
			OutfitsScroller:SetScrollDirection(OutfitsScroller.Enum.ScrollDirection.Horizontal)
			OutfitsScroller:SetCellSize(Vector2.new(220, 220))
			OutfitsScroller:SetSpacing(Vector2.new(25,25))
			OutfitsScroller:SetPosition(UDim2.new(0,0,0,OutfitsTitle.Size.Y.Offset))
			OutfitsScroller:SetRowColumnConstraint(1)
			-- OutfitsScroller:SetParent(SelectableAvatarsContainer)

		local CatalogTitle = Utility.Create'TextLabel'
		{
			Name = 'CatalogTitle';
			Text = Strings:LocalizedString('AvatarCatalogTitle'):upper();
			Size = UDim2.new(1,0,0,40);
			Position = UDim2.new(0,0,0,0);
			TextXAlignment = 'Left';
			TextColor3 = GlobalSettings.WhiteTextColor;
			Font = GlobalSettings.RegularFont;
			FontSize = GlobalSettings.SubHeaderSize;
			BackgroundTransparency = 1;
			Parent = SelectableAvatarsContainer;
		};

			local AvatarScroller = ScrollingGridModule()
			AvatarScroller:SetSize(UDim2.new(1,0,1,-CatalogTitle.Size.Y.Offset - 40))
			AvatarScroller:SetScrollDirection(AvatarScroller.Enum.ScrollDirection.Horizontal)
			AvatarScroller:SetCellSize(Vector2.new(220, 220))
			AvatarScroller:SetPosition(UDim2.new(0,0,0,40))
			AvatarScroller:SetSpacing(Vector2.new(25,25))
			AvatarScroller:SetRowColumnConstraint(2)
			AvatarScroller:SetParent(SelectableAvatarsContainer)

	local function SortOutfitsScroller()
		OutfitsScroller:SortItems(
			function(a, b)
				local aObject = a and AvatarObjects[a] and AvatarObjects[a]:GetPackageInfo()
				local bObject = b and AvatarObjects[b] and AvatarObjects[b]:GetPackageInfo()
				local aIsEquipped = aObject and aObject:IsWearing()
				local bIsEquipped = bObject and bObject:IsWearing()
				local aName = aObject and aObject:GetName()
				local bName = bObject and bObject:GetName()

				if aIsEquipped then return true elseif bIsEquipped then return false end
				if aName and bName then
					return aName < bName
				end
				return aObject ~= nil
			end)
	end
  
	--if all avatars owned, outfits should be 2 rows and catalog should be hidden
	local function CheckAllAvatarsOwned()
		if #AvatarScroller.GridItems == 0 then
			OutfitsScroller:SetRowColumnConstraint(2)
			CatalogTitle.Visible = false
			AvatarScroller.Visible = false
			AvatarScroller:SetParent(nil)
		else
			OutfitsScroller:SetRowColumnConstraint(1)
			CatalogTitle.Visible = true
			AvatarScroller.Visible = true
			AvatarScroller:SetParent(SelectableAvatarsContainer)
		end
	end

	local function OnAddToOutfitsScroller()
		CatalogTitle.Position = UDim2.new(0,0,0, CATALOG_BELOW_POSITION)
		AvatarScroller:SetPosition(UDim2.new(0,0,0,CATALOG_BELOW_POSITION + 40))
		AvatarScroller:SetRowColumnConstraint(1)
		OutfitsTitle.Visible = true
		OutfitsScroller:SetParent(SelectableAvatarsContainer)
		if Utility.IsFastFlagEnabled('XboxAvatarCatalogFix') then
			CheckAllAvatarsOwned()
		end
		SortOutfitsScroller()
	end



	local LoaderSpinner = nil
	local ProfileImageThumbnailLoader = nil
	local GlobalFadeCount = 1
	local function CrossfadeAvatarImage(frontImage, fadeImage, newImageUrl, duration)
		duration = duration or 0.75

		GlobalFadeCount = GlobalFadeCount + 1
		local thisFadeCount = GlobalFadeCount

		local fadeoutDuration = duration

		spawn(function()
			local dummyImage = {}
			local function waitForWearReady()
				PackageData:AwaitWearAssetRequest()
				if thisFadeCount == GlobalFadeCount then
					if ProfileImageThumbnailLoader then ProfileImageThumbnailLoader:Cancel() end
					ProfileImageThumbnailLoader = ThumbnailLoader:Create(dummyImage, newImageUrl, ThumbnailLoader.Sizes.Large, ThumbnailLoader.AssetType.Avatar, true)
					local loadResult = ProfileImageThumbnailLoader:LoadAsync(false, false)
					ProfileImageThumbnailLoader = nil
				end
			end

			if LoaderSpinner then LoaderSpinner:Cleanup() end
			local newLoaderSpinner = LoadingWidget({Parent = frontImage, ZIndex = 3}, {waitForWearReady})
			LoaderSpinner = newLoaderSpinner
			newLoaderSpinner:AwaitFinished()
			newLoaderSpinner:Cleanup()

			if thisFadeCount == GlobalFadeCount then
				local noPreviousImage = (frontImage.Image == "" and fadeImage.Image == "")
				if noPreviousImage then
					fadeoutDuration = 0
				else
					fadeImage.Image = frontImage.Image
				end


				Utility.PropertyTweener(fadeImage, 'ImageTransparency', noPreviousImage and 0 or frontImage.ImageTransparency, 1, fadeoutDuration, Utility.EaseInOutQuad, true)
				Utility.PropertyTweener(CharacterGlowBase, 'ImageTransparency', CharacterGlowBase.ImageTransparency, 1, fadeoutDuration, Utility.EaseInOutQuad, true)
				Utility.PropertyTweener(CharacterGlowTop, 'ImageTransparency', CharacterGlowTop.ImageTransparency, 1, fadeoutDuration, Utility.EaseInOutQuad, true)
				frontImage.ImageTransparency = 1

				frontImage.Image = dummyImage.Image

				Utility.PropertyTweener(frontImage, 'ImageTransparency', frontImage.ImageTransparency, 0, duration, Utility.EaseInOutQuad, true)
				Utility.PropertyTweener(CharacterGlowBase, 'ImageTransparency', CharacterGlowBase.ImageTransparency, GLOW_TRANSPARENCY, duration, Utility.EaseInOutQuad, true)
				Utility.PropertyTweener(CharacterGlowTop, 'ImageTransparency', CharacterGlowTop.ImageTransparency, GLOW_TRANSPARENCY, duration, Utility.EaseInOutQuad, true)
			end
		end)
	end

	local function UpdateProfileImage(forceRefresh)
		MyNameLabel.Text = UserData:GetDisplayName()
		if forceRefresh or
				LastWearingPackageAssetId ~= PackageData:GetCachedWearingPackage() or
				LastWearingOutfitId ~= OutfitData:GetCachedWearingOutfitId() then
			CrossfadeAvatarImage(ProfileImage, CrossfadeProfileImage, UserData.GetLocalUserIdAsync())

			LastWearingPackageAssetId = PackageData:GetCachedWearingPackage()
			LastWearingOutfitId = OutfitData:GetCachedWearingOutfitId()
		end
	end

	UpdateProfileImage(true)

	local function onOwnershipChanged(tile, nowOwns)
		-- Move purchased packages into my outfits
		if nowOwns then
			local guiObject = tile and tile:GetGuiObject()
			if guiObject and AvatarScroller:ContainsItem(guiObject) then
				AvatarScroller:RemoveItem(guiObject)
				OutfitsScroller:AddItem(guiObject)
				OnAddToOutfitsScroller()
				if inFocus then
					GuiService.SelectedCoreObject = guiObject
				end
			end
		end
	end

	local ownershipChangedCns = {}

	local function listenToOwnershipChanged(tile)
		local packageInfo = tile and tile:GetPackageInfo()
		if packageInfo and not packageInfo:IsOwned() and (packageInfo.OwnershipChanged ~= nil) then
			if not ownershipChangedCns[tile] then
				ownershipChangedCns[tile] = packageInfo.OwnershipChanged:connect(function(nowOwns)
					onOwnershipChanged(tile, nowOwns)
				end)
			end
		end
	end

	local function removeListenToOwnershipChanged(tile)
		if ownershipChangedCns[tile] then
			ownershipChangedCns[tile]:disconnect()
			ownershipChangedCns[tile] = nil
		end
	end

	local packagesLoaded = false
	local outfitsLoaded = true -- NOTE: we don't load outfits atm
	local LoadAvatarWebDataLoader = nil
	local function LoadAvatarWebData()
		local function loadCatalogPackages()
			local packages = PackageData:GetXboxCatalogPackagesAsync()

			if packages and not packagesLoaded then
				packagesLoaded = true
				for _, packageInfo in pairs(packages) do
					local avatarItemContainer = AvatarTile(packageInfo)

					AvatarObjects[avatarItemContainer:GetGuiObject()] = avatarItemContainer

					if packageInfo:IsOwned() then
						OutfitsScroller:AddItem(avatarItemContainer:GetGuiObject())
						OnAddToOutfitsScroller()
					else
						AvatarScroller:AddItem(avatarItemContainer:GetGuiObject())
						listenToOwnershipChanged(avatarItemContainer)
					end

					if isShown then
						avatarItemContainer:Show()
					end
				end
			end

		end

		local function loadOutfits()
			local outfits = OutfitData:GetMyOutfitsAsync()

			if outfits and not outfitsLoaded then
				outfitsLoaded = true
				for _, outfitInfo in pairs(outfits) do
					local outfitItemContainer = OutfitTile(outfitInfo)

					AvatarObjects[outfitItemContainer:GetGuiObject()] = outfitItemContainer
					OutfitsScroller:AddItem(outfitItemContainer:GetGuiObject())
					OnAddToOutfitsScroller()
					if isShown then
						outfitItemContainer:Show()
					end
				end
			end

		end

		if LoadAvatarWebDataLoader then return end

		SelectableAvatarsContainer.Visible = false
		local containerSize = SelectableAvatarsContainer.Size
		LoadAvatarWebDataLoader = LoadingWidget(
			{Parent = MainContainer, Position = SelectableAvatarsContainer.Position + UDim2.new(containerSize.X.Scale / 2, containerSize.X.Offset / 2, containerSize.Y.Scale / 2, containerSize.Y.Offset / 2)},
			-- {loadCatalogPackages, loadOutfits})
			{loadCatalogPackages})
		spawn(function()
			NoCatalogStatusMessage.Visible = false

			LoadAvatarWebDataLoader:AwaitFinished()
			LoadAvatarWebDataLoader:Cleanup()
			LoadAvatarWebDataLoader = nil
			SelectableAvatarsContainer.Visible = true
			-- SortOutfitsScroller()

			if not (packagesLoaded and outfitsLoaded) then
				NoCatalogStatusMessage.Visible = true
			end

			if inFocus and isShown and GuiService.SelectedCoreObject == nil then
				GuiService.SelectedCoreObject = this:GetDefaultSelectableObject()
			end

			if this.TransitionTweens == nil or #this.TransitionTweens == 0 then
				this.TransitionTweens = ScreenManager:FadeInSitu(SelectableAvatarsContainer)
			end
		end)
	end
	LoadAvatarWebData()

	local function onEquipChanged()
		local selectedObject = GuiService.SelectedCoreObject

		if AvatarObjects[selectedObject] then
			local packageInfo = AvatarObjects[selectedObject]:GetPackageInfo()
			if packageInfo then
				UpdateEquipButton(packageInfo:IsOwned(), packageInfo:IsWearing())
			end
		else
			-- user can select avatar tab button
			UpdateEquipButton(false, false)
		end
		SortOutfitsScroller()
	end

	local lastSelectedObject = GuiService.SelectedCoreObject
	local function OnSelectedCoreObjectChanged()
		local selectedObject = GuiService.SelectedCoreObject

		if AvatarObjects[lastSelectedObject] then
			AvatarObjects[lastSelectedObject]:RemoveFocus()
		end
		if AvatarObjects[selectedObject] then
			AvatarObjects[selectedObject]:Focus()
		end
		onEquipChanged()

		lastSelectedObject = selectedObject
	end


	function this:GetDefaultSelectableObject()
		if OutfitsScroller.GridItems[1] then
			return OutfitsScroller.GridItems[1]
		end
		if AvatarScroller.GridItems[1] then
			return AvatarScroller.GridItems[1]
		end
	end


	--[[ Public API ]]--
	function this:GetName()
		return Strings:LocalizedString('AvatarWord')
	end

	function this:GetAnalyticsInfo()
		return {[Analytics.WidgetNames('WidgetId')] = Analytics.WidgetNames('AvatarPaneId')}
	end

	function this:IsFocused()
		return inFocus
	end

	local debounceSelect = false
	function this:OnSelectAction()
		if debounceSelect then return end
		debounceSelect = true

		local selectedObject = GuiService.SelectedCoreObject
		if selectedObject and AvatarObjects[selectedObject] then
			if AvatarObjects[selectedObject]:Select() then
				SoundManager:Play('ButtonPress')
			end
		end

		debounceSelect = false
	end

	function this:OpenEquippedPackage()
		if isShown and inFocus then
			for _, avatarItemContainer in pairs(AvatarObjects) do
				local packageInfo = avatarItemContainer:GetPackageInfo()
				if packageInfo then
					if packageInfo:IsWearing() then
						avatarItemContainer:OnClick()
					end
				end
			end
		end
	end
	--[[ End Public API ]]--

	local profileImageChangeCn = nil
	function this:Show()
		isShown = true

		Utility.DisconnectEvent(profileImageChangeCn)
		profileImageChangeCn = ProfileImageContainer.Changed:connect(function(prop)
			if prop == 'AbsoluteSize' then
				onSizeChanged()
			end
		end)
		onSizeChanged()

		local lastUpdate = tick()
		RunService:BindToRenderStep("UpdateAvatarGlow", Enum.RenderPriority.Camera.Value,
			function()
				local now = tick()
				local delta = now - lastUpdate

				CharacterGlowBase.Rotation = CharacterGlowBase.Rotation + delta * GLOW_BASE_RPM * 6 -- 6 = 360 / 60
				CharacterGlowTop.Rotation = CharacterGlowTop.Rotation + delta * GLOW_TOP_RPM * 6

				lastUpdate = now
			end)

		if not (packagesLoaded and outfitsLoaded) then
			LoadAvatarWebData()
		end

		for _, avatarItemContainer in pairs(AvatarObjects) do
			avatarItemContainer:Show()
			listenToOwnershipChanged(avatarItemContainer)
			local packageInfo = tile and tile:GetPackageInfo()
			if packageInfo then
				onOwnershipChanged(avatarItemContainer, packageInfo:IsOwned())
			end
		end

		UpdateProfileImage()

		EventHub:addEventListener(EventHub.Notifications["AvatarEquipBegin"], "AvatarPane",
			function(assetId)
				UpdateProfileImage(true)
				spawn(function()
					PackageData:AwaitWearAssetRequest()
					LastWearingPackageAssetId = PackageData:GetCachedWearingPackage()
				end)
			end)

		EventHub:addEventListener(EventHub.Notifications["DonnedDifferentPackage"], "AvatarPane",
		function()
			onEquipChanged()
		end)
		EventHub:addEventListener(EventHub.Notifications["DonnedDifferentOutfit"], "AvatarPane",
		function()
			onEquipChanged()
			UpdateProfileImage()
		end)

		local seenXButtonPressed = false
		local function onSelectAvatar(actionName, inputState, inputObject)
			if inputState == Enum.UserInputState.Begin then
				seenXButtonPressed = true
			elseif inputState == Enum.UserInputState.End and seenXButtonPressed then
				self:OnSelectAction()
			end
		end

		hintActionView:BindAction(onSelectAvatar, Enum.KeyCode.ButtonX)

		self.TransitionTweens = ScreenManager:DefaultFadeIn(MainContainer)

		SortOutfitsScroller()

		MainContainer.Parent = lastParent
		MainContainer.Visible = true
	end


	function this:Hide()
		isShown = false
		MainContainer.Visible = false

		profileImageChangeCn = Utility.DisconnectEvent(profileImageChangeCn)

		RunService:UnbindFromRenderStep("UpdateAvatarGlow")
		hintActionView:UnbindAction()
		hintActionView:SetTransparency(1)

		for _, avatarItemContainer in pairs(AvatarObjects) do
			avatarItemContainer:Hide()
			removeListenToOwnershipChanged(avatarItemContainer)
		end

		EventHub:removeEventListener(EventHub.Notifications["AvatarEquipBegin"], "AvatarPane")
		EventHub:removeEventListener(EventHub.Notifications["DonnedDifferentPackage"], "AvatarPane")
		EventHub:removeEventListener(EventHub.Notifications["DonnedDifferentOutfit"], "AvatarPane")


		ScreenManager:DefaultCancelFade(self.TransitionTweens)
		self.TransitionTweens = nil
		-- Clean out saved selected object so when we tab back
		-- we will start with the default selection
		self.SavedSelectObject = nil
	end

	function this:Focus()
		inFocus = true

		Utility.DisconnectEvent(OnGuiServiceChangedConn)
		OnGuiServiceChangedConn = GuiService.Changed:connect(function(prop)
			if prop == 'SelectedCoreObject' then
				OnSelectedCoreObjectChanged()
			end
		end)
		OnSelectedCoreObjectChanged()

		if self.SavedSelectObject and self.SavedSelectObject:IsDescendantOf(MainContainer) then
			GuiService.SelectedCoreObject = self.SavedSelectObject
		else
			local defaultSelection = self:GetDefaultSelectableObject()
			if defaultSelection then
				GuiService.SelectedCoreObject = defaultSelection
			end
		end
	end

	function this:RemoveFocus()
		inFocus = false
		--Remove Focus on AvatarObjects
		if AvatarObjects and AvatarObjects[lastSelectedObject] then
			AvatarObjects[lastSelectedObject]:RemoveFocus()
		end
		OnGuiServiceChangedConn = Utility.DisconnectEvent(OnGuiServiceChangedConn)

		local selectedObject = GuiService.SelectedCoreObject
		if isShown and selectedObject and selectedObject:IsDescendantOf(MainContainer) then
			self.SavedSelectObject = GuiService.SelectedCoreObject
			GuiService.SelectedCoreObject = nil
		end
	end

	function this:SetPosition(newPosition)
		MainContainer.Position = newPosition
	end

	function this:SetParent(newParent)
		lastParent = newParent
		MainContainer.Parent = newParent
	end

	return this
end

return CreateAvatarPane
