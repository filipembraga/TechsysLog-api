FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY TechsysLog.API/TechsysLog.API.csproj TechsysLog.API/
COPY TechsysLog.Application/TechsysLog.Application.csproj TechsysLog.Application/
COPY TechsysLog.Domain/TechsysLog.Domain.csproj TechsysLog.Domain/
COPY TechsysLog.Infrastructure/TechsysLog.Infrastructure.csproj TechsysLog.Infrastructure/
COPY TechsysLog.CrossCutting/TechsysLog.CrossCutting.csproj TechsysLog.CrossCutting/

RUN dotnet restore TechsysLog.API/TechsysLog.API.csproj

COPY TechsysLog.API/ TechsysLog.API/
COPY TechsysLog.Application/ TechsysLog.Application/
COPY TechsysLog.Domain/ TechsysLog.Domain/
COPY TechsysLog.Infrastructure/ TechsysLog.Infrastructure/
COPY TechsysLog.CrossCutting/ TechsysLog.CrossCutting/

RUN dotnet publish TechsysLog.API/TechsysLog.API.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "TechsysLog.API.dll"]