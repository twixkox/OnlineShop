# Сборка сайта
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY OnlineShop.Db/OnlineShop.Db.csproj OnlineShop.Db/
COPY OnlineShopWebApp/OnlineShopWebApp.csproj OnlineShopWebApp/
RUN dotnet restore OnlineShopWebApp/OnlineShopWebApp.csproj
COPY OnlineShop.Db/ OnlineShop.Db/
COPY OnlineShopWebApp/ OnlineShopWebApp/
RUN dotnet publish OnlineShopWebApp/OnlineShopWebApp.csproj -c Release -o /app --no-restore

# Запуск от непривилегированного пользователя
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
# ImageSharp и культура ru-RU требуют ICU
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    ASPNETCORE_HTTP_PORTS=8080
COPY --from=build /app .
RUN mkdir -p /app/App_Data /app/Logs /app/wwwroot/uploads && chown -R app:app /app
USER app
# Загруженные фото, ключи cookie и журналы хранятся в томах
VOLUME ["/app/App_Data", "/app/Logs", "/app/wwwroot/uploads"]
EXPOSE 8080
ENTRYPOINT ["dotnet", "OnlineShopWebApp.dll"]
