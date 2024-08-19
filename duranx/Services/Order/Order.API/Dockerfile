#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 6063
EXPOSE 4020

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Debug
WORKDIR /src
COPY ["Services/Order/Order.API/Order.API.csproj", "Services/Order/Order.API/"]
COPY ["Services/Order/Order.Application/Order.Application.csproj", "Services/Order/Order.Application/"]
COPY ["BuildingBlocks/BuildingBlocks.Messaging/BuildingBlocks.Messaging.csproj", "BuildingBlocks/BuildingBlocks.Messaging/"]
COPY ["BuildingBlocks/BuildingBlocks.Common/BuildingBlocks.Common.csproj", "BuildingBlocks/BuildingBlocks.Common/"]
COPY ["BuildingBlocks/BuildingBlocks.WebAPI/BuildingBlocks.WebAPI.csproj", "BuildingBlocks/BuildingBlocks.WebAPI/"]
COPY ["Services/Order/Order.Domain/Order.Domain.csproj", "Services/Order/Order.Domain/"]
COPY ["Services/Order/Order.Infrastructure/Order.Infrastructure.csproj", "Services/Order/Order.Infrastructure/"]
RUN dotnet restore "./Services/Order/Order.API/Order.API.csproj"
COPY . .
WORKDIR "/src/Services/Order/Order.API"
RUN dotnet build "./Order.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Debug
RUN dotnet publish "./Order.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

USER root
RUN update-ca-certificates

USER app
ENTRYPOINT ["dotnet", "Order.API.dll"]