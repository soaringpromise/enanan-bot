FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

COPY *.csproj ./
RUN dotnet restore

COPY . .
RUN dotnet publish -c Release -o /out --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

RUN mkdir -p /app/data

RUN apt-get update && apt-get install -y \
    libfontconfig1 \
    libfreetype6 \
    fontconfig \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /out .

ENTRYPOINT ["dotnet", "EnananBot.dll"]
