local TEST_MODE_IN_PROD_STUDIO = true

--
--
local testMode = false
if TEST_MODE_IN_PROD_STUDIO and game["Run Service"]:IsStudio() and not pcall(settings) then
	testMode = true
end

local flags = {
	EnabledAvatarEditorV2 = false,
	AvatarEditorUsesSpriteSheets = false,
	EnabledAvatarScalePage = false,
	EnabledAvatarAnimationCategory = false,
	AvatarEditorUsesNewAssetGetEndpoint = false,
	AvatarEditorAmendsRigsWhenAppearanceChanges = false,
	AvatarEditorDisplaysWarningOnR15OnlyPages = false,
	AvatarEditorSpinCounter = false,
	AvatarEditorCameraZoomingEnabled = false,
}

local testStates = {
	EnabledAvatarEditorV2 = true,
	AvatarEditorUsesSpriteSheets = true,
	EnabledAvatarScalePage = true,
	EnabledAvatarAnimationCategory = true,
	AvatarEditorUsesNewAssetGetEndpoint = true,
	AvatarEditorAmendsRigsWhenAppearanceChanges = true,
	AvatarEditorDisplaysWarningOnR15OnlyPages = true,
	AvatarEditorSpinCounter = true,
	AvatarEditorCameraZoomingEnabled = true,
}

--
--
local this = {}

if testMode then
	for name, default in next, flags do
		this[name] = testStates[name] or default
	end
else
	for name, default in next, flags do
		pcall(function()
			this[name] = settings():GetFFlag(name)
		end)
	end
end

--
--
return this