# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["OAuthServer.V2.API/OAuthServer.V2.API.csproj", "OAuthServer.V2.API/"]
COPY ["OAuthServer.V2.Infrastructure/OAuthServer.V2.Infrastructure.csproj", "OAuthServer.V2.Infrastructure/"]
COPY ["OAuthServer.V2.Core/OAuthServer.V2.Core.csproj", "OAuthServer.V2.Core/"]
COPY ["OAuthServer.V2.Service/OAuthServer.V2.Service.csproj", "OAuthServer.V2.Service/"]
COPY ["OAuthServer.V2.Data/OAuthServer.V2.Data.csproj", "OAuthServer.V2.Data/"]
RUN dotnet restore "./OAuthServer.V2.API/OAuthServer.V2.API.csproj"
COPY . .
WORKDIR "/src/OAuthServer.V2.API"
RUN dotnet build "./OAuthServer.V2.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# MIGRATION STAGE
#FROM build AS migration
#RUN dotnet tool install --global dotnet-ef
#ENV PATH="${PATH}:/root/.dotnet/tools"
#WORKDIR /src
#RUN dotnet build OAuthServer.V2.API/OAuthServer.V2.API.csproj -c Release
#ENTRYPOINT ["dotnet", "ef", "database", "update", \
            #"--project", "OAuthServer.V2.Data/OAuthServer.V2.Data.csproj", \
            #"--startup-project", "OAuthServer.V2.API/OAuthServer.V2.API.csproj", \ 
            #"--no-build", \
            #"--configuration", "Release"]

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./OAuthServer.V2.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OAuthServer.V2.API.dll"]