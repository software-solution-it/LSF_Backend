FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG DOTNET_BUILD_ARGS=""
WORKDIR /src

COPY ["src/LSF/LSF.csproj", "src/LSF/"]
RUN dotnet restore "src/LSF/LSF.csproj"


COPY . .
WORKDIR "/src/src/LSF"
RUN dotnet build "LSF.csproj" -c Release -o /app/build $DOTNET_BUILD_ARGS


FROM build AS publish
RUN dotnet publish "LSF.csproj" -c Release -o /app/publish


FROM base AS final
WORKDIR /app


COPY --from=publish /app/publish .
RUN rm -rf /root/.nuget && \
    rm -rf /tmp/* && \
    rm -rf /var/tmp/*

ENTRYPOINT ["dotnet", "LSF.dll"]
