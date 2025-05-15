# Use SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy the .csproj and restore
COPY AttendanceTracker1/*.csproj ./AttendanceTracker1/
WORKDIR /src/AttendanceTracker1
RUN dotnet restore

# Copy the rest of the code and build
COPY AttendanceTracker1/. ./AttendanceTracker1/
WORKDIR /src/AttendanceTracker1
RUN dotnet publish -c Release -o /app/publish

# Use ASP.NET image to run the app
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "AttendanceTracker1.dll"]
