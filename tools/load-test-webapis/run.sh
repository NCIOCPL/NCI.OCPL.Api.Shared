#!/bin/bash

mkdir -p results

docker compose run --rm --remove-orphans jmeter
