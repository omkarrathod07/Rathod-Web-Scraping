FROM ://microsoft.com AS base
WORKDIR /app

EXPOSE 8080
EXPOSE 8081

FROM ://microsoft.com AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["RathodWebScraping.csproj", "."]
RUN dotnet restore "./RathodWebScraping.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "RathodWebScraping.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "RathodWebScraping.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# 4. Final step: Run the app using the exposed ports
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RathodWebScraping.dll"]