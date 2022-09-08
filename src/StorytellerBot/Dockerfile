FROM mcr.microsoft.com/dotnet/aspnet:6.0-focal AS base
WORKDIR /app
EXPOSE 5000

ENV ASPNETCORE_URLS=http://+:5000

# Creates a non-root user with an explicit UID and adds permission to access the /app folder
# For more info, please refer to https://aka.ms/vscode-docker-dotnet-configure-containers
RUN adduser -u 5678 --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

FROM mcr.microsoft.com/dotnet/sdk:6.0-focal AS build
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
VOLUME [ "/app/logs", "/app/media", "/app/scripts", "/app/db" ]
ENTRYPOINT ["dotnet", "StorytellerBot.dll"]
