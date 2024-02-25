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
game:GetService("ContentProvider"):SetAssetUrl(baseURL .. "/Asset/")
game:GetService("InsertService"):SetAssetUrl(baseURL .. "/Asset/?id=%d")
pcall(function() game:GetService("ScriptInformationProvider"):SetAssetUrl(url .. "/Asset/") end)
game:GetService("ContentProvider"):SetBaseUrl(baseURL .. "/")
Players:SetChatFilterUrl(baseURL .. "/Game/ChatFilter.ashx")
local Insert = game:GetService("InsertService")
game:GetService("InsertService"):SetAssetUrl(baseURL .. "/Asset/?id=%d")
game:GetService("InsertService"):SetAssetVersionUrl(baseURL .. "/Asset/?assetversionid=%d")

local function applyMesh(children, part)
    local ok, msg = pcall(function() 
        local partForMesh = part
        m = Instance.new("SpecialMesh")
        m.Parent = partForMesh
        -- set
        m.Scale = children.Scale
        m.TextureId = children.TextureId
        m.MeshId = children.MeshId
        m.MeshType = children.MeshType
        m.VertexColor = children.VertexColor
    end)
    if not ok then
        print("error loading mesh", msg)
    end
end

-- insert the part to render

local headPart = Instance.new("Part")
headPart.BrickColor = BrickColor.new("Medium stone grey")
headPart.Parent = Workspace
headPart.Anchored = true

local face = Instance.new("Decal")
face.Texture = "rbxasset://textures/face.png"
face.Name = "face"
face.Parent = headPart

print("passed inserts")

    local function render()
        for _, assetIds in pairs(assetId) do
            local asset = game:GetService("InsertService"):LoadAsset(assetIds)
            print(tostring(#asset:GetChildren()) .. " children inside the asset.")

            -- now do my assetmodel shitty trick?
            local actualMesh

            for i,v in pairs(asset:GetChildren()) do
                if v:IsA('SpecialMesh') or v:IsA("FileMesh") then
                    actualMesh = v
                end
            end

            local partForMesh = headPart
            actualMesh.Parent = headPart

            if actualMesh.MeshId == "http://www.roblox.com/asset/?id=134079402" then
                headPart.face:Destroy()
            end

            camera = Instance.new("Camera", asset)-- Instance.new("Camera", player.Character)
            camera.Name = "ThumbnailCamera"
            workspace.CurrentCamera = camera
        end

        wait(1)
        print("load now")
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
        render()
    end)
    print(ok, data);
    print("[debug] exit game");
