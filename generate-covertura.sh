#!/bin/bash

# Remove older test results
rm -rf CodeLineCounter.Tests/TestResults

# Collect code coverage
dotnet test --collect:"XPlat Code Coverage;Format=json,lcov,cobertura" --results-directory CodeLineCounter.Tests/TestResults --logger trx;LogFileName=testresults.trx