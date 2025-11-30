# base image for running the application
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app

# base image for building the application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ./ .
RUN dotnet restore log_output.csproj
RUN dotnet build log_output.csproj -c Release -o /app/build

FROM build AS publish
RUN dotnet publish log_output.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV PORT=3000
EXPOSE 3000
ENTRYPOINT [ "dotnet", "log_output.dll" ]