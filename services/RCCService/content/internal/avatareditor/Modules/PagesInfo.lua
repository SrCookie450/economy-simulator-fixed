local flagManager = require(script.Parent.FlagManager)

local legsZoomCFrame = CFrame.new(0, 0.75, -8)
local faceZoomCFrame = CFrame.new(0, 4.1, -10) * CFrame.Angles(math.rad(10), 0, 0)
local armsZoomCFrame = CFrame.new(0, 2.5, -8)
local headWideZoomCFrame = CFrame.new(0, 3.5, -6)

local scalingOnlyWorksForR15Text = 'Scaling only works\nfor R15 avatars'
local animationsOnlyWorkForR15Text = 'Animations only work\nfor R15 avatars'

local recentPage = {name = 'Recent All',
	iconImage =			'rbxasset://textures/AvatarEditorIcons/PageIcons/Category/ic-all.png',
	iconImageSelected =	'rbxasset://textures/AvatarEditorIcons/PageIcons/Category/ic-all-on.png',
	iconImageName =		'ic-all',
	iconImageSelectedName = 'ic-all-on'
}

local recentClothingPage = {name = 'Recent Clothing',
	iconImage =			'rbxasset://textures/AvatarEditorIcons/PageIcons/Category/ic-clothing.png',
	iconImageSelected =	'rbxasset://textures/AvatarEditorIcons/PageIcons/Category/ic-clothing-on.png',
	iconImageName = 'ic-clothing',
	iconImageSelectedName = 'ic-clothing-on'
}
local recentBodyPage = {name = 'Recent Body',
	iconImage =			'rbxasset://textures/AvatarEditorIcons/PageIcons/Category/ic-body-part.png',
	iconImageSelected =	'rbxasset://textures/AvatarEditorIcons/PageIcons/Category/ic-body-part-on.png',
	iconImageName = 'ic-body-part',
	iconImageSelectedName = 'ic-body-part-on',
}
local recentAnimationPage = {name = 'Recent Animation',
	iconImage =			'rbxasset://textures/AvatarEditorIcons/PageIcons/Category/ic-avatar-animation.png',
	iconImageSelected =	'rbxasset://textures/AvatarEditorIcons/PageIcons/Category/ic-avatar-animation-on.png',
	iconImageName = 'ic-avatar-animation',
	iconImageSelectedName = 'ic-avatar-animation-on'
}


