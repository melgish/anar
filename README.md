# Anar

 [![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](./LICENSE)
 [![.NET](https://github.com/melgish/anar/actions/workflows/dotnet.yml/badge.svg)](https://github.com/melgish/anar/actions/workflows/dotnet.yml)
 [![CodeQL](https://github.com/melgish/anar/actions/workflows/github-code-scanning/codeql/badge.svg)](https://github.com/melgish/anar/actions/workflows/github-code-scanning/codeql)
 [![codecov](https://codecov.io/github/melgish/anar/graph/badge.svg?token=Q7HPKX12NH)](https://codecov.io/github/melgish/anar)

On Tolkien's Middle-Earth, Anar is the sun. A vessel piloted through
the heavens by the Maia Arien after the fall of the Two Trees.

Anar monitors my own solar system [heh] by gathering data on my inverters and
uploading it an InfluxDB v2 database.  This project was geared towards creating
a .NET project the could be configured and run in a docker swarm.

# Configuration
Like most .NET apps, you can specify all configuration settings using a
combination of config files `appsettings.json`, environment variables, or
command line options.

In addition, this project supports using Docker configs and secrets as follows.
An environment variable APP_SETTINGS_DOCKER can be set to identify an
additional location for an appsettings style config file.  This can be
`/config-name` to load from a docker config, or `/run/secrets/secret-name` to
load from a docker secret.

## Gateway
This section controls settings for communicating with an Enphase IQ gateway.
Enphase does not make direct access easy.

### Gateway : Thumbprint (string)
This is the SHA-1 value of the self-signed certificate of your Enphase gateway.
You can capture this by reviewing browser security settings.  Value is a stream
of hex digits with no punctuation.

### Gateway : Token (string)
This is the security token the system should use when accessing your gateway.
[This Technical Brief](https://enphase.com/download/accessing-iq-gateway-local-apis-or-local-ui-token-based-authentication)
describes one way to get one.

### Gateway : Uri (string)
This is the web address of your gateway. Usually it is going to be a local IP
like `https://192.168.1.10`, but your setup may be different than mine.

## Influx
These options control how the app will connect to an InfluxDB database.

### Influx : Bucket (string)
This is the data bucket to insert data to.

### Influx : Organization (string)
This is the organization associated with the token.

### Influx : Token (string)
This is the InfluxDB authentication token.

### Influx : Uri
This is the web address of your database. For example `http://influxdb:8086`.
Your setup may be different than mine.

## Locator
These options control additional data that can be joined with inverter data
from the gateway.

### Locator : LayoutFile (string)
This optional setting provides an alternative to Layout above. Instead of
embedding the location array directly, layout information can be imported from
`array_layout_x.json` which is one of the files downloaded in the background
when you view your system on the
[Enlighten Website](https://enlighten.enphaseenergy.com/). You'll need to use
browser developer tools to capture this file.

## Worker
These settings control how often the worker will capture and upload data.

### Worker : Interval (TimeSpan) default = 0.00:05:00
This setting controls how often to poll the data from the gateway.

# Building
Most of the ways you can build .NET apps will work. In addition a compose.yaml
file in the folder can be done to build a docker container

## Via Command line
```shell
# make sure appsettings.json has been properly filled out.
dotnet build
dotnet run --project Anar
```

## Via Docker
```shell
# make sure all your settings have been configured.
docker compose build
docker compose up -d
```