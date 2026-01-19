FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["sources/X39.Software.ExamMaker.WebApp/X39.Software.ExamMaker.WebApp.csproj", "sources/X39.Software.ExamMaker.WebApp/"]
COPY ["sources/X39.Software.ExamMaker.Shared/X39.Software.ExamMaker.Shared.csproj", "sources/X39.Software.ExamMaker.Shared/"]
RUN dotnet restore "sources/X39.Software.ExamMaker.WebApp/X39.Software.ExamMaker.WebApp.csproj"
COPY . .
WORKDIR "/src/sources/X39.Software.ExamMaker.WebApp"
RUN dotnet build "./X39.Software.ExamMaker.WebApp.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./X39.Software.ExamMaker.WebApp.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "X39.Software.ExamMaker.WebApp.dll"]
