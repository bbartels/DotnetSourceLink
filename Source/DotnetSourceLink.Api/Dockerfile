FROM mcr.microsoft.com/dotnet/core/aspnet:3.0-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.0-buster AS build
WORKDIR /src
COPY ["DotnetSourceLink.Api/DotnetSourceLink.Api.csproj", "DotnetSourceLink.Api/"]
RUN dotnet restore "DotnetSourceLink.Api/DotnetSourceLink.Api.csproj"
COPY . .
WORKDIR "/src/DotnetSourceLink.Api"
RUN dotnet build "DotnetSourceLink.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DotnetSourceLink.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DotnetSourceLink.Api.dll"]