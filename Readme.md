Console app to test the Collatz Conjecture for all numbers up to a provided max number.

Call it by running bin/Release/collatz.exe with parameters.

Arguments:

- arg 1 &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;= required: number to test (in millions)
- '-xc \<int\>' = optional: do not calculate longest chain. This paramater has an optional \<int\> to set the size of the history array in millions (defaults to 40)
  
eg.
  - collatz.exe 10 &nbsp;  &nbsp;  &nbsp;  &nbsp;  &nbsp;  &nbsp;// test all numbers between 1 and 10 million. Calculates the longest chain.
  - collatz.exe 10 -xc &nbsp;  &nbsp;  &nbsp;// test all numbers between 1 and 10 million. Do not calculate longest chain. History array set to defaults size 40 million
  - collatz.exe 10 -xc 10 // test all numbers between 1 and 10 million. Do not calculate longest chain. History array set to size 10 million
