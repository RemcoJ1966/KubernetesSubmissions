# base image for running the application
FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
WORKDIR /app

# base image for building the application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ./ .
RUN dotnet restore writer.csproj
RUN dotnet build writer.csproj -c Release -o /app/build

FROM build AS publish
RUN dotnet publish writer.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT [ "dotnet", "writer.dll" ]
