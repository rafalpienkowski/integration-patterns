FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Scheduling/Scheduling.csproj", "Scheduling/"]
RUN dotnet restore "Scheduling/Scheduling.csproj"
COPY . .
WORKDIR "/src/Scheduling"
RUN dotnet build "Scheduling.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Scheduling.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Scheduling.dll"]
