﻿FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["CreditAgent/CreditAgent.csproj", "CreditAgent/"]
RUN dotnet restore "CreditAgent/CreditAgent.csproj"
COPY . .
WORKDIR "/src/CreditAgent"
RUN dotnet build "CreditAgent.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CreditAgent.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CreditAgent.dll"]
