Console app to test the Collatz Conjecture for all numbers up to a provided max number.

*Note: readme instructions based on Glen Hardings readme.*
## Pre-requisites

To build this application you will require the .NET 6 SDK.

## Building

**A pre-built version for Linux ARM64 (e.g. EC2 m6g) is in https://github.com/dm64emis/collatz/tree/master/_release/net6.0/linux-arm64)**

To build for your machine runtime:

- `dotnet build --configuration release`

If you want to package the application so it doesn't need the .NET 6 runtime installed on the target machine:

- `dotnet publish --configuration release`

To build for specific runtime (e.g. ARM based AWS Graviton):

- `dotnet build --configuration release -r linux-arm64 --self-contained`

## Deploying

1. Copy the build output to the machine that will run it.
2. In the application directory, run `chmod 777 Collatz`

**For Amazon Linux:**

1. Run `sudo yum update`
2. Run `sudo yum install libicu60`
## Running
Call it by running bin/Release/collatz.exe with parameters.

Arguments:

- arg 1 &nbsp; &nbsp; &nbsp; &nbsp;= required: number to test (in millions)
- '-x' &nbsp;  &nbsp;  &nbsp;  &nbsp;  &nbsp;  = optional: do not calculate longest chain
- '-h \<int\>' = optional: set the size of the history array in millions (defaults to 40)
- '-o' &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; = optional: find the optimal value for h by running the test with h = 10 to 130 in steps of 10 (-h parameter ignored if -o provided)
  
eg.
  - collatz.exe 10 &nbsp; &nbsp; &nbsp; &nbsp;  &nbsp;  &nbsp;  &nbsp;= test all numbers up to 10 million. Calculates the longest chain. . History array set to default size 40 million
  - collatz.exe 10 -h 10 &nbsp; &nbsp; = test all numbers up to 10 million. Calculates the longest chain. . History array set to size 10 million
  - collatz.exe 10 -x &nbsp; &nbsp; &nbsp; &nbsp;  &nbsp;= test all numbers up to 10 million. Do not calculate longest chain. History array set to default size 40 million
  - collatz.exe 10 -x -h 10 = test all numbers up to 10 million. Do not calculate longest chain. History array set to size 10 million
