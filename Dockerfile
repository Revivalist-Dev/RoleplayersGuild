# Stage 1: Frontend Build
# Use the official Node.js LTS image to build all frontend assets
FROM node:lts AS frontend-build
WORKDIR /src

# Copy all package management files first to leverage Docker layer caching
COPY package*.json ./
COPY Site.Client/package*.json ./Site.Client/

# Copy the script needed for asset copying
COPY Project.Tools/Scripts/copy-vendor-assets.js ./Project.Tools/Scripts/

# Install root dependencies
RUN npm install

# Install client dependencies
RUN npm install --prefix Site.Client

# Copy the rest of the frontend source code needed for the build
COPY Site.Styles/ ./Site.Styles/
COPY Site.Client/ ./Site.Client/

# Run all build scripts to generate the wwwroot assets
RUN npm run copy:assets
RUN npm run build:css
RUN npm run build --prefix Site.Client

# Stage 2: Backend Build
# Use the .NET SDK image to build the backend application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS backend-build
WORKDIR /src

# Copy project files and restore dependencies
COPY *.csproj ./
COPY Site.Client/*.csproj ./Site.Client/
RUN dotnet restore "RoleplayersGuild.csproj"

# Copy the rest of the backend source code
COPY . .

# Copy the pre-built frontend assets from the frontend-build stage
COPY --from=frontend-build /src/wwwroot ./wwwroot

# Publish the application.
# The frontend assets are already built. We pass /p:BuildingInsideDocker=true
# to prevent MSBuild from trying to run the npm targets again.
RUN dotnet publish "RoleplayersGuild.csproj" -c Release -o /app/publish --no-restore /p:BuildingInsideDocker=true

# Stage 3: Final Production Image
# Use the lightweight ASP.NET runtime image for a smaller final image
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

# Copy the published application from the backend-build stage
COPY --from=backend-build /app/publish .

# The default ASP.NET Core port in containers is 8080
EXPOSE 8080

# Define the entry point for running the application
ENTRYPOINT ["dotnet", "RoleplayersGuild.dll"]