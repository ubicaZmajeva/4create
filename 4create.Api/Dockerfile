FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["4create.Api/4create.Api.csproj", "4create.Api/"]
COPY ["4create.Application/4create.Application.csproj", "4create.Application/"]
COPY ["4create.Infrastructure/4create.Infrastructure.csproj", "4create.Infrastructure/"]
RUN dotnet restore "4create.Api/4create.Api.csproj"
COPY . .
WORKDIR "/src/4create.Api"
RUN dotnet build "4create.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "4create.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "4create.Api.dll"]
