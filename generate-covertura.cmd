 rem echo off
 echo
 rem - remove older test results
rmdir /s /q CodeLineCounter.Tests\TestResults
 rem - collect code coverage
dotnet test --collect:"XPlat Code Coverage;Format=json,lcov,cobertura"  --results-directory CodeLineCounter.Tests\TestResults --logger trx;LogFileName=testresults.trx