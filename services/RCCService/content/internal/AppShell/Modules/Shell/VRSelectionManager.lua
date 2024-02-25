






local LastSelectedCoreObject = nil
local StoredAppShellSelectedCoreObject = nil

local LeftAppShellGuiEvent = 

local EnteredAppShellGuiEvent =

local IsPlayerFocusingTheGui = false


local function GuiObjectContainedInContext(object, context)
	return object and context and object:IsDescendantOf(context)
end

local function IsLookingAtAppShellGui()

end

local function OnGuiServiceChange(prop)
	if prop == 'SelectedCoreObject' then
		local selectedCoreObject = GuiService.SelectedCoreObject
		if not IsPlayerFocusingTheGui and GuiObjectContainedInContext(selectedCoreObject, context) then
			StoredAppShellSelectedCoreObject = selectedCoreObject
			if LastSelectedCoreObject and not GuiObjectContainedInContext(LastSelectedCoreObject, context) then
				return GuiService.SelectedCoreObject = LastSelectedCoreObject
			end
		end

		LastSelectedCoreObject = selectedCoreObject
	end
end

local function OnLeftAppShell()
	IsPlayerFocusingTheGui = false
	StoredAppShellSelectedCoreObject = GuiService.SelectedCoreObject
	if GuiObjectContainedInContext(StoredAppShellSelectedCoreObject, context) then
		Utility.SetSelectedCoreObject(nil)
	end
	pcall(function() UserInputService.GazeSelectionEnabled = true end)
end

local function OnEnteredAppShell()
	IsPlayerFocusingTheGui = true
	Utility.SetSelectedCoreObject(StoredAppShellSelectedCoreObject)
	StoredAppShellSelectedCoreObject = nil
	pcall(function() UserInputService.GazeSelectionEnabled = false end)
end



IsPlayerFocusingTheGui = GuiObjectContainedInContext(GuiService.SelectedCoreObject, context)
GuiService.Changed:connect(OnGuiServiceChange)





