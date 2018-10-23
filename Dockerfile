FROM microsoft/dotnet:sdk AS build-env
WORKDIR /app

COPY . .
RUN dotnet restore
RUN dotnet publish ./MemeScrapper/MemeScrapper.csproj -c Release -o out

# Build runtime image
FROM microsoft/dotnet:2.1-runtime-deps
WORKDIR /app
COPY --from=build-env /app/MemeScrapper/out .
ENTRYPOINT ["dotnet", "run"]
