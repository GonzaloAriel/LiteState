# ============================================================
# Etapa 1: BUILD (contiene el SDK, pesa ~800MB, se descarta)
# ============================================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar SOLO el .csproj y restaurar primero.
# Mientras el csproj no cambie, esta capa sale del cache de Docker
# y los builds siguientes son mucho mas rapidos.
COPY LiteState.csproj ./
RUN dotnet restore LiteState.csproj

# Recien ahora el codigo fuente.
COPY . .
RUN dotnet publish LiteState.csproj -c Release -o /app/publish --no-restore


# ============================================================
# Etapa 2: RUNTIME (solo el runtime + la app publicada)
# ============================================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# curl unicamente para el HEALTHCHECK de abajo.
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_HTTP_PORTS=8080

EXPOSE 8080

# Dejar de correr como root. APP_UID lo define la imagen base.
# 8080 es un puerto alto, asi que no hace falta privilegio para abrirlo.
USER $APP_UID

# /Home/Privacy renderiza una vista real: si responde 200,
# significa que routing + controller + views + static files funcionan.
HEALTHCHECK --interval=30s --timeout=5s --start-period=15s --retries=3 \
    CMD curl -fsS http://localhost:8080/Home/Privacy || exit 1

ENTRYPOINT ["dotnet", "LiteState.dll"]
