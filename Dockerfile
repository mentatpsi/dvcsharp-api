FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine
LABEL MAINTAINER "Appsecco"

ENV ASPNETCORE_URLS=http://0.0.0.0:5000

COPY . /app

WORKDIR /app

ENV PATH="$PATH:/root/.dotnet/tools"

RUN dotnet tool install  dotnet-ef -a arm64

RUN dotnet restore \
    && dotnet ef database update

EXPOSE 5000

WORKDIR /app

CMD ["dotnet", "watch", "run"]