local outfitsPage = {name = 'Outfits',			--outfits will include packages in some way
	iconImage =			'rbxasset://textures/AvatarEditorIcons/ic-bundle.png',
	iconImageSelected =	'rbxasset://textures/AvatarEditorIcons/ic-bundle-white.png',
	iconImageName = 'ic-bundle',
	iconImageSelectedName = 'ic-bundle-on',
	--special = true,
	infiniteScrolling = true,
}
local hatsPage = {name = 'Hats',
	typeName = 'Hat',
	iconImage =			'rbxasset://textures/AvatarEditorIcons/ic-hat.png',
	iconImageSelected =	'rbxasset://textures/AvatarEditorIcons/ic-hat-white.png',
	iconImageName = 'ic-hat',
	iconImageSelectedName = 'ic-hat-on',
	infiniteScrolling = true,
	CameraCFrameOffset = headWideZoomCFrame
}
local hairPage = {name = 'Hair',
	typeName = 'Hair Accessory',
	iconImage =			'rbxasset://textures/AvatarEditorIcons/PageIcons/Clothing/ic-hair.png',
	iconImageSelected =	'rbxasset://textures/AvatarEditorIcons/PageIcons/Clothing/ic-hair-on.png',
	iconImageName = 'ic-hair',
	iconImageSelectedName = 'ic-hair-on',
	infiniteScrolling = true,
	CameraCFrameOffset = headWideZoomCFrame
}
local faceAccessoryPage = {name = 'Face Accessories',
	typeName = 'Face Accessory',
	iconImage =			'rbxasset://textures/AvatarEditorIcons/PageIcons/Clothing/ic-face.png',
	iconImageSelected =	'rbxasset://textures/AvatarEditorIcons/PageIcons/Clothing/ic-face-on.png',
	iconImageName = 'ic-face',
	iconImageSelectedName = 'ic-face-on',
	infiniteScrolling = true,
	CameraCFrameOffset = faceZoomCFrame
}
local neckAccessoryPage = {name = 'Neck Accessories',
	typeName = 'Neck Accessory',
	iconImage =			'rbxasset://textures/AvatarEditorIcons/PageIcons/Clothing/ic-neck.png',
	iconImageSelected =	'rbxasset://textures/AvatarEditorIcons/PageIcons/Clothing/ic-neck-on.png',
	iconImageName = 'ic-neck',
	iconImageSelectedName = 'ic-neck-on',
	infiniteScrolling = true,
	CameraCFrameOffset = armsZoomCFrame:lerp(faceZoomCFrame, 0.5)
}
local shoulderAccessoryPage = {name = 'Shoulder Accessories',
	typeName = 'Shoulder Accessory',
	iconImage =			'rbxasset://textures/AvatarEditorIcons/PageIcons/Clothing/ic-shoulder.png',
	iconImageSelected =	'rbxasset://textures/AvatarEditorIcons/PageIcons/Clothing/ic-shoulder-on.png',
	iconImageName = 'ic-shoulder',
	iconImageSelectedName = 'ic-shoulder-on',
	infiniteScrolling = true,
	CameraCFrameOffset = armsZoomCFrame:lerp(faceZoomCFrame, 0.5)
}
local frontAccessoryPage = {name = 'Front Accessories',
	typeName = 'Front Accessory',
	iconImage =			'rbxasset://textures/AvatarEditorIcons/PageIcons/Clothing/ic-front.png',
	iconImageSelected =	'rbxasset://textures/AvatarEditorIcons/PageIcons/Clothing/ic-front-on.png',
	iconImageName = 'ic-front',
	iconImageSelectedName = 'ic-front-on',
	infiniteScrolling = true,
	CameraCFrameOffset = armsZoomCFrame
}
local backAccessoryPage = {name = 'Back Accessories',
	typeName = 'Back Accessory',
	iconImage =			'rbxasset://textures/AvatarEditorIcons/PageIcons/Clothing/ic-back.png',
	iconImageSelected =	'rbxasset://textures/AvatarEditorIcons/PageIcons/Clothing/ic-back-on.png',
	iconImageName = 'ic-back',
	iconImageSelectedName = 'ic-back-on',
	infiniteScrolling = true,
	CameraCFrameOffset = armsZoomCFrame
}
local waistAccessoryPage = {name = 'Waist Accessories',
	typeName = 'Waist Accessory',
	iconImage =			'rbxasset://textures/AvatarEditorIcons/PageIcons/Clothing/ic-waist.png',
	iconImageSelected =	'rbxasset://textures/AvatarEditorIcons/PageIcons/Clothing/ic-waist-on.png',
	iconImageName = 'ic-waist',
	iconImageSelectedName = 'ic-waist-on',
	infiniteScrolling = true,
	CameraCFrameOffset = armsZoomCFrame:lerp(legsZoomCFrame, 0.5)
}
local shirtsPage = {name = 'Shirts',
	typeName = 'Shirt',
	iconImage =			'rbxasset://textures/AvatarEditorIcons/ic-tshirt.png',
	iconImageSelected =	'rbxasset://textures/AvatarEditorIcons/ic-tshirt-white.png',
	iconImageName = 'ic-tshirt',
	iconImageSelectedName = 'ic-tshirt-on',
	infiniteScrolling = true,
	CameraCFrameOffset = armsZoomCFrame
}
local pantsPage = {name = 'Pants',
	typeName = 'Pants',
	iconImage =			'rbxasset://textures/AvatarEditorIcons/ic-pant.png',
	iconImageSelected =	'rbxasset://textures/AvatarEditorIcons/ic-pant-white.png',
	iconImageName = 'ic-pants',
	iconImageSelectedName = 'ic-pants-on',
	infiniteScrolling = true,
	CameraCFrameOffset = legsZoomCFrame
}
local facesPage = {name = 'Faces',
	typeName = 'Face',
	iconImage =			'rbxasset://textures/AvatarEditorIcons/ic-face.png',
	iconImageSelected =	'rbxasset://textures/AvatarEditorIcons/ic-face-white.png',
	iconImageName = 'ic-face',
	iconImageSelectedName = 'ic-face-on',
	infiniteScrolling = true,
	CameraCFrameOffset = faceZoomCFrame
}
local headsPage = {name = 'Heads',
	typeName = 'Head',
	iconImage =			'rbxasset://textures/AvatarEditorIcons/ic-head.png',
	iconImageSelected =	'rbxasset://textures/AvatarEditorIcons/ic-head-white.png',
	iconImageName = 'ic-head',
	iconImageSelectedName = 'ic-head-on',
	infiniteScrolling = true,
	CameraCFrameOffset = faceZoomCFrame
}
local torsosPage = {name = 'Torsos',
	typeName = 'Torso',
	iconImage =			'rbxasset://textures/AvatarEditorIcons/ic-torso.png',
	iconImageSelected =	'rbxasset://textures/AvatarEditorIcons/ic-torso-white.png',
	iconImageName = 'ic-torso',
	iconImageSelectedName = 'ic-torso-on',
	infiniteScrolling = true,
	CameraCFrameOffset = armsZoomCFrame
}
local rightArmsPage = {name = 'Right Arms',
	typeName = 'RightArm',
	iconImage =			'rbxasset://textures/AvatarEditorIcons/ic-rightarm.png',
	iconImageSelected =	'rbxasset://textures/AvatarEditorIcons/ic-rightarm-white.png',
	iconImageName = 'ic-right-arm',
	iconImageSelectedName = 'ic-right-arm-on',
	infiniteScrolling = true,
	CameraCFrameOffset = armsZoomCFrame
}
local leftArmsPage = {name = 'Left Arms',
	typeName = 'LeftArm',
	iconImage =			'rbxasset://textures/AvatarEditorIcons/ic-leftarm.png',
	iconImageSelected =	'rbxasset://textures/AvatarEditorIcons/ic-leftarm-white.png',
	iconImageName = 'ic-left-arm',
	iconImageSelectedName = 'ic-left-arm-on',
	infiniteScrolling = true,
	CameraCFrameOffset = armsZoomCFrame
}
local rightLegsPage = {name = 'Right Legs',
	typeName = 'RightLeg',
	iconImage =			'rbxasset://textures/AvatarEditorIcons/ic-rightleg.png',
	iconImageSelected =	'rbxasset://textures/AvatarEditorIcons/ic-rightleg-white.png',
	iconImageName = 'ic-right-leg',
	iconImageSelectedName = 'ic-right-leg-on',
	infiniteScrolling = true,
	CameraCFrameOffset = legsZoomCFrame
}
local leftLegsPage = {name = 'Left Legs',
	typeName = 'LeftLeg',
	iconImage =			'rbxasset://textures/AvatarEditorIcons/ic-leftleg.png',
	iconImageSelected =	'rbxasset://textures/AvatarEditorIcons/ic-leftleg-white.png',
	iconImageName = 'ic-left-leg',
	iconImageSelectedName = 'ic-left-leg-on',
	infiniteScrolling = true,
	CameraCFrameOffset = legsZoomCFrame
}
local gearPage = {name = 'Gear',
	typeName = 'Gear',
	iconImage =			'rbxasset://textures/AvatarEditorIcons/ic-gear.png',
	iconImageSelected =	'rbxasset://textures/AvatarEditorIcons/ic-gear-white.png',
	iconImageName = 'ic-gear',
	iconImageSelectedName = 'ic-gear-on',
	infiniteScrolling = true,
}
local skinTonePage = {name = 'Skin Tone',
	iconImage =			'rbxasset://textures/AvatarEditorIcons/ic-color.png',
	iconImageSelected =	'rbxasset://textures/AvatarEditorIcons/ic-color-filled.png',
	iconImageName = 'ic-color',
	iconImageSelectedName = 'ic-color-on',
	special = true,
}
local scalePage = {name = 'Scale',
	iconImage =			'rbxasset://textures/AvatarEditorIcons/ic-scale@2x.png',
	iconImageSelected =	'rbxasset://textures/AvatarEditorIcons/ic-scale-filled.png',
	iconImageName = 'ic-scaling@2x',
	iconImageSelectedName = 'ic-scaling-on',
	special = true,
	r15only = true,
	r15onlyMessage = scalingOnlyWorksForR15Text
}

