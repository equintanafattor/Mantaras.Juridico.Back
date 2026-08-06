FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY . .

RUN dotnet restore \
    src/Mantaras.Juridico.Api/Mantaras.Juridico.Api.csproj

RUN dotnet publish \
    src/Mantaras.Juridico.Api/Mantaras.Juridico.Api.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

CMD ["sh", "-c", "dotnet Mantaras.Juridico.Api.dll --urls http://0.0.0.0:${PORT:-8080}"]