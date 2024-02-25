local sheetIds = {
	[1] = 'rbxasset://textures/AvatarEditorImages/Sheet1.png'
}

local sprites = {
	["bar-empty-head"] = {
		name = "bar-empty-head",
		filename = "1x/bar-empty-head.png",
		imageRectSize = Vector2.new(3, 6),
		imageRectOffset = Vector2.new(120, 24),
		imageId = sheetIds[1],
	},
	["bar-empty-mid"] = {
		name = "bar-empty-mid",
		filename = "1x/bar-empty-mid.png",
		imageRectSize = Vector2.new(1, 6),
		imageRectOffset = Vector2.new(121, 36),
		imageId = sheetIds[1],
	},
	["bar-empty-tail"] = {
		name = "bar-empty-tail",
		filename = "1x/bar-empty-tail.png",
		imageRectSize = Vector2.new(3, 6),
		imageRectOffset = Vector2.new(120, 30),
		imageId = sheetIds[1],
	},
	["bar-full-head"] = {
		name = "bar-full-head",
		filename = "1x/bar-full-head.png",
		imageRectSize = Vector2.new(7, 14),
		imageRectOffset = Vector2.new(56, 28),
		imageId = sheetIds[1],
	},
	["bar-full-mid"] = {
		name = "bar-full-mid",
		filename = "1x/bar-full-mid.png",
		imageRectSize = Vector2.new(1, 14),
		imageRectOffset = Vector2.new(120, 36),
		imageId = sheetIds[1],
	},
	["bar-full-tail"] = {
		name = "bar-full-tail",
		filename = "1x/bar-full-tail.png",
		imageRectSize = Vector2.new(7, 14),
		imageRectOffset = Vector2.new(56, 42),
		imageId = sheetIds[1],
	},
	["btn-expand"] = {
		name = "btn-expand",
		filename = "1x/btn-expand.png",
		imageRectSize = Vector2.new(60, 24),
		imageRectOffset = Vector2.new(64, 0),
		imageId = sheetIds[1],
	},
	["dragger-highlight"] = {
		name = "dragger-highlight",
		filename = "1x/dragger-highlight.png",
		imageRectSize = Vector2.new(48, 48),
		imageRectOffset = Vector2.new(180, 52),
		imageId = sheetIds[1],
	},
	["dragger"] = {
		name = "dragger",
		filename = "1x/dragger.png",
		imageRectSize = Vector2.new(32, 32),
		imageRectOffset = Vector2.new(122, 104),
		imageId = sheetIds[1],
	},
	["gr-box-shadow"] = {
		name = "gr-box-shadow",
		filename = "1x/gr-box-shadow.png",
		imageRectSize = Vector2.new(12, 12),
		imageRectOffset = Vector2.new(110, 93),
		imageId = sheetIds[1],
	},
	["gr-card corner"] = {
		name = "gr-card corner",
		filename = "1x/gr-card corner.png",
		imageRectSize = Vector2.new(13, 13),
		imageRectOffset = Vector2.new(110, 80),
		imageId = sheetIds[1],
	},
	["gr-category-selector"] = {
		name = "gr-category-selector",
		filename = "1x/gr-category-selector.png",
		imageRectSize = Vector2.new(52, 52),
		imageRectOffset = Vector2.new(0, 84),
		imageId = sheetIds[1],
	},
	["gr-circle-shadow"] = {
		name = "gr-circle-shadow",
		filename = "1x/gr-circle-shadow.png",
		imageRectSize = Vector2.new(56, 56),
		imageRectOffset = Vector2.new(124, 0),
		imageId = sheetIds[1],
	},
	["gr-circle-white"] = {
		name = "gr-circle-white",
		filename = "1x/gr-circle-white.png",
		imageRectSize = Vector2.new(48, 48),
		imageRectOffset = Vector2.new(124, 56),
		imageId = sheetIds[1],
	},
	["gr-corner"] = {
		name = "gr-corner",
		filename = "1x/gr-corner.png",
		imageRectSize = Vector2.new(56, 56),
		imageRectOffset = Vector2.new(64, 24),
		imageId = sheetIds[1],
	},
	["gr-half-circle"] = {
		name = "gr-half-circle",
		filename = "1x/gr-half-circle.png",
		imageRectSize = Vector2.new(34, 70),
		imageRectOffset = Vector2.new(220, 108),
		imageId = sheetIds[1],
	},
	["gr-orange-circle"] = {
		name = "gr-orange-circle",
		filename = "1x/gr-orange-circle.png",
		imageRectSize = Vector2.new(52, 52),
		imageRectOffset = Vector2.new(180, 0),
		imageId = sheetIds[1],
	},
	["gr-ring-selector"] = {
		name = "gr-ring-selector",
		filename = "1x/gr-ring-selector.png",
		imageRectSize = Vector2.new(54, 54),
		imageRectOffset = Vector2.new(56, 80),
		imageId = sheetIds[1],
	},
	["gr-ring01"] = {
		name = "gr-ring01",
		filename = "1x/gr-ring01.png",
		imageRectSize = Vector2.new(48, 48),
		imageRectOffset = Vector2.new(172, 100),
		imageId = sheetIds[1],
	},
	["gr-tail"] = {
		name = "gr-tail",
		filename = "1x/gr-tail.png",
		imageRectSize = Vector2.new(1, 70),
		imageRectOffset = Vector2.new(123, 24),
		imageId = sheetIds[1],
	},
	["gra-toggle-button"] = {
		name = "gra-toggle-button",
		filename = "1x/gra-toggle-button.png",
		imageRectSize = Vector2.new(30, 24),
		imageRectOffset = Vector2.new(52, 134),
		imageId = sheetIds[1],
	},
	["gra-toggle-frame"] = {
		name = "gra-toggle-frame",
		filename = "1x/gra-toggle-frame.png",
		imageRectSize = Vector2.new(64, 28),
		imageRectOffset = Vector2.new(0, 0),
		imageId = sheetIds[1],
	},
	["ic-all-on"] = {
		name = "ic-all-on",
		filename = "1x/ic-all-on.png",
		imageRectSize = Vector2.new(28, 28),
		imageRectOffset = Vector2.new(228, 52),
		imageId = sheetIds[1],
	},
	["ic-all"] = {
		name = "ic-all",
		filename = "1x/ic-all.png",
		imageRectSize = Vector2.new(28, 28),
		imageRectOffset = Vector2.new(228, 80),
		imageId = sheetIds[1],
	},
	["ic-avatar-animation-on"] = {
		name = "ic-avatar-animation-on",
		filename = "1x/ic-avatar-animation-on.png",
		imageRectSize = Vector2.new(28, 28),
		imageRectOffset = Vector2.new(82, 134),
		imageId = sheetIds[1],
	},
	["ic-avatar-animation"] = {
		name = "ic-avatar-animation",
		filename = "1x/ic-avatar-animation.png",
		imageRectSize = Vector2.new(28, 28),
		imageRectOffset = Vector2.new(0, 136),
		imageId = sheetIds[1],
	},
	["ic-back-on"] = {
		name = "ic-back-on",
		filename = "1x/ic-back-on.png",
		imageRectSize = Vector2.new(28, 28),
		imageRectOffset = Vector2.new(28, 158),
		imageId = sheetIds[1],
	},
	["ic-back-white"] = {
		name = "ic-back-white",
		filename = "1x/ic-back-white.png",
		imageRectSize = Vector2.new(28, 28),
		imageRectOffset = Vector2.new(0, 164),
		imageId = sheetIds[1],
	},
	["ic-back"] = {
		name = "ic-back",
		filename = "1x/ic-back.png",
		imageRectSize = Vector2.new(28, 28),
		imageRectOffset = Vector2.new(110, 136),
		imageId = sheetIds[1],
	},
	["ic-body-part-on"] = {
		name = "ic-body-part-on",
		filename = "1x/ic-body-part-on.png",
		imageRectSize = Vector2.new(28, 28),
		imageRectOffset = Vector2.new(138, 136),
		imageId = sheetIds[1],
	},
	["ic-body-part"] = {
		name = "ic-body-part",
		filename = "1x/ic-body-part.png",
		imageRectSize = Vector2.new(28, 28),
		imageRectOffset = Vector2.new(56, 162),
		imageId = sheetIds[1],
	},
	["ic-bundle-on"] = {
		name = "ic-bundle-on",
		filename = "1x/ic-bundle-on.png",
		imageRectSize = Vector2.new(28, 28),
		imageRectOffset = Vector2.new(28, 186),
		imageId = sheetIds[1],
	},
	["ic-bundle"] = {
		name = "ic-bundle",
		filename = "1x/ic-bundle.png",
		imageRectSize = Vector2.new(28, 28),
		imageRectOffset = Vector2.new(0, 192),
		imageId = sheetIds[1],
	},
	["ic-climb-on"] = {
		name = "ic-climb-on",
		filename = "1x/ic-climb-on.png",
		imageRectSize = Vector2.new(28, 28),
		imageRectOffset = Vector2.new(166, 148),
		imageId = sheetIds[1],
	},
	["ic-climb"] = {
		name = "ic-climb",
		filename = "1x/ic-climb.png",
		imageRectSize = Vector2.new(28, 28),
		imageRectOffset = Vector2.new(84, 164),
		imageId = sheetIds[1],
	},
	["ic-close"] = {
		name = "ic-close",
		filename = "1x/ic-close.png",
		imageRectSize = Vector2.new(18, 18),
		imageRectOffset = Vector2.new(232, 0),
		imageId = sheetIds[1],
	},
	["ic-clothing-on"] = {
		name = "ic-clothing-on",
		filename = "1x/ic-clothing-on.png",
		imageRectSize = Vector2.new(28, 28),
		imageRectOffset = Vector2.new(56, 190),
		imageId = sheetIds[1],
	},
	["ic-clothing"] = {
		name = "ic-clothing",
		filename = "1x/ic-clothing.png",
		imageRectSize = Vector2.new(28, 28),
		imageRectOffset = Vector2.new(28, 214),
		imageId = sheetIds[1],
	},
	["ic-color-on"] = {
		name = "ic-color-on",
		filename = "1x/ic-color-on.png",
		imageRectSize = Vector2.new(28, 28),
		imageRectOffset = Vector2.new(0, 220),
		imageId = sheetIds[1],
	},
	["ic-color"] = {
		name = "ic-color",
		filename = "1x/ic-color.png",
		imageRectSize = Vector2.new(28, 28),
		imageRectOffset = Vector2.new(112, 164),
		imageId = sheetIds[1],
	},
	["ic-face-on"] = {
		name = "ic-face-on",
		filename = "1x/ic-face-on.png",
		imageRectSize = Vector2.new(28, 28),
		imageRectOffset = Vector2.new(140, 176),
		imageId = sheetIds[1],
	},
	["ic-face"] = {
		name = "ic-face",
		filename = "1x/ic-face.png",
		imageRectSize = Vector2.new(28, 28),
		imageRectOffset = Vector2.new(168, 176),
		imageId = sheetIds[1],
	},
	["ic-fall-on"] = {
		name = "ic-fall-on",
		filename = "1x/ic-fall-on.png",
		imageRectSize = Vector2.new(28, 28),
		imageRectOffset = Vector2.new(84, 192),
		imageId = sheetIds[1],
	},
	["ic-fall"] = {
		name = "ic-fall",
		filename = "1x/ic-fall.png",
		imageRectSize = Vector2.new(28, 28),
		imageRectOffset = Vector2.new(56, 218),
		imageId = sheetIds[1],
	},
	["ic-front-on"] = {
		name = "ic-front-on",
		filename = "1x/ic-front-on.png",
		imageRectSize = Vector2.new(28, 28),
		imageRectOffset = Vector2.new(28, 242),
		imageId = sheetIds[1],
	},
	["ic-front"] = {
		name = "ic-front",
		filename = "1x/ic-front.png",
		imageRectSize = Vector2.new(28, 28),
		imageRectOffset = Vector2.new(0, 248),
		imageId = sheetIds[1],
	},
	["ic-gear-on"] = {
		name = "ic-gear-on",
		filename = "1x/ic-gear-on.png",
		imageRectSize = Vector2.new(28, 28),
		imageRectOffset = Vector2.new(112, 192),
		imageId = sheetIds[1],
	},
	["ic-gear"] = {
		name = "ic-gear",
		filename = "1x/ic-gear.png",
		imageRectSize = Vector2.new(28, 28),
		imageRectOffset = Vector2.new(196, 178),
		imageId = sheetIds[1],
	},
	["ic-hair-on"] = {
		name = "ic-hair-on",
		filename = "1x/ic-hair-on.png",
		imageRectSize = Vector2.new(28, 28),
		imageRectOffset = Vector2.new(224, 178),
		imageId = sheetIds[1],
	},
	["ic-hair"] = {
		name = "ic-hair",
		filename = "1x/ic-hair.png",
		imageRectSize = Vector2.new(28, 28),
		imageRectOffset = Vector2.new(140, 204),
		imageId = sheetIds[1],
	},
	["ic-hat-on"] = {
		name = "ic-hat-on",
		filename = "1x/ic-hat-on.png",
		imageRectSize = Vector2.new(28, 28),
		imageRectOffset = Vector2.new(168, 204),
		imageId = sheetIds[1],
	},
	["ic-hat"] = {
		name = "ic-hat",
		filename = "1x/ic-hat.png",
		imageRectSize = Vector2.new(28, 28),
		imageRectOffset = Vector2.new(84, 220),
		imageId = sheetIds[1],
	},
	["ic-head-on"] = {
		name = "ic-head-on",
		filename = "1x/ic-head-on.png",
		imageRectSize = Vector2.new(28, 28),
		imageRectOffset = Vector2.new(56, 246),
		imageId = sheetIds[1],
	},
	["ic-head"] = {
		name = "ic-head",
		filename = "1x/ic-head.png",
		imageRectSize = Vector2.new(28, 28),
		imageRectOffset = Vector2.new(28, 270),
		imageId = sheetIds[1],
	},
	["ic-idle-on"] = {
		name = "ic-idle-on",
		filename = "1x/ic-idle-on.png",
		imageRectSize = Vector2.new(28, 28),
		imageRectOffset = Vector2.new(0, 276),
		imageId = sheetIds[1],
	},
	["ic-idle"] = {
		name = "ic-idle",
		filename = "1x/ic-idle.png",
		imageRectSize = Vector2.new(28, 28),
		imageRectOffset = Vector2.new(112, 220),
		imageId = sheetIds[1],
	},
	["ic-jump-on"] = {
		name = "ic-jump-on",
		filename = "1x/ic-jump-on.png",
		imageRectSize = Vector2.new(28, 28),
		imageRectOffset = Vector2.new(196, 206),
		imageId = sheetIds[1],
	},
	["ic-jump"] = {
		name = "ic-jump",
		filename = "1x/ic-jump.png",
		imageRectSize = Vector2.new(28, 28),
		imageRectOffset = Vector2.new(224, 206),
		imageId = sheetIds[1],
	},
	["ic-left-arm-on"] = {
		name = "ic-left-arm-on",
		filename = "1x/ic-left-arm-on.png",
		imageRectSize = Vector2.new(28, 28),
		imageRectOffset = Vector2.new(140, 232),
		imageId = sheetIds[1],
	},
	["ic-left-arm"] = {
		name = "ic-left-arm",
		filename = "1x/ic-left-arm.png",
		imageRectSize = Vector2.new(28, 28),
		imageRectOffset = Vector2.new(168, 232),
		imageId = sheetIds[1],
	},
	["ic-left-leg-on"] = {
		name = "ic-left-leg-on",
		filename = "1x/ic-left-leg-on.png",
		imageRectSize = Vector2.new(28, 28),
		imageRectOffset = Vector2.new(84, 248),
		imageId = sheetIds[1],
	},
	["ic-left-leg"] = {
		name = "ic-left-leg",
		filename = "1x/ic-left-leg.png",
		imageRectSize = Vector2.new(28, 28),
		imageRectOffset = Vector2.new(56, 274),
		imageId = sheetIds[1],
	},
	["ic-neck-on"] = {
		name = "ic-neck-on",
		filename = "1x/ic-neck-on.png",
		imageRectSize = Vector2.new(28, 28),
		imageRectOffset = Vector2.new(28, 298),
		imageId = sheetIds[1],
	},
	["ic-neck"] = {
		name = "ic-neck",
		filename = "1x/ic-neck.png",
		imageRectSize = Vector2.new(28, 28),
		imageRectOffset = Vector2.new(0, 304),
		imageId = sheetIds[1],
	},
	["ic-pants-on"] = {
		name = "ic-pants-on",
		filename = "1x/ic-pants-on.png",
		imageRectSize = Vector2.new(28, 28),
		imageRectOffset = Vector2.new(112, 248),
		imageId = sheetIds[1],
	},
	["ic-pants"] = {
		name = "ic-pants",
		filename = "1x/ic-pants.png",
		imageRectSize = Vector2.new(28, 28),
		imageRectOffset = Vector2.new(196, 234),
		imageId = sheetIds[1],
	},
	["ic-recent-on"] = {
		name = "ic-recent-on",
		filename = "1x/ic-recent-on.png",
		imageRectSize = Vector2.new(28, 28),
		imageRectOffset = Vector2.new(224, 234),
		imageId = sheetIds[1],
	},
	["ic-recent"] = {
		name = "ic-recent",
		filename = "1x/ic-recent.png",
		imageRectSize = Vector2.new(28, 28),
		imageRectOffset = Vector2.new(140, 260),
		imageId = sheetIds[1],
	},
	["ic-right-arm-on"] = {
		name = "ic-right-arm-on",
		filename = "1x/ic-right-arm-on.png",
		imageRectSize = Vector2.new(28, 28),
		imageRectOffset = Vector2.new(168, 260),
		imageId = sheetIds[1],
	},
	["ic-right-arm"] = {
		name = "ic-right-arm",
		filename = "1x/ic-right-arm.png",
		imageRectSize = Vector2.new(28, 28),
		imageRectOffset = Vector2.new(84, 276),
		imageId = sheetIds[1],
	},
	["ic-right-leg-on"] = {
		name = "ic-right-leg-on",
		filename = "1x/ic-right-leg-on.png",
		imageRectSize = Vector2.new(28, 28),
		imageRectOffset = Vector2.new(56, 302),
		imageId = sheetIds[1],
	},
	["ic-right-leg"] = {
		name = "ic-right-leg",
		filename = "1x/ic-right-leg.png",
		imageRectSize = Vector2.new(28, 28),
		imageRectOffset = Vector2.new(28, 326),
		imageId = sheetIds[1],
	},
	["ic-run-on"] = {
		name = "ic-run-on",
		filename = "1x/ic-run-on.png",
		imageRectSize = Vector2.new(28, 28),
		imageRectOffset = Vector2.new(0, 332),
		imageId = sheetIds[1],
	},
	["ic-run"] = {
		name = "ic-run",
		filename = "1x/ic-run.png",
		imageRectSize = Vector2.new(28, 28),
		imageRectOffset = Vector2.new(112, 276),
		imageId = sheetIds[1],
	},
	["ic-scaling-on"] = {
		name = "ic-scaling-on",
		filename = "1x/ic-scaling-on.png",
		imageRectSize = Vector2.new(28, 28),
		imageRectOffset = Vector2.new(196, 262),
		imageId = sheetIds[1],
	},
	["ic-scaling"] = {
		name = "ic-scaling",
		filename = "1x/ic-scaling.png",
		imageRectSize = Vector2.new(28, 28),
		imageRectOffset = Vector2.new(224, 262),
		imageId = sheetIds[1],
	},
	["ic-shirt-on"] = {
		name = "ic-shirt-on",
		filename = "1x/ic-shirt-on.png",
		imageRectSize = Vector2.new(28, 28),
		imageRectOffset = Vector2.new(140, 288),
		imageId = sheetIds[1],
	},
	["ic-shirt"] = {
		name = "ic-shirt",
		filename = "1x/ic-shirt.png",
		imageRectSize = Vector2.new(28, 28),
		imageRectOffset = Vector2.new(168, 288),
		imageId = sheetIds[1],
	},
	["ic-shoulder-on"] = {
		name = "ic-shoulder-on",
		filename = "1x/ic-shoulder-on.png",
		imageRectSize = Vector2.new(28, 28),
		imageRectOffset = Vector2.new(84, 304),
		imageId = sheetIds[1],
	},
	["ic-shoulder"] = {
		name = "ic-shoulder",
		filename = "1x/ic-shoulder.png",
		imageRectSize = Vector2.new(28, 28),
		imageRectOffset = Vector2.new(56, 330),
		imageId = sheetIds[1],
	},
	["ic-swim-on"] = {
		name = "ic-swim-on",
		filename = "1x/ic-swim-on.png",
		imageRectSize = Vector2.new(28, 28),
		imageRectOffset = Vector2.new(28, 354),
		imageId = sheetIds[1],
	},
	["ic-swim"] = {
		name = "ic-swim",
		filename = "1x/ic-swim.png",
		imageRectSize = Vector2.new(28, 28),
		imageRectOffset = Vector2.new(0, 360),
		imageId = sheetIds[1],
	},
	["ic-torso-on"] = {
		name = "ic-torso-on",
		filename = "1x/ic-torso-on.png",
		imageRectSize = Vector2.new(28, 28),
		imageRectOffset = Vector2.new(112, 304),
		imageId = sheetIds[1],
	},
	["ic-torso"] = {
		name = "ic-torso",
		filename = "1x/ic-torso.png",
		imageRectSize = Vector2.new(28, 28),
		imageRectOffset = Vector2.new(196, 290),
		imageId = sheetIds[1],
	},
	["ic-tshirt-on"] = {
		name = "ic-tshirt-on",
		filename = "1x/ic-tshirt-on.png",
		imageRectSize = Vector2.new(28, 28),
		imageRectOffset = Vector2.new(224, 290),
		imageId = sheetIds[1],
	},
	["ic-tshirt"] = {
		name = "ic-tshirt",
		filename = "1x/ic-tshirt.png",
		imageRectSize = Vector2.new(28, 28),
		imageRectOffset = Vector2.new(140, 316),
		imageId = sheetIds[1],
	},
	["ic-up-black"] = {
		name = "ic-up-black",
		filename = "1x/ic-up-black.png",
		imageRectSize = Vector2.new(28, 28),
		imageRectOffset = Vector2.new(168, 316),
		imageId = sheetIds[1],
	},
	["ic-waist-on"] = {
		name = "ic-waist-on",
		filename = "1x/ic-waist-on.png",
		imageRectSize = Vector2.new(28, 28),
		imageRectOffset = Vector2.new(84, 332),
		imageId = sheetIds[1],
	},
	["ic-waist"] = {
		name = "ic-waist",
		filename = "1x/ic-waist.png",
		imageRectSize = Vector2.new(28, 28),
		imageRectOffset = Vector2.new(56, 358),
		imageId = sheetIds[1],
	},
	["ic-walk-on"] = {
		name = "ic-walk-on",
		filename = "1x/ic-walk-on.png",
		imageRectSize = Vector2.new(28, 28),
		imageRectOffset = Vector2.new(28, 382),
		imageId = sheetIds[1],
	},
	["ic-walk"] = {
		name = "ic-walk",
		filename = "1x/ic-walk.png",
		imageRectSize = Vector2.new(28, 28),
		imageRectOffset = Vector2.new(0, 388),
		imageId = sheetIds[1],
	},
	["img-selector-box"] = {
		name = "img-selector-box",
		filename = "1x/img-selector-box.png",
		imageRectSize = Vector2.new(56, 56),
		imageRectOffset = Vector2.new(0, 28),
		imageId = sheetIds[1],
	},
}

