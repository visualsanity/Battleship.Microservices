FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-focal AS base
WORKDIR /app
EXPOSE 8085/tcp
ENV ASPNETCORE_URLS http://*:8085
HEALTHCHECK --interval=30s --timeout=3s --retries=1 CMD curl --silent --fail http://0.0.0.0:8085/api/Audit || exit 1

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-focal AS build
WORKDIR /src
COPY ["Battleship.Audit/Battleship.Audit.csproj", "Battleship.Audit/"]
RUN dotnet restore "Battleship.Audit/Battleship.Audit.csproj"
COPY . .
WORKDIR "/src/Battleship.Audit"
RUN dotnet build "Battleship.Audit.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Battleship.Audit.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY /Battleship.Audit/wait-for-it.sh wait-for-it.sh
RUN chmod +x wait-for-it.sh
