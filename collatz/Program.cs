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
		static ulong _history_size = 83333333;
		static int[] _history;

		static void Main(string[] args)
		{
			//arg 1: number to test (in millions)
			//arg 2: number of parallel iterations
			//arg 3: history_size (in millions)

			Stopwatch stopWatch = new Stopwatch();
			stopWatch.Start();

			if (args.Length > 0) _numberToTest = Int32.Parse(args[0]) * 1000000;
			if (args.Length > 1) _blockCount = Int32.Parse(args[1]);
			if (args.Length > 0) _history_size = (ulong)(Int32.Parse(args[2]) * 1000000);

			_history = new int[_history_size];
			int blockSize = _numberToTest / _blockCount;

			bool success = TestCollatz(_blockCount, blockSize);
			stopWatch.Stop();
			TimeSpan tsOverall = stopWatch.Elapsed;

			string runTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", tsOverall.Hours, tsOverall.Minutes, tsOverall.Seconds, tsOverall.Milliseconds / 10);

			Console.WriteLine($"NumberToTest: {_numberToTest}, HISTORY SIZE: {_history_size}, BLOCKCOUNT: {_blockCount}, BLOCKSIZE: {blockSize}");
			Console.WriteLine($"RUN TIME: {runTime}");
			Console.WriteLine(success ? "SUCCESS:" : "FAILURE");

			Console.WriteLine("Press any key to exit ...");
			Console.ReadLine();
		}

		private static bool TestCollatz(int blockCount, int blockSize)
		{
			try
			{
				Parallel.For(0, blockCount, b => TestCollatzBlock((b * blockSize) + 1, blockSize));
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

		private static void TestCollatzBlock(int start, int blockSize)
		{
			int r;
			ulong itr;
			int historyNum;

			for (r = start; r < start + blockSize; r++)
			{
				itr = (ulong)r;

				// loop while not a power of 2
				while ((itr & (itr - 1)) != 0)
				{
					//Console.WriteLine($"R: {r}, ITR: {itr}");

					if (itr < _history_size)
					{
						historyNum = _history[itr];

						if (historyNum == 0 || historyNum > r)
						{
							// historyNum == 0 : we have not met this number before
							// historyNum > r  : we have met this number before in a chain with seed > r. Let the lower seeded chain follow the chain to the end
							_history[itr] = r;
							goto NextIteration;
						}

						if (historyNum < r)
							break; // found in another chain. Let the lower seeded chain follow the chain to the end

						throw new InvalidDataException($"FAILURE ({r})"); // historyNum == r means already found in this iteration so this is a failure
					}

				NextIteration:
					itr = (itr & 1) == 0
						? itr = itr / 2
						: itr = (itr * 3) + 1;
				}
			}
		}
	}
}
