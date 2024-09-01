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
Enphase does not make direct access easy.  See the following
[This Technical Brief](https://enphase.com/download/accessing-iq-gateway-local-apis-or-local-ui-token-based-authentication)
for information on how to generate your token.

```json
"Gateway": {
  // Should not need to be changed. Path for gateway request.
  "RequestPath": "api/v1/production/inverters",
  // The SHA-1 thumbprint of the enphase gateway self signed certificate.
  "Thumbprint": "<THUMBPRINT>",
  // The authentication token enphase requires for gateway requests.
  "Token": "<TOKEN>",
  // The IP address or host name of the enphase gateway
  "Uri": "https://192.168.1.10"
}
```

## Influx
These options control how the app will connect to an InfluxDB database.

```json
"Influx": {
  // The InfluxDB bucket to write data to.
  "Bucket": "testing",
  // The InfluxDB organization for the token owner.
  "Organization": "home",
  // The InfluxDB token to use for authentication.
  "Token": "<TOKEN>",
  // The InfluxDB URI.
  "Uri": "http://influxdb:8086"
}
```

## Locator
This setting is optional, and allows you to provide additional data that can
be joined with inverter data from the gateway.

```json
"Locator": {
  // Optional path to file containing location data for system.
  // Instead of embedding location information directly, view your system on
  // the [Enlighten Website](https://enlighten.enphaseenergy.com/)
  // Open browser tools and then go to the Arrays tab.  You will want to find
  // and copy teh array_layout_x.json file.  Save that and put the path
  // here.
  "LayoutFile": ""
}
```

## Notify
This section is optional. If present, any authentication or certificate errors
will be published to the supplied NTFY host and topic. NOTE: Remove the entire
section if you do not want notifications.

```json
// This section is optional. Remove the entire section if you do not want
// notifications.
"Notify": {
  // The full URL and path to your NTFY server and topic.
  "Uri": "https://ntfy.sh/your-topic-name",
  // The token to use for authenticating you NTFY request.
  // It must have write access to the topic.
  "Token": "<TOKEN>",
  // How often to check for and send notifications. Default is 5 minutes
  "PollInterval": "0.00:05:00",
  // How much time before repeating a notification. Default is 1 day.
  "SpamInterval": "1.00:00:00"
}

```
## Worker
These settings control how often the worker will capture and upload data.
```json
"Worker": {
  // How often to poll the enphase gateway for data. Default is 5 minutes.
  "Interval": "0.00:05:00"
}
```

# Building
Most of the ways you can build .NET apps will work. In addition a compose.yaml
file in the folder can be done to build a docker container

## Via Command line
```shell
dotnet build
dotnet test

# Test with coverage
dotnet test --verbosity normal /p:CollectCoverage=true /p:CoverletOutputFormat=opencover /p:CoverletOutput=../.coverage/

# Generate a coverage report
dotnet reportgenerator -reports:.coverage/coverage.opencover.xml -targetdir:.coverage/report

# Run the project - Make sure appsettings.json has been properly filled out.
dotnet run --project Anar
```

## Via Docker
```shell
# Make sure all your settings have been configured.
docker compose build
docker compose up -d
```