# base image for running the application
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app

# base image for building the application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ./ .
RUN dotnet restore todo_backend.csproj
RUN dotnet build todo_backend.csproj -c Release -o /app/build

FROM build AS publish
RUN dotnet publish todo_backend.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
RUN mkdir -p /app/wwwroot/images
COPY --from=publish /app/publish .
ENV PORT=3500
EXPOSE 3500
ENTRYPOINT [ "dotnet", "todo_backend.dll" ]