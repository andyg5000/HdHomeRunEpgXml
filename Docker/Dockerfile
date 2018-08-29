FROM alpine:latest

# Install dependencies from apk package manager.
RUN apk update && \
    apk upgrade && \
    apk --no-cache add python3 curl

# Install python3, pip and urllib3 dependencies.
RUN curl https://bootstrap.pypa.io/get-pip.py -o get-pip.py && \
    python3 get-pip.py && \
    pip install urllib3

# Download the python script from github.
RUN curl https://raw.githubusercontent.com/FairfieldTekLLC/HdHomeRunEpgXml/master/hdhomerun.py -o hdhomerun.py

# Expose the command.
CMD python3 hdhomerun.py > /dev/null 2>&1 ; cat /hdhomerun.xml
