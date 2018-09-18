FROM microsoft/dotnet:2.1-aspnetcore-runtime-nanoserver-1709 AS base
WORKDIR /app
EXPOSE 51809
EXPOSE 44348

FROM microsoft/dotnet:2.1-sdk-nanoserver-1709 AS build
WORKDIR /src
COPY Books.Api/Books.Api.csproj Books.Api/
RUN dotnet restore Books.Api/Books.Api.csproj
COPY . .
WORKDIR /src/Books.Api
RUN dotnet build Books.Api.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish Books.Api.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Books.Api.dll"]
