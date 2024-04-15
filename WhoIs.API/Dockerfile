FROM mcr.microsoft.com/dotnet/sdk:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src


COPY ["WhoIsAPI.Application/WhoIsAPI.Application.csproj", "./WhoIsAPI.Application/WhoIsAPI.Application.csproj"]
RUN dotnet restore "./WhoIsAPI.Application/WhoIsAPI.Application.csproj"

COPY ["WhoIsAPI.Domain/WhoIsAPI.Domain.csproj", "./WhoIsAPI.Domain/WhoIsAPI.Domain.csproj"]
RUN dotnet restore "./WhoIsAPI.Domain/WhoIsAPI.Domain.csproj"

COPY ["WhoIsAPI.Persistence/WhoIsAPI.Persistence.csproj", "./WhoIsAPI.Persistence/WhoIsAPI.Persistence.csproj"]
RUN dotnet restore "./WhoIsAPI.Persistence/WhoIsAPI.Persistence.csproj"

COPY ["WhoIsAPI/WhoIsAPI.csproj", "./WhoIsAPI/WhoIsAPI.csproj"]
RUN dotnet restore "./WhoIsAPI/WhoIsAPI.csproj"


COPY . .
WORKDIR "/src/."
RUN dotnet build "./WhoIsAPI/WhoIsAPI.csproj" -c %BUILD_CONFIGURATION% -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./WhoIsAPI/WhoIsAPI.csproj" -c %BUILD_CONFIGURATION% -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WhoIsAPI.dll"]