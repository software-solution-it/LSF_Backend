FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["LSF.csproj", "./"]
RUN dotnet restore "./LSF.csproj"

COPY . .
WORKDIR "/src/."
RUN dotnet build "LSF.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LSF.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LSF.dll"]
