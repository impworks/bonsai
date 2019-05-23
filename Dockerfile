FROM node:lts-alpine as node
WORKDIR /build

ADD package.json .
ADD package-lock.json .
RUN npm ci
ADD . .
RUN node_modules/.bin/gulp build

FROM microsoft/dotnet:2.1-sdk as net-builder
WORKDIR /build
ADD Bonsai.sln .
ADD Bonsai.csproj .

RUN dotnet restore

COPY --from=node /build .
RUN dotnet publish --output ../out/ --configuration Release --runtime linux-x64 Bonsai.csproj

FROM microsoft/dotnet:2.1-aspnetcore-runtime

RUN apt-get -yqq update && \
    apt-get -yqq install ffmpeg libc6-dev libgdiplus libx11-dev && \
    rm -rf /var/lib/apt/lists/*

RUN curl -sL https://deb.nodesource.com/setup_10.x | bash - && \
    apt-get install -y nodejs && \
    rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY --from=net-builder /out .

RUN mkdir /app/External/ffmpeg
RUN ln -s /usr/bin/ffmpeg /app/External/ffmpeg/ffmpeg
RUN ln -s /usr/bin/ffprobe /app/External/ffmpeg/ffprobe

ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "Bonsai.dll"]
