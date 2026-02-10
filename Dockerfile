FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["User Management System/User Management System.csproj", "User Management System/"]
COPY ["Application/Application.csproj", "Application/"]
COPY ["Domain/Domain.csproj", "Domain/"]
COPY ["Infrasturcture/Infrasturcture.csproj", "Infrasturcture/"]

RUN dotnet restore "User Management System/User Management System.csproj"

COPY . .
RUN dotnet publish "User Management System/User Management System.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "User Management System.dll"]
