FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["SendEmail/APIsEstudioJuridico.csproj", "SendEmail/"]
RUN dotnet restore "SendEmail/APIsEstudioJuridico.csproj"
COPY . .
WORKDIR "/src/SendEmail"
RUN dotnet build "APIsEstudioJuridico.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "APIsEstudioJuridico.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
# OpenCV/Tesseract native deps that may be required by some packages
RUN apt-get update && apt-get install -y --no-install-recommends \
    libgdiplus \
    libc6-dev \
    && rm -rf /var/lib/apt/lists/*
COPY --from=publish /app/publish .
RUN mkdir -p /app/DocumentosSistema /app/secrets
ENTRYPOINT ["dotnet", "APIsEstudioJuridico.dll"]
