FROM mcr.microsoft.com/dotnet/aspnet:6.0-focal AS base
LABEL org.opencontainers.image.source https://github.com/tipcat-net/tipcat-net-payout

ARG VAULT_TOKEN

ENV TCDN_VAULT_TOKEN=$VAULT_TOKEN

RUN apt update && apt install -y \
    libgdiplus \
    libc6-dev \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0-focal AS build
ARG GITHUB_TOKEN
WORKDIR /src
COPY *.sln ./
COPY . .

RUN dotnet restore
WORKDIR /src/TipCatDotNet.Payout
RUN dotnet build -c Release -o /app

FROM build AS publish
RUN dotnet publish -c Release -o /app

FROM base AS final
WORKDIR /app

COPY --from=publish /app .
HEALTHCHECK --interval=6s --timeout=10s --retries=3 CMD curl -sS 127.0.0.1/health || exit 1

ENTRYPOINT ["dotnet", "TipCatDotNet.Payout.dll"]
