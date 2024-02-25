--[[
			// BaseScreen.lua

			// Creates a base screen with breadcrumbs and title. Do not use for a pane/tab
]]
local CoreGui = game:GetService("CoreGui")
local GuiRoot = CoreGui:FindFirstChild("RobloxGui")
local Modules = GuiRoot:FindFirstChild("Modules")
local ShellModules = Modules:FindFirstChild("Shell")


local AssetManager = require(ShellModules:FindFirstChild('AssetManager'))
local GlobalSettings = require(ShellModules:FindFirstChild('GlobalSettings'))
local Utility = require(ShellModules:FindFirstChild('Utility'))

local function createBaseScreen(controller)
	local this = {}

	local container = Utility.Create'Frame'
	{
		Name = "Container";
		Size = UDim2.new(1, 0, 1, 0);
		BackgroundTransparency = 1;
	}
	local backButton = Utility.Create'ImageButton'
	{
		Name = "BackButton";
		BackgroundTransparency = 1;
		Image = 'rbxasset://textures/ui/Lobby/Buttons/nine_slice_button.png';
		ImageColor3 = GlobalSettings.GreyButtonColor;
		Size = UDim2.new(0,175,0,48);
		ScaleType = Enum.ScaleType.Slice;
		SliceCenter = Rect.new(9,9,39,39);
	}
	local backImage = Utility.Create'ImageLabel'
	{
		Name = "BackImage";
		BackgroundTransparency = 1;
		Parent = container;
	}
	if not Utility.ShouldUseVRAppLobby() then
		AssetManager.LocalImage(backImage,
			'rbxasset://textures/ui/Shell/Icons/BackIcon', {['720'] = UDim2.new(0,32,0,32); ['1080'] = UDim2.new(0,48,0,48);})
	end
	local backText = Utility.Create'TextLabel'
	{
		Name = "BackText";
		Size = UDim2.new(0, 0, 0, backImage.Size.Y.Offset);
		Position = UDim2.new(0, backImage.Size.X.Offset + 8, 0, 0);
		BackgroundTransparency = 1;
		Font = GlobalSettings.RegularFont;
		FontSize = GlobalSettings.ButtonSize;
		TextXAlignment = Enum.TextXAlignment.Left;
		TextColor3 = GlobalSettings.WhiteTextColor;
		Text = "";
		Parent = container;
	}
	local titleText = Utility.Create'TextLabel'
	{
		Name = "TitleText";
		Size = UDim2.new(0, 0, 0, 35);
		Position = UDim2.new(0, 16, 0, backImage.Size.Y.Offset + 74);
		BackgroundTransparency = 1;
		Font = GlobalSettings.LightFont;
		FontSize = GlobalSettings.HeaderSize;
		TextXAlignment = Enum.TextXAlignment.Left;
		TextColor3 = GlobalSettings.WhiteTextColor;
		Text = "";
		Parent = container;
	}
	if Utility.ShouldUseVRAppLobby() then
		backButton.Parent = container;

		backImage.Parent = backButton
		backText.Parent = backButton

		backImage.Size = UDim2.new(0,25,0,25);
		backImage.Image = 'rbxasset://textures/ui/Lobby/Icons/back_icon.png';
		
		local spacing = (backButton.Size.Y.Offset - backImage.Size.Y.Offset) / 2
		backImage.Position = UDim2.new(0, spacing, 0, spacing)

		local textSpacing = (backButton.Size.Y.Offset - backText.Size.Y.Offset) / 2
		backText.Position = UDim2.new(0, backImage.Position.X.Offset + backImage.Size.X.Offset + 8, 0, textSpacing);
		titleText.Position = UDim2.new(0, 16, 0, backButton.Size.Y.Offset + 74);
	else
		titleText.Position = UDim2.new(0, 16, 0, backImage.Size.Y.Offset + 74);
	end

	--[[ Public API ]]--
	this.Container = container
	this.BackImage = backImage
	this.BackText = backText
	this.TitleText = titleText

	function this:SetBackText(newText)
		local TextService = game:GetService('TextService')

		newText = string.upper(newText)
		local textSize = TextService:GetTextSize(
			newText,
			Utility.ConvertFontSizeEnumToInt(backText.FontSize),
			backText.Font,
			Vector2.new()) -- Essentially, we don't want to bound our textbox

		local spacing = (backButton.Size.Y.Offset - backImage.Size.Y.Offset) / 2

		backText.Text = newText

		backButton.Size = UDim2.new(0, spacing * 2 + textSize.X + backImage.Size.X.Offset + 8,
			backButton.Size.Y.Scale, backButton.Size.Y.Offset)
	end

	this:SetBackText(controller:GetBackText())

	backButton.MouseButton1Click:connect(function()
		controller:OnBackButtonClick()
	end)

	return this
end

return createBaseScreen
