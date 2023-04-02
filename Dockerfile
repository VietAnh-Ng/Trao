FROM mcr.microsoft.com/dotnet/sdk:7.0 as build-env
WORKDIR /src
COPY Client ./Client
COPY Server ./Server
COPY Shared ./Shared
WORKDIR /src/Server
RUN dotnet restore
RUN dotnet publish -c Release -o /publish

FROM mcr.microsoft.com/dotnet/aspnet:7.0 as runtime
WORKDIR /publish
COPY --from=build-env /publish .
EXPOSE 5000
ENTRYPOINT ["dotnet", "TraoApp.Server.dll"]