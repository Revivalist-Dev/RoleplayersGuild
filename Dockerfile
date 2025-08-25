# Stage 1: Frontend Build
# Use a dedicated Node.js image to build the client-side assets.
FROM node:lts-alpine AS frontend
WORKDIR /project/Site.Client
COPY Site.Client/package*.json ./
RUN npm install
COPY Site.Client/ ./
COPY Site.Assets/ ../Site.Assets/
RUN npm run build

# Stage 2: Backend Build
# Use the .NET SDK image to build the application.
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /app

# Copy project files and restore dependencies first to leverage Docker layer caching.
# To optimize the build and avoid Windows path length issues, we copy only the
# project file first. This allows 'dotnet restore' to run with shorter paths
# and leverages Docker's layer caching more effectively.
COPY RoleplayersGuild.csproj ./

# Restore dependencies for the project.
RUN dotnet restore "RoleplayersGuild.csproj"

# Now, copy the rest of the source code into the image.
COPY . .

# CRITICAL STEP: Copy the compiled frontend assets from the 'frontend' stage
# into the location where the .NET publish process expects them.
COPY --from=frontend /project/Site.Client/dist ./wwwroot/

# Publish the .NET application.
RUN dotnet publish "RoleplayersGuild.csproj" -c $BUILD_CONFIGURATION -o /app/publish --no-restore

# Stage 3: Final Production Image
# Use the lightweight ASP.NET runtime for a smaller final image.
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "RoleplayersGuild.dll"]