FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
WORKDIR /src
COPY ["SampleAPI/SampleAPI.csproj", "SampleAPI/"]
RUN dotnet restore "SampleAPI/SampleAPI.csproj"
COPY . .
WORKDIR "/src/SampleAPI"
RUN dotnet build "SampleAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SampleAPI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SampleAPI.dll"]
