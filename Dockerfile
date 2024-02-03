FROM --platform=$BUILDPLATFORM node:14-alpine as node
ARG TARGETARCH
ARG TARGETVARIANT

WORKDIR /build/Bonsai

ADD src/Bonsai/package.json .
ADD src/Bonsai/package-lock.json .
RUN npm ci
ADD src/Bonsai .
RUN node_modules/.bin/gulp build

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0-alpine as net-builder
ARG TARGETARCH
ARG TARGETVARIANT

WORKDIR /build
ADD src/Bonsai.sln .
ADD src/Bonsai/Bonsai.csproj Bonsai/
ADD src/Bonsai.Tests.Search/Bonsai.Tests.Search.csproj Bonsai.Tests.Search/

RUN --mount=type=cache,id=nuget-$TARGETARCH$TARGETVARIANT,sharing=locked,target=/root/.nuget/packages \
    dotnet restore -a ${TARGETARCH/amd64/x64}
COPY --from=node /build .

RUN --mount=type=cache,id=nuget-$TARGETARCH$TARGETVARIANT,sharing=locked,target=/root/.nuget/packages \
    dotnet publish Bonsai/Bonsai.csproj \
      --output ../out/ \
      --configuration Release \
      -a ${TARGETARCH/amd64/x64} \
      --self-contained true

FROM --platform=$TARGETPLATFORM alpine:latest
ARG TARGETARCH
ARG TARGETVARIANT
ARG BUILD_COMMIT

RUN --mount=type=cache,id=apk-$TARGETARCH$TARGETVARIANT,sharing=locked,target=/var/cache/apk \
    apk --update add \
      nodejs \
      ffmpeg \
      libintl \
      icu \
      icu-data-full \
    && apk add libgdiplus --repository=https://dl-cdn.alpinelinux.org/alpine/edge/testing/

WORKDIR /app
COPY --from=net-builder /out .

RUN mkdir /app/App_Data && chmod +w /app/App_Data
RUN mkdir /app/External/ffmpeg
RUN ln -s /usr/bin/ffmpeg /app/External/ffmpeg/ffmpeg && \
    ln -s /usr/bin/ffprobe /app/External/ffmpeg/ffprobe && \
    chmod +x /app/Bonsai

ENV ASPNETCORE_ENVIRONMENT=Production
ENV BuildCommit=$BUILD_COMMIT

EXPOSE 80

ENTRYPOINT ["/app/Bonsai"]
