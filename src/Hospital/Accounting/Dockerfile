FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Accounting/Accounting.csproj", "Accounting/"]
RUN dotnet restore "Accounting/Accounting.csproj"
COPY . .
WORKDIR "/src/Accounting"
RUN dotnet build "Accounting.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Accounting.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Accounting.dll"]
