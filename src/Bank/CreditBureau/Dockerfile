FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["CreditBureau/CreditBureau.csproj", "CreditBureau/"]
RUN dotnet restore "CreditBureau/CreditBureau.csproj"
COPY . .
WORKDIR "/src/CreditBureau"
RUN dotnet build "CreditBureau.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CreditBureau.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CreditBureau.dll"]
