FROM microsoft/dotnet:3.1-sdk AS build-env
WORKDIR /app

# Copy necessary files and restore as distinct layer
COPY *.csproj ./
RUN dotnet restore 

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out

# Build runtime image
FROM microsoft/dotnet:3.1-aspnetcore-runtime
COPY --from=build-env /app/out .

# Expose ports
EXPOSE 8085/tcp
ENV ASPNETCORE_URLS http://*:8086
HEALTHCHECK --interval=30s --timeout=3s --retries=1 CMD curl --silent --fail http://localhost:8086 || exit 1

# Start
ENTRYPOINT ["dotnet", "Battleship.Warehouse.dll"]