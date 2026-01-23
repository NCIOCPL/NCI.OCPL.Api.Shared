# inspired in part by https://github.com/alpine-docker/jmeter/blob/master/Dockerfile
# and https://github.com/NCIOCPL/wcms-cde-load-test

FROM alpine:3.22

ARG JMETER_VERSION="5.6.3"
ENV JMETER_HOME=/opt/apache-jmeter-${JMETER_VERSION}
ENV	JMETER_BIN=${JMETER_HOME}/bin
ENV	JMETER_DOWNLOAD_URL=https://archive.apache.org/dist/jmeter/binaries/apache-jmeter-${JMETER_VERSION}.tgz

RUN apk add --no-cache \
            ca-certificates \
            tzdata \
            openjdk8 \
            curl \
            unzip \
            bash \
            nss

# Set JAVA_HOME
ENV JAVA_HOME=/usr/lib/jvm/java-1.8-openjdk

# Set timezone
ENV TZ=America/New_York

# Get the NIH Root certificates and install them so we work on VPN
# See https://myitsm.nih.gov/kb_view_customer.do?sysparm_article=KB0020936
RUN mkdir -p /usr/local/share/ca-certificates/nih \
    && wget -P /usr/local/share/ca-certificates/ http://nihdpkicrl.nih.gov/CertData/NIH-DPKI-CA-1A.crt \
    && wget -P /usr/local/share/ca-certificates/ http://nihdpkicrl.nih.gov/CertData/NIHDPKIRCA_NIH-DPKI-ROOT-1A.crt \
    && update-ca-certificates \
    && keytool -delete -alias nih-dpki-ca-1a -keystore /etc/ssl/certs/java/cacerts -storepass changeit || true \
    && keytool -importcert -noprompt -trustcacerts \
        -alias nih-dpki-ca-1a \
        -file /usr/local/share/ca-certificates/NIH-DPKI-CA-1A.crt \
        -keystore /etc/ssl/certs/java/cacerts \
        -storepass changeit \
    && keytool -delete -alias nih-dpki-root-1a -keystore /etc/ssl/certs/java/cacerts -storepass changeit || true \
    && keytool -importcert -noprompt -trustcacerts \
        -alias nih-dpki-root-1a \
        -file /usr/local/share/ca-certificates/NIHDPKIRCA_NIH-DPKI-ROOT-1A.crt \
        -keystore /etc/ssl/certs/java/cacerts \
        -storepass changeit

# Download and install JMeter
RUN mkdir -p /tmp/dependencies  \
    && curl -L --silent ${JMETER_DOWNLOAD_URL} >  /tmp/dependencies/apache-jmeter-${JMETER_VERSION}.tgz  \
    && mkdir -p /opt  \
    && tar -xzf /tmp/dependencies/apache-jmeter-${JMETER_VERSION}.tgz -C /opt  \
    && rm -rf /tmp/dependencies

# Set global PATH such that "jmeter" command is found
ENV PATH=$PATH:$JMETER_BIN

COPY entrypoint.sh /root/entrypoint.sh
RUN chmod +x /root/entrypoint.sh

ENTRYPOINT ["/root/entrypoint.sh"]


WORKDIR	/root
