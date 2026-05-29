FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["PimientaRosa.API/PimientaRosa.API.csproj", "PimientaRosa.API/"]
RUN dotnet restore "PimientaRosa.API/PimientaRosa.API.csproj"
COPY . .
WORKDIR "/src/PimientaRosa.API"
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PimientaRosa.API.dll"]