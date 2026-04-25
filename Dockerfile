FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY Heureka_Metaheuristic_System_Backend_Api.slnx ./
COPY Heureka_Metaheuristic_System_Backend_Api/ Heureka_Metaheuristic_System_Backend_Api/

RUN dotnet restore Heureka_Metaheuristic_System_Backend_Api/Heureka_Metaheuristic_System_Backend_Api.csproj

RUN dotnet publish Heureka_Metaheuristic_System_Backend_Api/Heureka_Metaheuristic_System_Backend_Api.csproj -c Release -o /app /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app ./

# Porty
EXPOSE 5000
EXPOSE 5001

ENTRYPOINT ["dotnet", "Heureka_Metaheuristic_System_Backend_Api.dll"]
