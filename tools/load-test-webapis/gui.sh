#!/bin/bash

mkdir -p results

jmeter  -t "./test-plan.jmx" \
        -q "./config/config.properties" \
        -l "./results/result.jtl" \
        $@