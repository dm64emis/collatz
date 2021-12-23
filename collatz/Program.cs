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
		static ulong _history_size = 100000000;
		static int[] _history;
		static bool _calculateChain = true;
		static long _maxChainSeed = 0;
		static long _maxChainLength = 0;
		static object _lockObject = new object();

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
'nc'       = optional: do not calculate longest chain
'-b <int>' = optional: number of parallel iterations (defaults to 40)
'-h <int>' = optional: size of history array in millions (defaults to 100)");
                
				return;
			}

			_history = new int[_history_size];
			int blockSize = _numberToTest / _blockCount;

			bool success = TestCollatz(_blockCount, blockSize);

			stopWatch.Stop();
			TimeSpan tsOverall = stopWatch.Elapsed;
			string runTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", tsOverall.Hours, tsOverall.Minutes, tsOverall.Seconds, tsOverall.Milliseconds / 10);

			Console.WriteLine(success ? "SUCCESS" : "FAILURE");
			Console.WriteLine($"{"Number Under Test:", -25} {_numberToTest}");
			Console.WriteLine($"{"Run Time:", -25} {runTime}");

			if (_calculateChain)
			{
				Console.WriteLine($"{"Longest Chain Seed:", -25} {_maxChainSeed}");
				Console.WriteLine($"{"Longest Chain Length:", -25} {_maxChainLength}");
			}

			// Console.WriteLine($"{"History Size:", -25} {_history_size}");
			// Console.WriteLine($"{"Parallel Block Count:", -25} {_blockCount}");
			// Console.WriteLine($"{"Block Size:", -25} {blockSize}");

			Console.WriteLine("Press any key to exit ...");
			Console.ReadLine();
		}

		private static bool TryParseArgs(string[] args)
		{
			if (args.Length == 0)
				return false;

			_numberToTest = Int32.Parse(args[0]) * 1000000;

			int i = 1;

			while (i < args.Length)
			{
				switch (args[i])
				{
					case "nc":
						_calculateChain = false;
						break;
					case "-b":
						_blockCount = Int32.Parse(args[i + 1]);
						i++;
						break;
					case "-h":
						_history_size = (ulong)(Int32.Parse(args[i + 1]) * 1000000);
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

		private static bool TestCollatz(int blockCount, int blockSize)
		{
			try
			{
				if (_calculateChain)
				{
					long[] maxChain = new long[_numberToTest + 1];

					Parallel.For(
						fromInclusive: 0,
						toExclusive: blockCount,
						body: (b) => TestCollatzBlockWithChainCalc((b * blockSize) + 1, blockSize, maxChain));

					for (long lng = 1; lng <= _numberToTest; lng++)
					{
						if (maxChain[lng] > _maxChainLength)
						{
							_maxChainLength = maxChain[lng];
							_maxChainSeed = lng;
						}
					}
				}
				else
					Parallel.For(
						fromInclusive: 0,
						toExclusive: blockCount,
						body: (b) => TestCollatzBlockNoChainCalc((b * blockSize) + 1, blockSize));
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

		private static void TestCollatzBlockWithChainCalc(int start, int blockSize, long[] maxChain)
		{
			ulong itr = 1;
			long chainLength = 0;
			int historyNum;

			for (int seed = start; seed < start + blockSize; seed++)
			{
				chainLength = 0;
				itr = (ulong)seed;

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

		private static void TestCollatzBlockNoChainCalc(int start, int blockSize)
		{
			ulong itr = 1;
			int historyNum;

			for (int seed = start; seed < start + blockSize; seed++)
			{
				itr = (ulong)seed;

				// loop while not a power of 2
				while ((itr & (itr - 1)) != 0)
				{
					//Console.WriteLine($"R: {r}, ITR: {itr}");

					if (itr < _history_size)
					{
						historyNum = _history[itr];

						if (historyNum == 0)
							_history[itr] = seed; // we have not met this number before
						else if (historyNum > seed)
							_history[itr] = seed; // this number already found in a chain with a higher seed value. Follow to the end of the chain here as we are the lower seed value
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

		private static long WhatPowerOf2(ulong num)
		{
			long exponent = 0;

			while (num != 1)
			{
				num = num >> 1;
				exponent++;
			}

			return exponent;
		}
	}
}
