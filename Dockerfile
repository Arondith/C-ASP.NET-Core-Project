FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/TaskForge.Api/TaskForge.Api.csproj", "src/TaskForge.Api/"]
RUN dotnet restore "src/TaskForge.Api/TaskForge.Api.csproj"

COPY . .
RUN dotnet publish "src/TaskForge.Api/TaskForge.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "TaskForge.Api.dll"]
