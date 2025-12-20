# base image for running the application
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app

# base image for building the application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ./ .
RUN dotnet restore todo_job.csproj
RUN dotnet build todo_job.csproj -c Release -o /app/build

FROM build AS publish
RUN dotnet publish todo_job.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
RUN mkdir -p /app/wwwroot/images
COPY --from=publish /app/publish .
ENTRYPOINT [ "dotnet", "todo_job.dll" ]