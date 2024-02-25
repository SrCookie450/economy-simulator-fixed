FROM mcr.microsoft.com/dotnet/sdk:6.0

WORKDIR /src
# Copy everything...
COPY ["./Roblox/", "/src/"]
# Restore everything
RUN dotnet restore "/src/Roblox.IntegrationTest/Roblox.IntegrationTest.csproj"&&\
    cp /src/Roblox.IntegrationTest/appsettings.json /src/Roblox.Website/ &&\
    cd /src/Roblox.IntegrationTest && dotnet build;


CMD cd /src/Roblox.IntegrationTest/ && dotnet test;