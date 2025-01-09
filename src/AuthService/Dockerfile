FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
EXPOSE 80

# copy all .csproj file and restore as distinct layers. Use of the same COPY command
# for every dockerfile in the project to take advantage of docker caching.
COPY Auction.sln Auction.sln
COPY src/AuctionService/AuctionService.Api.csproj src/AuctionService/AuctionService.Api.csproj
COPY src/SearchService/SearchService.Api.csproj src/SearchService/SearchService.Api.csproj
COPY src/ApiGatewayService/ApiGatewayService.csproj src/ApiGatewayService/ApiGatewayService.csproj
COPY src/SharedContracts/SharedContracts.csproj src/SharedContracts/SharedContracts.csproj
COPY src/AuthService/AuthService.csproj src/AuthService/AuthService.csproj

# Restore package dependencies
RUN dotnet restore Auction.sln


# Copy the app folders over
COPY src/AuthService src/AuthService
WORKDIR /app/src/AuthService
RUN dotnet publish -c Release -o /app/src/out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/src/out .
ENTRYPOINT [ "dotnet", "AuthService.dll" ]