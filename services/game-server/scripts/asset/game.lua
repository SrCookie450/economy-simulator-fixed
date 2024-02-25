local assetId = {1234};
local jobId = "InsertJobIdHere";
local mode = "R6";
local baseURL = "http://economy-simulator.org";
local uploadURL = "UPLOAD_URL_HERE";
local ScriptContext = game:GetService("ScriptContext");
local Lighting = game:GetService('Lighting');
local RunService = game:GetService('RunService');
local ContentProvider = game:GetService('ContentProvider');
local HttpService = game:GetService("HttpService");
local ThumbnailGenerator = game:GetService('ThumbnailGenerator');
local Players = game:GetService("Players");
game:GetService('StarterGui'):SetCoreGuiEnabled(Enum.CoreGuiType.All, false);
game:GetService('ThumbnailGenerator').GraphicsMode = 2;
HttpService.HttpEnabled = true;
ScriptContext.ScriptsDisabled = true
Lighting.Outlines = false
ContentProvider:SetBaseUrl('http://economy-simulator.org')
print(ContentProvider.BaseUrl)
game:GetService("ContentProvider"):SetAssetUrl(baseURL .. "/asset/")
game:GetService("InsertService"):SetAssetUrl(baseURL .. "/asset/?id=%d")
pcall(function() game:GetService("ScriptInformationProvider"):SetAssetUrl(url .. "/asset/") end)
game:GetService("ContentProvider"):SetBaseUrl(baseURL .. "/")
Players:SetChatFilterUrl(baseURL .. "/Game/ChatFilter.ashx")
local Insert = game:GetService("InsertService")
game:GetService("InsertService"):SetAssetUrl(baseURL .. "/asset/?id=%d")
game:GetService("InsertService"):SetAssetVersionUrl(baseURL .. "/asset/?assetversionid=%d")

local didRun = false
local assetId = assetId[1];

    local function render()
        didRun = true
        print("[debug] render game thumbnail")
        local encoded = ThumbnailGenerator:Click('png', _X_RES_, _Y_RES_, false, true)
        print("[debug] send post request containing game")

        local ok, data = pcall(function()
            HttpService.HttpEnabled = true;
            return HttpService:PostAsync(uploadURL, HttpService:JSONEncode({
                ['type'] = 'Asset',
                ['assetId'] = assetId,
                ['thumbnail'] = encoded,
                ['accessKey'] = "AccessKey",
                ['jobId'] = jobId,
            }), Enum.HttpContentType.TextPlain)
        end)
        print("[debug] post over",ok,data)
    end

    local ok, data = pcall(function()
        delay(5, function()
            if didRun == false then
                render()
            end
        end)
        print("load game...")
        local s,e = pcall(function()
            game:Load(baseURL .. "/asset/?id=" .. assetId)
        end)
        print("game load over")
    end)
    print(ok, data);
    print("[debug] exit game");
