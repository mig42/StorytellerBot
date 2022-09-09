FROM mcr.microsoft.com/dotnet/aspnet:6.0-jammy AS base
WORKDIR /app
EXPOSE 5000

ENV ASPNETCORE_URLS=http://+:5000
RUN  mkdir -p /app/db /app/logs /app/scripts /app/media

FROM mcr.microsoft.com/dotnet/sdk:6.0-jammy AS build
WORKDIR /src
COPY ["src/StorytellerBot/StorytellerBot.csproj", "src/StorytellerBot/"]
RUN dotnet restore "src/StorytellerBot/StorytellerBot.csproj"
COPY . .
WORKDIR "/src/src/StorytellerBot"
RUN dotnet build "StorytellerBot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "StorytellerBot.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

COPY --from=publish /app/publish .

VOLUME [ "/app/db", "/app/logs", "/app/media", "/app/scripts" ]
ENTRYPOINT ["dotnet", "StorytellerBot.dll"]
