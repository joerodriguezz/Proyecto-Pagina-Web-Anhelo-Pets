FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Backend/AnheloPets.API/AnheloPets.API.csproj Backend/AnheloPets.API/
RUN dotnet restore Backend/AnheloPets.API/AnheloPets.API.csproj

COPY Backend/AnheloPets.API/ Backend/AnheloPets.API/
RUN dotnet publish Backend/AnheloPets.API/AnheloPets.API.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 10000
ENTRYPOINT ["dotnet", "AnheloPets.API.dll"]