local climbAnimPage = {name = 'Climb Animations',
	typeName = 'ClimbAnimation',
	iconImage =			'rbxasset://textures/AvatarEditorIcons/PageIcons/Avatar-Animation/ic-climb.png',
	iconImageSelected =	'rbxasset://textures/AvatarEditorIcons/PageIcons/Avatar-Animation/ic-climb-on.png',
	iconImageName = 'ic-climb',
	iconImageSelectedName = 'ic-climb-on',
	infiniteScrolling = true,
	r15only = true,
	r15onlyMessage = animationsOnlyWorkForR15Text
}
local jumpAnimPage = {name = 'Jump Animations',
	typeName = 'JumpAnimation',
	iconImage =			'rbxasset://textures/AvatarEditorIcons/PageIcons/Avatar-Animation/ic-jump.png',
	iconImageSelected =	'rbxasset://textures/AvatarEditorIcons/PageIcons/Avatar-Animation/ic-jump-on.png',
	iconImageName = 'ic-jump',
	iconImageSelectedName = 'ic-jump-on',
	infiniteScrolling = true,
	r15only = true,
	r15onlyMessage = animationsOnlyWorkForR15Text
}
local fallAnimPage = {name = 'Fall Animations',
	typeName = 'FallAnimation',
	iconImage =			'rbxasset://textures/AvatarEditorIcons/PageIcons/Avatar-Animation/ic-fall.png',
	iconImageSelected =	'rbxasset://textures/AvatarEditorIcons/PageIcons/Avatar-Animation/ic-fall-on.png',
	iconImageName = 'ic-fall',
	iconImageSelectedName = 'ic-fall-on',
	infiniteScrolling = true,
	r15only = true,
	r15onlyMessage = animationsOnlyWorkForR15Text
}
local idleAnimPage = {name = 'Idle Animations',
	typeName = 'IdleAnimation',
	iconImage =			'rbxasset://textures/AvatarEditorIcons/PageIcons/Avatar-Animation/ic-idle.png',
	iconImageSelected =	'rbxasset://textures/AvatarEditorIcons/PageIcons/Avatar-Animation/ic-idle-on.png',
	iconImageName = 'ic-idle',
	iconImageSelectedName = 'ic-idle-on',
	infiniteScrolling = true,
	r15only = true,
	r15onlyMessage = animationsOnlyWorkForR15Text
}
local walkAnimPage = {name = 'Walk Animations',
	typeName = 'WalkAnimation',
	iconImage =			'rbxasset://textures/AvatarEditorIcons/PageIcons/Avatar-Animation/ic-walk.png',
	iconImageSelected =	'rbxasset://textures/AvatarEditorIcons/PageIcons/Avatar-Animation/ic-walk-on.png',
	iconImageName = 'ic-walk',
	iconImageSelectedName = 'ic-walk-on',
	infiniteScrolling = true,
	r15only = true,
	r15onlyMessage = animationsOnlyWorkForR15Text
}
local runAnimPage = {name = 'Run Animations',
	typeName = 'RunAnimation',
	iconImage =			'rbxasset://textures/AvatarEditorIcons/PageIcons/Avatar-Animation/ic-run.png',
	iconImageSelected =	'rbxasset://textures/AvatarEditorIcons/PageIcons/Avatar-Animation/ic-run-on.png',
	iconImageName = 'ic-run',
	iconImageSelectedName = 'ic-run-on',
	infiniteScrolling = true,
	r15only = true,
	r15onlyMessage = animationsOnlyWorkForR15Text
}
local swimAnimPage = {name = 'Swim Animations',
	typeName = 'SwimAnimation',
	iconImage =			'rbxasset://textures/AvatarEditorIcons/PageIcons/Avatar-Animation/ic-swim.png',
	iconImageSelected =	'rbxasset://textures/AvatarEditorIcons/PageIcons/Avatar-Animation/ic-swim-on.png',
	iconImageName = 'ic-swim',
	iconImageSelectedName = 'ic-swim-on',
	infiniteScrolling = true,
	r15only = true,
	r15onlyMessage = animationsOnlyWorkForR15Text
}


