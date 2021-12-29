Console app to test the Collatz Conjecture for all numbers up to a provided max number.

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
