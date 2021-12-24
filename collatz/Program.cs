using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Collatz
{
	class Program
	{

		static int _numberToTest = 100000000;
		static int _blockCount = 40;
		static long _history_size = 100000000;
		static long[] _history;
		static string _version = "c6";
		static long _maxChainSeed = 0;
		static long _maxChainLength = 0;
		static object _lockObject = new object();
		static object _lockObject1 = new object();

		static void Main(string[] args)
		{
			//arg 1:	required: number to test (in millions)
			//nc:		optional: do not calculate longest chain
			//-b <int>: optional: number of parallel iterations
			//-h <int>: optional: history_size (in millions)

			Stopwatch stopWatch = new Stopwatch();
			
			stopWatch.Start();
			
			if (!TryParseArgs(args))
			{
				Console.WriteLine(@"Error parsing parameters.
Arguments:
arg1       = required: number to test in millions
'-b <int>' = optional: number of parallel iterations (defaults to 40)
'-h <int>' = optional: size of history array in millions (defaults to 100)
'-v <int>' = optional: version to run (defaults to nc0 - simplest approach)");
                
				return;
			}

			bool success = true;

			if (_version == "c0")
				TestCollatz_c0();
			else
				success = TestCollatz();

			stopWatch.Stop();
			TimeSpan tsOverall = stopWatch.Elapsed;
			string runTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", tsOverall.Hours, tsOverall.Minutes, tsOverall.Seconds, tsOverall.Milliseconds / 10);

			Console.WriteLine(success ? "SUCCESS" : "FAILURE");
			Console.WriteLine($"{"Number Under Test:", -25} {_numberToTest}");
			Console.WriteLine($"{"Run Time:", -25} {runTime}");


			Console.WriteLine($"{"Version:", -25} {_version}");
			Console.WriteLine($"{"Longest Chain Seed:", -25} {_maxChainSeed}");
			Console.WriteLine($"{"Longest Chain Length:", -25} {_maxChainLength}");

			// Console.WriteLine($"{"History Size:", -25} {_history_size}");
			// Console.WriteLine($"{"Parallel Block Count:", -25} {_blockCount}");
			// Console.WriteLine($"{"Block Size:", -25} {blockSize}");

			Console.WriteLine("Press any key to exit ...");
			Console.ReadLine();
		}

		private static bool TryParseArgs(string[] args)
		{
			if (args.Length == 0)
				return true;

			_numberToTest = Int32.Parse(args[0]) * 1000000;

			int i = 1;

			while (i < args.Length)
			{
				switch (args[i])
				{
					case "-v":
						_version = args[i + 1];
						i++;
						break;
					case "-b":
						_blockCount = Int32.Parse(args[i + 1]);
						i++;
						break;
					case "-h":
						_history_size = Int32.Parse(args[i + 1]); // * 1000000);
						i++;
						break;
					default:
						Console.WriteLine($"Unknown argument {args[i]}");
						return false;
				}

				i++;
			}

			return true;
		}

		private static bool TestCollatz()
		{
			try
			{
				int blockSize = _numberToTest / _blockCount;

				switch (_version)
				{
					case "nc1":					
						_history = new long[_history_size];

						Parallel.For(
							fromInclusive: 0,
							toExclusive: _blockCount,
							body: (b) => TestCollatzBlock_nc1((b * blockSize) + 1, blockSize));
						break;
					case "nc2":					
						_history = new long[_history_size];

						Parallel.For(
							fromInclusive: 0,
							toExclusive: _blockCount,
							body: (b) => TestCollatzBlock_nc2((b * blockSize) + 1, blockSize));
						break;
					case "nc0":					
						TestCollatz_nc0();
						break;
					case "nc4":					
						TestCollatz_nc4();
						break;
					case "c0":
						TestCollatz_c0();
						break;
					case "c2":					
						_history = new long[_history_size];
						long[] maxChain2 = new long[_numberToTest + 1];

						Parallel.For(
							fromInclusive: 0,
							toExclusive: _blockCount,
							body: (b) => TestCollatzBlock_c2((b * blockSize) + 1, blockSize, maxChain2));
						break;
					case "c3":					
						long[] maxChain3 = new long[_numberToTest + 1];

						Parallel.For(
							fromInclusive: 0,
							toExclusive: _blockCount,
							body: (b) => TestCollatzBlock_c3((b * blockSize) + 1, blockSize, maxChain3));
						break;
					case "c4":					
						Parallel.For(
							fromInclusive: 0,
							toExclusive: _blockCount,
							body: (b) => TestCollatzBlock_c4((b * blockSize) + 1, blockSize));
						break;
					case "c5":					
						Parallel.For(
							fromInclusive: 0,
							toExclusive: _blockCount,
							body: (b) => TestCollatzBlock_c5((b * blockSize) + 1, blockSize));
						break;
					case "c6":					
						TestCollatz_c6();
						break;
				}
			}
			catch (AggregateException ex)
			{
				foreach (var ie in ex.InnerExceptions)
				{
					Console.WriteLine(ie.Message);
				}

				return false;
			}

			return true;
		}

		private static void TestCollatzBlock_nc1(int start, int blockSize)
		{
			// does not calculate the longest chain. Use blocks and _history array
			long itr = 1;
			long historyNum;

			for (int seed = start; seed < start + blockSize; seed++)
			{
				itr = seed;

				// loop while not a power of 2
				while ((itr & (itr - 1)) != 0)
				{
					//Console.WriteLine($"R: {r}, ITR: {itr}");

					if (itr < _history_size)
					{
						historyNum = _history[itr];

						if (historyNum == seed)
							throw new InvalidDataException($"FAILURE ({seed})"); // we have dropped into a loop

						if (historyNum > seed)
							break; // this number already found in a chain with a lower seed value. No need to follow to the end as we are not calculating chain length

						// historyNum == 0:   we have not met this number before
						// historyNum > seed: this number already found in a chain with a higher seed value. Follow to the end of the chain here as we are the lower seed value
						_history[itr] = seed; 
					}

					itr = (itr & 1) == 0
						? itr = itr / 2
						: itr = (itr * 3) + 1;
				}
			}
		}

		private static void TestCollatzBlock_nc2(int start, int blockSize)
		{
			// does not calculate the longest chain. Use blocks and _history array
			long itr = 1;
			long historyNum;

			for (int seed = start; seed < start + blockSize; seed++)
			{
				itr = seed;

				// loop while not a power of 2
				while ((itr & (itr - 1)) != 0)
				{
					//Console.WriteLine($"R: {r}, ITR: {itr}");

					if (itr < _history_size)
					{
						historyNum = _history[itr];

						if (historyNum == 0 || historyNum > seed)
						{
							// historyNum == 0:   we have not met this number before
							// historyNum > seed: this number already found in a chain with a higher seed value. Follow to the end of the chain here as we are the lower seed value
							_history[itr] = seed; 
						}
						else if (historyNum == seed)
							throw new InvalidDataException($"FAILURE ({seed})"); // we have dropped into a loop
						else
							break; // this number already found in a chain with a lower seed value. No need to follow to the end as we are not calculating chain length
					}

					itr = (itr & 1) == 0
						? itr = itr / 2
						: itr = (itr * 3) + 1;
				}
			}
		}

		private static void TestCollatz_nc0()
		{
			// does not calculate the longest chain. Does not use blocks. Uses _history array
			_history = new long[_history_size];

			Parallel.For(1, _numberToTest + 1, seed => {
				long itr = seed;

				while (itr > 1)
				{
					if (itr < _history_size)
					{
						long historyNum = _history[itr];

						if (historyNum == 0 || historyNum > seed)
						{
							// historyNum == 0:   we have not met this number before
							// historyNum > seed: this number already found in a chain with a higher seed value. Follow to the end of the chain here as we are the lower seed value
							_history[itr] = seed; 
						}
						else if (historyNum < seed)
							break; // this number already found in a chain with a lower seed value. No need to follow to the end as we are not calculating chain length
						else
							throw new InvalidDataException($"FAILURE ({seed})"); // historyNum == seed so we have dropped into a loop
					}

					itr = (itr & 1) == 0
						? itr = itr / 2
						: itr = (itr * 3) + 1;
				}
			});
		}

		private static void TestCollatz_nc4()
		{
			// does not calculate the longest chain. Does not use blocks or _history array

			Parallel.For(1, _numberToTest + 1, seed => {
				long itr = seed;

				while (itr > 1)
				{
					itr = (itr & 1) == 0
						? itr = itr / 2
						: itr = (itr * 3) + 1;
				}
			});
		}

		private static void TestCollatz_c0()
		{
			// calculates the longest chain. Does not use blocks or _history array. Uses locking rather than _maxChain array
			Parallel.For(1, _numberToTest + 1, seed => {
				long chainLength = 0;
				long itr = seed;

				while (itr > 1)
				{
					chainLength++;

					itr = (itr & 1) == 0
						? itr = itr / 2
						: itr = (itr * 3) + 1;
				}

				if(chainLength > _maxChainLength)
				{
					lock(_lockObject)
					{
						_maxChainLength = chainLength;
						_maxChainSeed = seed;
					}
				}
			});
		}

		private static void TestCollatzBlock_c2(int start, int blockSize, long[] maxChain)
		{
			// calculates the longest chain. Uses blocks and _history array
			long itr = 1;
			long chainLength = 0;
			long historyNum;

			for (int seed = start; seed < start + blockSize; seed++)
			{
				chainLength = 0;
				itr = seed;

				// loop while not a power of 2
				while ((itr & (itr - 1)) != 0)
				{
					if (itr < _history_size)
					{
						historyNum = _history[itr];

						if (historyNum == 0)
							_history[itr] = seed; // we have not met this number before
						else if (historyNum > seed)
							_history[itr] = seed; // this number already found in a chain with a higher seed value. Follow to the end of the chain here as we are the lower seed value
						else if (historyNum == seed)
							throw new InvalidDataException($"FAILURE ({seed})"); // we have dropped into a loop
					}

					chainLength++;

					itr = (itr & 1) == 0
						? itr = itr / 2
						: itr = (itr * 3) + 1;
				}

				maxChain[seed] = chainLength + WhatPowerOf2(itr);
			}
		}

		private static void TestCollatzBlock_c3(int start, int blockSize, long[] maxChain)
		{
			// calculates the longest chain. Use blocks. Does not use the _history array
			ulong itr = 1;
			long chainLength = 0;

			for (int seed = start; seed < start + blockSize; seed++)
			{
				chainLength = 0;
				itr = (ulong)seed;

				while (itr > 1)
				{
					chainLength++;

					itr = (itr & 1) == 0
						? itr = itr / 2
						: itr = (itr * 3) + 1;
				}

				maxChain[seed] = chainLength;
			}
		}

		private static void TestCollatzBlock_c4(int start, int blockSize)
		{
			// calculates the longest chain. Uses blocks. Does not use the _history array. Uses locking rather than _maxChain array
			ulong itr = 1;
			long chainLength = 0;

			for (int seed = start; seed < start + blockSize; seed++)
			{
				chainLength = 0;
				itr = (ulong)seed;

				while (itr > 1)
				{
					chainLength++;

					itr = (itr & 1) == 0
						? itr = itr / 2
						: itr = (itr * 3) + 1;
				}

				if(chainLength > _maxChainLength)
                {
                    lock(_lockObject) //make these shared variable updates thread safe
                    {
                        _maxChainLength = chainLength;
                        _maxChainSeed = seed;
                    }
                }
			}
		}

		private static void TestCollatzBlock_c5(int start, int blockSize)
		{
			// calculates the longest chain. Does not use the _history array. Non parallel solution
			ulong itr = 1;
			long chainLength = 0;

			for (int seed = start; seed < start + blockSize; seed++)
			{
				chainLength = 0;
				itr = (ulong)seed;

				while (itr > 1)
				{
					chainLength++;

					itr = (itr & 1) == 0
						? itr = itr / 2
						: itr = (itr * 3) + 1;
				}

				if (chainLength > _maxChainLength)
				{
					_maxChainLength = chainLength;
					_maxChainSeed = seed;
				}
			}
		}

		private static void TestCollatz_c6()
		{
			// calculates the longest chain. Does not use blocks. Uses _history array

			long chainLength = 0;
			_history = new long[_history_size + 1];

			Parallel.For(1, _numberToTest + 1, seed => {
				long itr = seed;

				while (itr > 1)
				{
					if (seed > _history_size)
					{
						chainLength++;
						continue;
					}

					if (chainLength < _history[seed])
						return;

					chainLength++;

					//lock(_lockObject)
					//{
						_history[seed] = chainLength;
					//}

					itr = (itr & 1) == 0
						? itr = itr / 2
						: itr = (itr * 3) + 1;
				}

				if (chainLength > _maxChainLength)
				{
					lock (_lockObject1)
					{
						_maxChainSeed = seed;
						_maxChainLength = chainLength;
					};
				}
			});
		}

		private static long WhatPowerOf2(long num)
		{
			long exponent = 0;

			while ((num = num >> 1) != 1)
			{
				exponent++;
			}

			return exponent;
		}
	}
}