local allPages = {
	recentPage,
	outfitsPage,
	hatsPage,
	hairPage,
	faceAccessoryPage,
	neckAccessoryPage,
	shoulderAccessoryPage,
	frontAccessoryPage,
	backAccessoryPage,
	waistAccessoryPage,
	shirtsPage,
	pantsPage,
	facesPage,
	headsPage,
	torsosPage,
	rightArmsPage,
	leftArmsPage,
	rightLegsPage,
	leftLegsPage,
	gearPage,
	skinTonePage,
	climbAnimPage,
	jumpAnimPage,
	fallAnimPage,
	idleAnimPage,
	walkAnimPage,
	runAnimPage,
	swimAnimPage
}

local recentCategory = {name = 'Recent',
	iconImage = 'rbxasset://textures/AvatarEditorIcons/PageIcons/Category/ic-recent.png',
	selectedIconImage = 'rbxasset://textures/AvatarEditorIcons/PageIcons/Category/ic-recent-on.png',
	iconImageName = 'ic-recent',
	selectedIconImageName = 'ic-recent-on',
	pages = {recentPage, recentClothingPage, recentBodyPage},
}
local clothingCategory = {name = 'Clothing',
	iconImage = 'rbxasset://textures/AvatarEditorIcons/PageIcons/Category/ic-clothing.png',
	selectedIconImage = 'rbxasset://textures/AvatarEditorIcons/PageIcons/Category/ic-clothing-on.png',
	iconImageName = 'ic-clothing',
	selectedIconImageName = 'ic-clothing-on',
	pages = {hatsPage, shirtsPage, pantsPage, hairPage, faceAccessoryPage, neckAccessoryPage, shoulderAccessoryPage, frontAccessoryPage, backAccessoryPage, waistAccessoryPage, gearPage,},
}
local bodyCategory = {name = 'Body',
	iconImage = 'rbxasset://textures/AvatarEditorIcons/PageIcons/Category/ic-body-part.png',
	selectedIconImage = 'rbxasset://textures/AvatarEditorIcons/PageIcons/Category/ic-body-part-on.png',
	iconImageName = 'ic-body-part',
	selectedIconImageName = 'ic-body-part-on',
	pages = {outfitsPage, facesPage, headsPage, torsosPage, rightArmsPage, leftArmsPage, rightLegsPage, leftLegsPage, skinTonePage,},
}
local animationCategory = {name = 'Animation',
	iconImage = 'rbxasset://textures/AvatarEditorIcons/PageIcons/Category/ic-avatar-animation.png',
	selectedIconImage = 'rbxasset://textures/AvatarEditorIcons/PageIcons/Category/ic-avatar-animation-on.png',
	iconImageName = 'ic-avatar-animation',
	selectedIconImageName = 'ic-avatar-animation-on',
--	pages = {climbAnimPage, jumpAnimPage, fallAnimPage, idleAnimPage, walkAnimPage, runAnimPage, swimAnimPage}
	pages = {idleAnimPage, walkAnimPage, runAnimPage, jumpAnimPage, fallAnimPage, climbAnimPage, swimAnimPage}
}

local categories = {
	recentCategory,
	clothingCategory,
	bodyCategory,
}

do
	local enabledAvatarScalePage = false
	local enabledAvatarScalePage = flagManager.EnabledAvatarScalePage
	
	if enabledAvatarScalePage then
		table.insert(allPages, scalePage)
		table.insert(bodyCategory.pages, scalePage)
	end
end


do
	local enabledAvatarAnimationCategory = false
	local enabledAvatarAnimationCategory = flagManager.EnabledAvatarAnimationCategory
	
	if enabledAvatarAnimationCategory then
		table.insert(categories, animationCategory)
		table.insert(recentCategory.pages, recentAnimationPage)
	end
end


local enabledAvatarEditorV2 = false
do
	enabledAvatarEditorV2 = flagManager.EnabledAvatarEditorV2
end


if enabledAvatarEditorV2 then		--todo: enable this code before shipping
	return categories
else
	return allPages
end












