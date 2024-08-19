#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 6061
EXPOSE 4020

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Debug
WORKDIR /src
COPY ["Services/Cart/Cart.API/Cart.API.csproj", "Services/Cart/Cart.API/"]
COPY ["BuildingBlocks/BuildingBlocks.Cache/BuildingBlocks.Cache.csproj", "BuildingBlocks/BuildingBlocks.Cache/"]
COPY ["BuildingBlocks/BuildingBlocks.Common/BuildingBlocks.Common.csproj", "BuildingBlocks/BuildingBlocks.Common/"]
COPY ["BuildingBlocks/BuildingBlocks.Messaging/BuildingBlocks.Messaging.csproj", "BuildingBlocks/BuildingBlocks.Messaging/"]
COPY ["BuildingBlocks/BuildingBlocks.WebAPI/BuildingBlocks.WebAPI.csproj", "BuildingBlocks/BuildingBlocks.WebAPI/"]
RUN dotnet restore "./Services/Cart/Cart.API/Cart.API.csproj"
COPY . .
WORKDIR "/src/Services/Cart/Cart.API"
RUN dotnet build "./Cart.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Debug
RUN dotnet publish "./Cart.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Cambiar al usuario root temporalmente para copiar el certificado y actualizar certificados
USER root

WORKDIR /app
# Copiar el certificado CRT al contenedor
COPY Certificates/ /usr/local/share/ca-certificates/

# Actualizar el almacén de certificados de CA
RUN update-ca-certificates

# Cambiar de vuelta al usuario app
USER app

ENTRYPOINT ["dotnet", "Cart.API.dll"]