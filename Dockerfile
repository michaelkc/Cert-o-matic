FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY src/*.sln ./src/
COPY src/UI/*.csproj ./src/UI/
RUN dotnet restore ./src/

# Copy everything else and build
COPY src/UI/. ./src/UI/
COPY lib/. ./lib/
RUN dotnet publish ./src/ -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "UI.dll"]