
local CoreGui = Game:GetService("CoreGui")
local GuiRoot = CoreGui:FindFirstChild("RobloxGui")
local Modules = GuiRoot:FindFirstChild("Modules")
local ShellModules = Modules:FindFirstChild("Shell")
local GuiService = game:GetService('GuiService')

local Utility = require(ShellModules:FindFirstChild('Utility'))
local FriendsView = require(ShellModules:FindFirstChild('LazyFriendsView'))
local GlobalSettings = require(ShellModules:FindFirstChild('GlobalSettings'))
local ScreenManager = require(ShellModules:FindFirstChild('ScreenManager'))
local Strings = require(ShellModules:FindFirstChild('LocalizedStrings'))
local ProgressSpinner = require(ShellModules:FindFirstChild('ProgressSpinner'))
local Analytics = require(ShellModules:FindFirstChild('Analytics'))

local function CreateFriendsPane(parent)
	local this = {}

	local friendsView = nil
	local isPaneFocused = false
	local defaultSelectionObject = nil

	local noSelectionObject = Utility.Create'ImageLabel'
	{
		Name = 'NoSelectionObject';
		BackgroundTransparency = 1;
	}

	local FriendsPaneContainer = Utility.Create'Frame'
	{
		Name = 'FriendsPane';
		Size = UDim2.new(1, 0, 1, 0);
		BackgroundTransparency = 1;
		Visible = false;
		SelectionImageObject = noSelectionObject;
		Parent = parent;
	}

	local onlineFriendsTitle = Utility.Create'TextLabel'
	{
		Name = "OnlineFriendsTitle";
		Size = UDim2.new(0, 0, 0, 33);
		BackgroundTransparency = 1;
		Font = GlobalSettings.RegularFont;
		FontSize = GlobalSettings.SubHeaderSize;
		TextColor3 = GlobalSettings.WhiteTextColor;
		TextXAlignment = Enum.TextXAlignment.Left;
		Text = string.upper(Strings:LocalizedString("OnlineFriendsWords"));
		Visible = false;
		Parent = FriendsPaneContainer;
	}

	local onlineFriendsContainer = Utility.Create'Frame'
	{
		Name = "OnlineFriendsContainer";
		Size = UDim2.new(0, 1720, 0, 610);
		Position = UDim2.new(0, 0, 0, onlineFriendsTitle.Size.Y.Offset);
		BackgroundTransparency = 1;
		Parent = FriendsPaneContainer;
	}

	local noFriendsIcon = Utility.Create'ImageLabel'
	{
		Name = "noFriendsIcon";
		Size = UDim2.new(0, 296, 0, 259);
		Position = UDim2.new(0.5, -296/2, 0, 100);
		BackgroundTransparency = 1;
		Image = 'rbxasset://textures/ui/Shell/Icons/FriendsIcon@1080.png';
		Visible = false;
		Parent = FriendsPaneContainer;
	}

	local noFriendsText = Utility.Create'TextLabel'
	{
		Name = "NoFriendsText";
		Size = UDim2.new(0, 500, 0, 72);
		BackgroundTransparency = 1;
		Font = GlobalSettings.RegularFont;
		FontSize = GlobalSettings.ButtonSize;
		TextColor3 = GlobalSettings.WhiteTextColor;
		Text = Strings:LocalizedString("NoFriendsPhrase");
		TextYAlignment = Enum.TextYAlignment.Top;
		TextWrapped = true;
		Visible = false;
		Parent = FriendsPaneContainer;
	}

	noFriendsText.Position = UDim2.new(0.5, -noFriendsText.Size.X.Offset/2, 0,
		noFriendsIcon.Position.Y.Offset + noFriendsIcon.Size.Y.Offset + 32)

	local function setPaneContentVisible(hasOnlineFriends)
		noFriendsIcon.Visible = not hasOnlineFriends
		noFriendsText.Visible = not hasOnlineFriends
		onlineFriendsTitle.Visible = hasOnlineFriends
		defaultSelectionObject = hasOnlineFriends and friendsView:GetDefaultFocusItem() or nil
	end

	local loadingSpinner = ProgressSpinner( {Parent = FriendsPaneContainer} )

	local function onFriendsUpdated(friendCount)
		loadingSpinner:Kill()
		setPaneContentVisible(friendCount > 0)
	end

	friendsView = FriendsView(
		{
			Size = UDim2.new(1, 0, 1, 0),
			CellSize = Vector2.new(446, 114),
			Spacing = Vector2.new(50, 10),
			Padding = Vector2.new(0, 0),
			ScrollDirection = "Horizontal",
			Position = UDim2.new(0, 0, 0, 0)
		},
		onFriendsUpdated
	)

	friendsView:SetParent(onlineFriendsContainer)


	function this:GetName()
		return Strings:LocalizedString('FriendsWord')
	end

	function this:GetAnalyticsInfo()
		return {[Analytics.WidgetNames('WidgetId')] = Analytics.WidgetNames('FriendsPaneId')}
	end

	function this:IsFocused()
		return isPaneFocused
	end

	function this:Show()
		FriendsPaneContainer.Visible = true
		self.TransitionTweens = ScreenManager:DefaultFadeIn(FriendsPaneContainer)
		ScreenManager:PlayDefaultOpenSound()
	end

	function this:Hide()
		FriendsPaneContainer.Visible = false
		ScreenManager:DefaultCancelFade(self.TransitionTweens)
		self.TransitionTweens = nil
	end

	function this:Focus()
	if Utility.IsFastFlagEnabled("XboxFriendsAnalytics") then
		if not isPaneFocused then
			Analytics.SetRBXEventStream("FriendsPaneEntered")
		end
	end
		isPaneFocused = true
		friendsView:Focus()
	end

	function this:RemoveFocus()
		friendsView:RemoveFocus()
		isPaneFocused = false
	end

	function this:SetPosition(newPosition)
		FriendsPaneContainer.Position = newPosition
	end

	function this:SetParent(newParent)
		FriendsPaneContainer.Parent = newParent
	end

	function this:IsAncestorOf(object)
		return FriendsPaneContainer and FriendsPaneContainer:IsAncestorOf(object)
	end

	return this
end

return CreateFriendsPane
