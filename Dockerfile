# --- etap 1: build ---
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# najpierw sam csproj -> cache restore'a gdy zmienia się kod, a zależności nie
COPY goynote.csproj .
RUN dotnet restore

COPY . .
RUN dotnet publish goynote.csproj -c Release -o /app/publish

# --- etap 2: runtime (chudy obraz, bez SDK) ---
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# katalog na bazę SQLite - montowany jako wolumen w runtime.
# Tworzony tutaj jako siatka bezpieczeństwa, gdyby wolumen nie był podany.
RUN mkdir -p /app/data

# .NET 8+ domyślnie nasłuchuje na 8080 w kontenerze. TLS terminuje reverse proxy PRZED kontenerem.
EXPOSE 8080
ENTRYPOINT ["dotnet", "goynote.dll"]
