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

    local function render()
        local model = Instance.new("Model");
        for _, assetId in pairs(assetId) do
            local asset = game:GetService("InsertService"):LoadAsset(assetId)
            for _, child in pairs(asset:GetChildren()) do
                child.Parent = model;
            end
        end
        model.Parent = game.Workspace
        
        local assetModel

        -- get assetmodel

        for i,v in pairs(model:GetChildren()) do
            if v:IsA("Accessory") or v:IsA("Tool") or v:IsA("Hat") then
                assetModel = v
            end
        end

        -- check for thumbnailcamera

        if assetModel:FindFirstChild("ThumbnailCamera") then
            print("Has ThumbnailCamera, now using it")
            camera = assetModel:WaitForChild("ThumbnailCamera")
            camera.Parent = model
        else
            camera = Instance.new("Camera", model)-- Instance.new("Camera", player.Character)
        end

        -- set the cam


        workspace.CurrentCamera = camera

        print("[debug] render test asset")
        -- We render twice to correct some weird bugs when rendering assets specifically - meshes only seem to load after the second attempt(?)
        -- ideally this should be done with ContentProvider:PreloadAsync() or even on the C++ side, but that would just take a while to implement.
        -- realistically, Click() is fast enough to be called twice without affecting performance much.
        ThumbnailGenerator:Click('png', 1, 1, true, false)
        -- "fileType", "width", "height", "hideSky", "doCameraZoom"
        local encoded = ThumbnailGenerator:Click('png', _X_RES_, _Y_RES_, true, true)
        print("[debug] send post request containing test asset")

        local ok, data = pcall(function()
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
        return render()
    end)
    print(ok, data);
    print("[debug] exit asset render game");
