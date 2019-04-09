using System;
using System.IO;

namespace MainCycle
{
	public static class Console
	{
		private static void RefreshDateAndTime()
		{
			timeStamp = $"[{DateTime.Now.TimeOfDay}]: ";
			dateStamp = $"{DateTime.Today.ToString(@"yyyy-MM\m-dd\d")}";
			_logPath = Path.Combine("D:\\ITouchPushService\\Logs", dateStamp);
			_logFile = Path.Combine(_logPath, "log.txt");
			Directory.CreateDirectory(_logPath);
		}
		private static string timeStamp;
		private static string dateStamp;
		private static string _logPath;
		private static string _logFile;

		public static void WriteLine()
		{
			RefreshDateAndTime();
			File.AppendAllText(_logFile, Environment.NewLine);
			System.Console.WriteLine();
		}

		public static void WriteLine( string line )
		{
			RefreshDateAndTime();
			File.AppendAllText(_logFile, Environment.NewLine + timeStamp + line);
			System.Console.WriteLine(line);
		}

		public static void Write( string line )
		{
			RefreshDateAndTime();
			File.AppendAllText(_logFile, Environment.NewLine + timeStamp + line);
			System.Console.Write(line);
		}

		public static void Write( int n )
		{
			RefreshDateAndTime();
			File.AppendAllText(_logFile, Environment.NewLine + timeStamp + n.ToString());
			System.Console.Write(n);
		}

		public static void WriteLine( string line, object obj )
		{
			RefreshDateAndTime();
			File.AppendAllText(_logFile, Environment.NewLine + timeStamp + line);
			System.Console.WriteLine(line, obj);
		}
		public static void Read()
		{
			System.Console.Read();
		}

		public static void ReadKey()
		{
			System.Console.ReadKey();
		}
		public static void ReadLine()
		{
			System.Console.ReadLine();
		}

		public static void ReadKey(bool key)
		{
			System.Console.ReadKey(key);
		}
	}
}
