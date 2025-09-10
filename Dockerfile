FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/EAI.Api/EAI.Api.csproj", "src/EAI.Api/"]
COPY ["src/EAI.Core/EAI.Core.csproj", "src/EAI.Core/"]
COPY ["src/EAI.Infrastructure/EAI.Infrastructure.csproj", "src/EAI.Infrastructure/"]
RUN dotnet restore "src/EAI.Api/EAI.Api.csproj"
COPY . .
WORKDIR "/src/src/EAI.Api"
RUN dotnet build "EAI.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EAI.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EAI.Api.dll"]
