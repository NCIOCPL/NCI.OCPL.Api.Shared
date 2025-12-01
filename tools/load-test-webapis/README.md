# Web API Load Tester

Uses a containerized instance of [Apache JMeter](https://jmeter.apache.org/) to generate traffic against
a selected instance of the CancerGov webapis, which in turn create load against the configured Elasticsearch
cluster.

## How to run

Run `run.sh`.  This launches JMeter in a batch mode.

Alternatively, if you have JMeter installed, and an appropriate version of Java, you can run `gui.sh` to
launch the interactive GUI for editing/debugging the test plan.

## Limitations

The main concern for this tool is to generate traffic.  It does not verify the API response beyond checking for a
status 200 response code.  Additionally, it does not exercise 100% of the endpoints.  (E.g. There are no calls to the
the various health checks, nor to the endpoints for retrieving specific terms from the dictionary.)

It does however emulate the sitewide search application by calling the sitewide search, best bets, and glossary search APIs
for the same term.

## Configuration

There are two configuration points in `config/config.properties`.

- `protocol` - `https` vs. `http`.  You'll generally only use `http` when running against local development.
- `host` - This is the fully-qualified hostname to run against (e.g. webapis-stage.cancer.gov)

## TODO

Figure out what sort of reporting we want.  For now, this is just traffic generation.