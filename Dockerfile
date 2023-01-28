FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000
RUN groupadd -g 2000 dotnet \
    && useradd -m -u 2000 -g 2000 dotnet
USER dotnet


FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["elasticsearchApi/elasticsearchApi.csproj", "elasticsearchApi/"]
RUN dotnet restore "elasticsearchApi/elasticsearchApi.csproj"
COPY . .
WORKDIR "/src/elasticsearchApi"

RUN dotnet build "elasticsearchApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "elasticsearchApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "elasticsearchApi.dll"]