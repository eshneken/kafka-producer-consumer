FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY [".", "/src/"]


#WORKDIR /src/
RUN dotnet restore "kafka-producer-consumer.csproj"
COPY . .
RUN dotnet build "kafka-producer-consumer.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "kafka-producer-consumer.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "kafka-producer-consumer.dll"]
