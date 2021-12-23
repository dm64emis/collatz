Console app to test the Collatz Conjecture for all numbers up to a provided max number.

Call it by running bin/Release/collatz.exe with parameters.

Arguments:

- arg 1:	required: number to test (in millions)
- 'nc':		optional: do not calculate longest chain
- '-b' <int>: optional: number of parallel iterations (defaults to 40)
- '-h' <int>: optional: history_size in millions (defaults to 100)
  
eg.
  collatz.exe 10     // tests all numbers between 1 and 10 million. Does not calculate the longest chain
  collatz.exe 10 nc  // test all numbers between 1 and 10 million and calculate longest chain
 
depending on the system it is running, optimisations can be made using the '-b' and '-h' parameters. The defaults were picked based on my laptop performance.
