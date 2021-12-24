using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Collatz
{
	class Program
	{

		static int _numberToTest;
		static long _history_size = 40000000;
		static bool _calculateChain = true;
		static long _maxChainSeed = 1;
		static long _maxChainLength = 1;

		static void Main(string[] args)
		{
			Stopwatch stopWatch = new Stopwatch();

			stopWatch.Start();

			if (!TryParseArgs(args))
			{
				Console.WriteLine(@"Error parsing parameters.
Arguments:
arg1       = required: number to test in millions (defaults to 100 million)
'-x'       = optional: do not calculate longest chain.
'-h <int>  = optional: set the size of the history array in millions (defaults to 40)");

				return;
			}

			if (_calculateChain)
				TestCollatz_c1();
			else
				TestCollatz_xc0();

			stopWatch.Stop();
			ReportSummary(stopWatch.Elapsed);
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
					case "-x":
						_calculateChain = false;
						break;
					case "-h":
						i++;
						_history_size = Int32.Parse(args[i]) * 1000000;
						break;
					default:
						return false;
				}

				i++;
			}

			return true;
		}

		private static void TestCollatz_xc0()
		{
			// does not calculate the longest chain. Uses _history array
			long[] history = new long[_history_size];

			Parallel.For(1, _numberToTest + 1, seed => {
				long itr = seed;

				while (itr > 1)
				{
					if (itr < _history_size)
					{
						long historyNum = history[itr];

						if (historyNum == 0 || historyNum > seed)
						{
							// historyNum == 0:   we have not met this number before
							// historyNum > seed: this number already found in a chain with a higher seed value. Follow to the end of the chain here as we are the lower seed value
							// note: no need to lock the array. Clashes will resolve themselves in later iterations
							history[itr] = seed;
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

		private static void TestCollatz_c0()
		{
			Object lockObject = new Object();

			// calculate the longest chain. Without history array.
			Parallel.For(1, _numberToTest + 1, seed => {
				long chainLength = 1;
				long itr = seed;

				while (itr > 1)
				{
					itr = (itr & 1) == 0
						? itr = itr / 2
						: itr = (itr * 3) + 1;
					chainLength++;
				}

				if(chainLength > _maxChainLength)
				{
					lock(lockObject)
					{
						_maxChainLength = chainLength;
						_maxChainSeed = seed;
					}
				}
			});
		}

		private static void TestCollatz_c1()
		{
			// calculate the longest chain. Uses history array.
			Object lockObject = new Object();
			long[] history = new long[_history_size];

			Parallel.For(1, _numberToTest + 1, seed => {
				long itr = seed;
				long chainLength = 1;

				while (itr > 1)
				{
					if (itr < _history_size)
					{
						if (chainLength <= history[itr])
							return; // a longer chain already passed through this number

						history[itr] = chainLength; // locking not necessary. If the update fails due to a clash, it means the other seed loops may have more iterations than optimal. Adding a lock slows it down considerably
					}

					itr = (itr & 1) == 0
						? itr = itr / 2
						: itr = (itr * 3) + 1;
					chainLength++;
				}

				if (chainLength > _maxChainLength)
					lock (lockObject)
					{
						_maxChainSeed = seed;
						_maxChainLength = chainLength;
					}
			});
		}

		private static void TestCollatz_history_size(Stopwatch stopWatch)
		{
			TimeSpan tsMin = new TimeSpan(0);
			int hMin = 0;
			string runTime;

			for (int h = 1; h < 100; h++)
			{
				_history_size = h * 1000000;
				stopWatch.Reset();
				stopWatch.Start();
				TestCollatz_c1();
				stopWatch.Stop();
				TimeSpan ts = stopWatch.Elapsed;

				if (ts < tsMin)
				{
					tsMin = ts;
					hMin = h;
				}

				runTime = String.Format("{0:00}.{1:00}", ts.Seconds, ts.Milliseconds / 10);
				Console.WriteLine($"Run Time: {runTime}, h: {h}");
			}

			runTime = String.Format("{0:00}.{1:00}", tsMin.Seconds, tsMin.Milliseconds / 10);
			Console.WriteLine($"Min Run Time: {runTime}, h: {hMin}");
		}

		private static void ReportSummary(TimeSpan ts)
		{
			string runTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);

			Console.WriteLine($"{"Number Under Test:", -25} {_numberToTest}");
			Console.WriteLine($"{"Run Time:", -25} {runTime}");

			Console.WriteLine($"{"Calculate Chain:", -25} {_calculateChain}");

			if (_calculateChain)
			{
				Console.WriteLine($"{"Longest Chain Seed:", -25} {_maxChainSeed}");
				Console.WriteLine($"{"Longest Chain Length:", -25} {_maxChainLength}");
			}

			Console.WriteLine($"{"History Size:", -25} {_history_size}");

			Console.WriteLine("Press any key to exit ...");
			Console.ReadLine();
		}
	}
}
