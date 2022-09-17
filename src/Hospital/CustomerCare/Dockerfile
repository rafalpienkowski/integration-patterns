FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["CustomerCare/CustomerCare.csproj", "CustomerCare/"]
RUN dotnet restore "CustomerCare/CustomerCare.csproj"
COPY . .
WORKDIR "/src/CustomerCare"
RUN dotnet build "CustomerCare.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CustomerCare.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CustomerCare.dll"]
