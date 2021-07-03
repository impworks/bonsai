FROM node:lts-alpine as node
WORKDIR /build/Bonsai

ADD src/Bonsai/package.json .
ADD src/Bonsai/package-lock.json .
RUN npm ci
ADD src/Bonsai .
RUN node_modules/.bin/gulp build

FROM mcr.microsoft.com/dotnet/sdk:5.0 as net-builder
WORKDIR /build
ADD src/Bonsai.sln .
ADD src/Bonsai/Bonsai.csproj Bonsai/
ADD src/Bonsai.Tests.Search/Bonsai.Tests.Search.csproj Bonsai.Tests.Search/

RUN dotnet restore
COPY --from=node /build .

RUN dotnet publish --output ../out/ --configuration Release --runtime linux-musl-x64 --self-contained true /p:PublishTrimmed=true Bonsai/Bonsai.csproj

FROM alpine:latest

RUN apk add --no-cache nodejs ffmpeg libintl icu && \
    apk add --no-cache libgdiplus --repository=http://dl-cdn.alpinelinux.org/alpine/edge/testing/

WORKDIR /app
COPY --from=net-builder /out .

RUN mkdir /app/External/ffmpeg
RUN ln -s /usr/bin/ffmpeg /app/External/ffmpeg/ffmpeg && \
    ln -s /usr/bin/ffprobe /app/External/ffmpeg/ffprobe && \
    chmod +x /app/Bonsai

ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["/app/Bonsai"]
