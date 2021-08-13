FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["AppointmentEnricher/AppointmentEnricher.csproj", "AppointmentEnricher/"]
RUN dotnet restore "AppointmentEnricher/AppointmentEnricher.csproj"
COPY . .
WORKDIR "/src/AppointmentEnricher"
RUN dotnet build "AppointmentEnricher.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AppointmentEnricher.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AppointmentEnricher.dll"]
