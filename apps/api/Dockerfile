# ---------- Build ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY apps/api/src/*.csproj ./apps/api/src/
RUN dotnet restore ./apps/api/src/CustomerSpending.Api.csproj

COPY . .

RUN dotnet publish ./apps/api/src/CustomerSpending.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

# ---------- Runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish ./
ENTRYPOINT ["dotnet", "CustomerSpending.Api.dll"]
