FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-alpine3.9 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-alpine3.9 AS build
WORKDIR /src

RUN apk update && apk add ca-certificates && rm -rf /var/cache/apk/*

RUN mkdir /root/certs
COPY /certs/* /usr/local/share/ca-certificates/
RUN update-ca-certificates

COPY src/Skoruba.IdentityServer4.Admin/Skoruba.IdentityServer4.Admin.csproj src/Skoruba.IdentityServer4.Admin/
RUN dotnet restore src/Skoruba.IdentityServer4.Admin/Skoruba.IdentityServer4.Admin.csproj
COPY . .
WORKDIR /src/src/Skoruba.IdentityServer4.Admin
RUN dotnet build Skoruba.IdentityServer4.Admin.csproj -c Release -o /app


FROM build AS publish
RUN dotnet publish Skoruba.IdentityServer4.Admin.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Skoruba.IdentityServer4.Admin.dll"]

RUN apk update && apk add ca-certificates && rm -rf /var/cache/apk/*

RUN mkdir /root/certs
COPY /certs/localhost.cer /usr/local/share/ca-certificates/localhost.cer
RUN update-ca-certificates
