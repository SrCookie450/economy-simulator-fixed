
local GuiService = game:GetService('GuiService')

local CoreGui = game:GetService("CoreGui")
local GuiRoot = CoreGui:FindFirstChild("RobloxGui")
local Modules = GuiRoot:FindFirstChild("Modules")
local ShellModules = Modules:FindFirstChild("Shell")

local Strings = require(ShellModules:FindFirstChild('LocalizedStrings'))
local AssetManager = require(ShellModules:FindFirstChild('AssetManager'))
local GlobalSettings = require(ShellModules:FindFirstChild('GlobalSettings'))
local Utility = require(ShellModules:FindFirstChild('Utility'))
local SoundManager = require(ShellModules:FindFirstChild('SoundManager'))

local function CreateMoreButton()
	if Utility.ShouldUseVRAppLobby() then
		local overrideSelection = Utility.Create'ImageLabel'
		{
			Name = "OverrideSelection";
			Image = '';
			BackgroundTransparency = 1;
			ImageTransparency = 1;
			ZIndex = 1;
		}
		local moreButton = Utility.Create'ImageButton'
		{
			Name = "MoreButton";
			BackgroundTransparency = 1;
			ZIndex = 2;
			Visible = false;
			SelectionImageObject = overrideSelection;
			ScaleType = Enum.ScaleType.Slice;
			SliceCenter = Rect.new(21,21,21,21);
			ImageColor3 = GlobalSettings.GreyButtonColor;
			Image = 'rbxasset://textures/ui/Lobby/Buttons/more_nine_slice_button.png';
			SoundManager:CreateSound('MoveSelection');
		}
		local moreText = Utility.Create'TextLabel'
		{
			Name = "MoreText";
			BackgroundTransparency = 1;
			BorderSizePixel = 0;
			Size = UDim2.new(1,0,1,0);
			Text = Strings:LocalizedString('MoreWord');
			Font = GlobalSettings.LightFont;
			FontSize = GlobalSettings.TitleSize;
			TextColor3 = GlobalSettings.WhiteTextColor;
			ZIndex = 2;
			Parent = moreButton;
		}

		local highlightGlow = Utility.Create'ImageLabel'
		{
			Name = "HighlightGlow";
			Image = 'rbxasset://textures/ui/Lobby/Buttons/glow_nine_slice.png';
			ScaleType = Enum.ScaleType.Slice;
			SliceCenter = Rect.new(78,78,78,78);
			Size = UDim2.new(1, 40, 1, 40);
			Position = UDim2.new(0, -20, 0, -20);
			BackgroundTransparency = 1;
			ImageTransparency = 1;
			Parent = moreButton;
		}

		local function updateMoreImage(isSelected)
			highlightGlow.ImageTransparency = isSelected and 0 or 1
		end
		moreButton.SelectionGained:connect(function()
			updateMoreImage(true)
		end)
		moreButton.SelectionLost:connect(function()
			updateMoreImage(false)
		end)
		updateMoreImage(GuiService.SelectedCoreObject == moreButton)

		return moreButton, overrideSelection
	else
		-- we override the selection on moreButton to fit around the moreImage
		local overrideSelection = Utility.Create'ImageLabel'
		{
			Name = "OverrideSelection";
			Image = 'rbxasset://textures/ui/SelectionBox.png';
			ScaleType = Enum.ScaleType.Slice;
			SliceCenter = Rect.new(19,19,43,43);
			BackgroundTransparency = 1;
		}

		local moreButton = Utility.Create'ImageButton'
		{
			Name = "MoreButton";
			BackgroundTransparency = 1;
			Visible = false;
			SelectionImageObject = overrideSelection;
			SoundManager:CreateSound('MoveSelection');
		}
		AssetManager.LocalImage(moreButton,
			'rbxasset://textures/ui/Shell/Buttons/MoreButton',
			{['720'] = UDim2.new(0,72,0,33); ['1080'] = UDim2.new(0,108,0,50);})

		local function updateMoreImage(isSelected)
			local uri = isSelected and 'rbxasset://textures/ui/Shell/Buttons/MoreButtonSelected' or 'rbxasset://textures/ui/Shell/Buttons/MoreButton'

			AssetManager.LocalImage(moreButton, uri, {['720'] = UDim2.new(0,72,0,33); ['1080'] = UDim2.new(0,108,0,50);})
			-- moreButton.Position = UDim2.new(1, -moreButton.Size.X.Offset, 0, 0)
		end
		moreButton.SelectionGained:connect(function()
			updateMoreImage(true)
		end)
		moreButton.SelectionLost:connect(function()
			updateMoreImage(false)
		end)
		updateMoreImage(GuiService.SelectedCoreObject == moreButton)

		return moreButton, overrideSelection
	end
end

return CreateMoreButton
