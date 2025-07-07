# 1) Usa el SDK .NET 8 como etapa base
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

## 1.1) Instala las librerías nativas requeridas por SkiaSharp en Debian Bookworm
# ————— Tus deps SkiaSharp ————————————————————————————————
RUN apt-get update && \
    apt-get install -y --no-install-recommends \
      libfontconfig1 \
      libharfbuzz0b \ 
      libfreetype6 \
      libpng16-16 \
      libjpeg62-turbo && \
    rm -rf /var/lib/apt/lists/*

## 2) unixODBC para runtime ODBC
#RUN apt-get update && \
    #apt-get install -y --no-install-recommends \
      #unixodbc unixodbc-dev libaio1 curl && \
    #rm -rf /var/lib/apt/lists/*
#
## 3) Herramientas de compilación para el driver
#RUN apt-get update && \
    #apt-get install -y --no-install-recommends \
      #build-essential autoconf automake libtool pkg-config && \
    #rm -rf /var/lib/apt/lists/*
#
## 4) Copia el tar.gz (código fuente) y compílalo
#COPY ibm-iaccess-linux.tar.gz /tmp/
#RUN mkdir -p /opt/ibm/iaccess && \
    #cd /tmp && \
    #tar -xzf ibm-iaccess-linux.tar.gz && \
    #cd $(ls -d */ | grep -i "iaccess\|cwbodbc") && \
    #./configure --prefix=/opt/ibm/iaccess && \
    #make && make install

# 2) Copia la solución y restaura paquetes
COPY *.sln ./
COPY AutomationTest.FitbankWeb3/*.csproj AutomationTest.FitbankWeb3/
RUN dotnet restore

# 3) Copia todo el código y compila en modo Release
COPY . .

# 4) Compila en Release (genera playwright.ps1 y playwright.sh)
RUN dotnet build -c Release --no-restore

# 5) Instala el Playwright CLI como herramienta global (última versión disponible)
RUN dotnet tool install --global Microsoft.Playwright.CLI

# 6) Actualiza el PATH para que incluya las global tools
ENV PATH="$PATH:/root/.dotnet/tools"

# 7) Descarga los navegadores y sus deps
#RUN playwright install --with-deps
RUN playwright install chromium --with-deps

# Ya no definimos ENTRYPOINT: ejecutaremos 'dotnet test' manualmente