# GameServer
FROM ubuntu:20.04

WORKDIR /usr/src/app
COPY rcc-docker/ ./RCCService
COPY ./docker/start.sh .
RUN chmod +x ./start.sh
RUN echo "If you can read this file, please report the vulnerability via matrix to <redacted>\n" >> ./RCCService/readme.txt

RUN apt-get update && apt-get install -y libsdl2-2.0-0 libosmesa6 libpng16-16 libx11-6 libcrypto++6 libssl1.1 libopenal1 libgl1 libcrypt1 liblz4-1 libcurl3-gnutls libcurl4 socat wget xvfb libc6 libstdc++6

ENTRYPOINT ["/usr/src/app/start.sh"]
