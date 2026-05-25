FROM mcr.microsoft.com/dotnet/sdk:10.0.300-alpine3.23 AS build
WORKDIR /src

COPY ["AW.slnx", "./"]
COPY ["Directory.Packages.props", "./"]
COPY ["AW.Api/AW.Api.csproj", "AW.Api/"]
COPY ["AW.Application/AW.Application.csproj", "AW.Application/"]
COPY ["AW.Infrastructure/AW.Infrastructure.csproj", "AW.Infrastructure/"]
COPY ["AW.Domain/AW.Domain.csproj", "AW.Domain/"]
RUN dotnet restore "AW.slnx"

COPY . .
RUN dotnet publish "AW.Api/AW.Api.csproj" \
    -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0.8-alpine3.23 AS runtime
WORKDIR /app

RUN apk add --no-cache icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
RUN addgroup -S appgroup && adduser -S appuser -G appgroup
USER appuser

COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "AW.Api.dll"]