--
-- api
local flagManagerModule = script.Parent and script.Parent:FindFirstChild'FlagManager'

return {
	enabled = flagManagerModule and require(flagManagerModule).AvatarEditorUsesSpriteSheets or false,
	all = function()
		return sprites
	end,
	info = function(name)
		return sprites[name]
	end,
	equip = function(gui, name) local p = function(...) if name:find('gr-circle') then print(...) end end
		local scale
		local scaleSuffix = name:match('@%dx')
		if scaleSuffix then
			scale = scaleSuffix:sub(2,-2)
			name = name:gsub(scaleSuffix, '')
		end
		
		if sprites[name] then
			local content = sprites[name].imageId
			if scale then
				-- example input: rbxasset://textures/AvatarEditorIcons/Sheet1.png
				-- example scale: 2
				content =
					content:match('^.+%.'):sub(1,-2).. -- captures: rbxasset://textures/AvatarEditorIcons/Sheet1
					'@'..scale..'x'.. -- captures: @2x
					content:match('%..+$') -- captures: .png
				-- example output: rbxasset://textures/AvatarEditorIcons/Sheet1@2x.png
			end
			
			gui.ImageRectSize = sprites[name].imageRectSize * (scale or 1)
			gui.ImageRectOffset = sprites[name].imageRectOffset * (scale or 1)
			gui.Image = content
		else
			warn('BAD SPRITE NAME: "'..tostring(name)..'".  Tried to equip to '..gui:GetFullName())
			gui.ImageRectSize = Vector2.new(0, 0)
			gui.ImageRectOffset = Vector2.new(0, 0)
			gui.Image = 'rbxasset://textures/ui/ErrorIcon.png'
		end
	end
}