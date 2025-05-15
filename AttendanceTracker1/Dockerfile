# --- Stage 1: Build ---
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out

# --- Stage 2: Runtime ---
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./

# Set port (you can change 80 if needed)
EXPOSE 80

# Entry point to run your app
ENTRYPOINT ["dotnet", "AttendanceTracker1.dll"]
