# --- Build stage ---
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Kopiujemy csproj i solution
COPY Heureka_Metaheuristic_System_Backend_Api.slnx ./
COPY Heureka_Metaheuristic_System_Backend_Api/ Heureka_Metaheuristic_System_Backend_Api/

# Restore tylko projektu
RUN dotnet restore Heureka_Metaheuristic_System_Backend_Api/Heureka_Metaheuristic_System_Backend_Api.csproj

# Publish tylko projektu w Release
RUN dotnet publish Heureka_Metaheuristic_System_Backend_Api/Heureka_Metaheuristic_System_Backend_Api.csproj -c Release -o /app /p:UseAppHost=false

# --- Runtime stage ---
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Kopiujemy publikowane pliki
COPY --from=build /app ./

# Porty
EXPOSE 5000
EXPOSE 5001

ENTRYPOINT ["dotnet", "Heureka_Metaheuristic_System_Backend_Api.dll"]
