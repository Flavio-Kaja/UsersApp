FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env
WORKDIR /app

COPY ["UserService.csproj", "./"]
RUN dotnet restore "./UserService.csproj"

COPY . ./
RUN dotnet build "UserService.csproj" -c Release -o /app/build

FROM build-env AS publish
RUN dotnet publish "UserService.csproj" -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY --from=publish /app/out .

ENV ASPNETCORE_URLS=http://+:5360
EXPOSE 5360

ENTRYPOINT ["dotnet", "/app/UserService.dll"]
