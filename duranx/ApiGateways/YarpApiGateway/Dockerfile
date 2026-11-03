#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER root
COPY Certificates/duranx-ca-trust.crt /usr/local/share/ca-certificates/duranx-ca-trust.crt
RUN update-ca-certificates
USER app
WORKDIR /app
EXPOSE 6064
EXPOSE 4020

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Debug
WORKDIR /src
COPY ["ApiGateways/YarpApiGateway/YarpApiGateway.csproj", "ApiGateways/YarpApiGateway/"]
RUN dotnet restore "./ApiGateways/YarpApiGateway/YarpApiGateway.csproj"
COPY . .
WORKDIR "/src/ApiGateways/YarpApiGateway"
RUN dotnet build "./YarpApiGateway.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Debug
RUN dotnet publish "./YarpApiGateway.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Cambiar al usuario root temporalmente para copiar el certificado y actualizar certificados
USER root

WORKDIR /app
# Copiar el certificado CRT al contenedor
# La CA publica se instala en la etapa base.

# Actualizar el almacén de certificados de CA
RUN update-ca-certificates

USER app
ENTRYPOINT ["dotnet", "YarpApiGateway.dll"]