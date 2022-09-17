FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["CustomerCare.RecipientList/CustomerCare.RecipientList.csproj", "CustomerCare.RecipientList/"]
RUN dotnet restore "CustomerCare.RecipientList/CustomerCare.RecipientList.csproj"
COPY . .
WORKDIR "/src/CustomerCare.RecipientList"
RUN dotnet build "CustomerCare.RecipientList.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CustomerCare.RecipientList.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CustomerCare.RecipientList.dll"]
