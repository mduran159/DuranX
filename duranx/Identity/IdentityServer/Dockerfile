#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081
EXPOSE 4020

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Debug
WORKDIR /src
COPY ["Identity/IdentityServer/IdentityServer.csproj", "Identity/IdentityServer/"]
RUN dotnet restore "./Identity/IdentityServer/IdentityServer.csproj"
COPY . .
WORKDIR "/src/Identity/IdentityServer"
RUN dotnet build "./IdentityServer.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Debug
RUN dotnet publish "./IdentityServer.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Cambiar al usuario root temporalmente para copiar el certificado y actualizar certificados
USER root

WORKDIR /app
# Copiar el certificado CRT al contenedor
COPY Certificates/ /usr/local/share/ca-certificates/

# Actualizar el almac�n de certificados de CA
RUN update-ca-certificates

USER app
ENTRYPOINT ["dotnet", "IdentityServer.dll"]
