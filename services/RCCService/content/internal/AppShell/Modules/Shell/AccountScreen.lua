--[[
			// AccountPage.lua
]]
local CoreGui = Game:GetService("CoreGui")
local GuiRoot = CoreGui:FindFirstChild("RobloxGui")
local Modules = GuiRoot:FindFirstChild("Modules")
local ShellModules = Modules:FindFirstChild("Shell")

local UserData = require(ShellModules:FindFirstChild('UserData'))
local Strings = require(ShellModules:FindFirstChild('LocalizedStrings'))

local SetAccountCredentialsScreen = require(ShellModules:FindFirstChild('SetAccountCredentialsScreen'))
local UnlinkAccountScreen = require(ShellModules:FindFirstChild('UnlinkAccountScreen'))

-- This is an empty page that is a place holder. Account page changes depending on cases
local function createAccountScreen()
	local hasLinkedAccount = UserData:HasLinkedAccount()
	local hasRobloxCredentials = UserData:HasRobloxCredentials()

	-- Cases
	-- 1. Has roblox credentials, which implies they have a linked account
	-- 2. No credentials but a linked account
	-- 3. No Credentials/No Linked account - this should never happen, but cover it
	-- 4. One of these calls has a web error, result will be nil in that case

	-- Legacy
	-- Old signup flow could put a user in a state where they were linked but did not set credentials.
	-- We need to continue to check this until all accounts in this state have been cleaned up. When this
	-- happens, this should just return the unlink screen, or rather the caller can just request an unlink screen

	local this = nil

	if hasRobloxCredentials ~= nil and hasLinkedAccount ~= nil then
		if hasRobloxCredentials == true then
			this = UnlinkAccountScreen()
		elseif hasLinkedAccount == true and hasRobloxCredentials == false then
			this = SetAccountCredentialsScreen(Strings:LocalizedString("SignUpTitle"),
				Strings:LocalizedString("SignUpPhrase"), Strings:LocalizedString("SignUpWord"))
		end
	end

	return this
end

return createAccountScreen
