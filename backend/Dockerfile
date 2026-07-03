FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/ChainMind.Api/ChainMind.Api.csproj src/ChainMind.Api/
COPY src/ChainMind.Application/ChainMind.Application.csproj src/ChainMind.Application/
COPY src/ChainMind.Core/ChainMind.Core.csproj src/ChainMind.Core/
COPY src/ChainMind.Infrastructure/ChainMind.Infrastructure.csproj src/ChainMind.Infrastructure/

RUN dotnet restore src/ChainMind.Api/ChainMind.Api.csproj

COPY src/ src/

RUN dotnet publish src/ChainMind.Api/ChainMind.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://0.0.0.0:10000
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 10000

COPY --from=build /app/publish .

CMD ["dotnet", "ChainMind.Api.dll"]
