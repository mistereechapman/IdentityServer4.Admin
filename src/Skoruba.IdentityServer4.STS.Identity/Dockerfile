FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-alpine3.9 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-alpine3.9 AS build
WORKDIR /src

RUN mkdir /root/certs
COPY /certs/* /usr/local/share/ca-certificates/
RUN update-ca-certificates

COPY src/Skoruba.IdentityServer4.STS.Identity/Skoruba.IdentityServer4.STS.Identity.csproj src/Skoruba.IdentityServer4.STS.Identity/
RUN dotnet restore src/Skoruba.IdentityServer4.STS.Identity/Skoruba.IdentityServer4.STS.Identity.csproj
COPY . .
WORKDIR /src/src/Skoruba.IdentityServer4.STS.Identity
RUN dotnet build Skoruba.IdentityServer4.STS.Identity.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish Skoruba.IdentityServer4.STS.Identity.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Skoruba.IdentityServer4.STS.Identity.dll"]
