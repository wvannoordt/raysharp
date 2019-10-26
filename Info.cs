using System;
using System.Diagnostics;
namespace raysharp
{
	public static class Info
	{
		public static void Kill(object sender, string message)
		{
			throw new Exception("[" + (sender.GetType().Name.ToLower() == "string" ? sender.ToString() : sender.GetType().Name) + "] Error: " + message);
		}
		public static void Warning(object sender, string message)
		{
			ConsoleColor pre = Console.ForegroundColor;
			Console.ForegroundColor = ConsoleColor.Yellow;
			string outputmessage = "[" + (sender.GetType().Name.ToLower() == "string" ? sender.ToString() : sender.GetType().Name) + "] Warning: " + message;
			Console.WriteLine(outputmessage);
			Console.ForegroundColor = pre;
		}

		public static void Write(object message, ConsoleColor color)
		{
			ConsoleColor pre = Console.ForegroundColor;
			Console.ForegroundColor = color;
			Console.Write(message);
			Console.ForegroundColor = pre;
		}

		public static void Write(object message, int color)
		{
			Write(message, (ConsoleColor)color);
		}

		public static void Write(object message, string color)
		{
			ConsoleColor mcolor;
			Enum.TryParse(color, true, out mcolor);
			Write(message, mcolor);
		}

		public static void WriteLine(object message, ConsoleColor color)
		{
			ConsoleColor pre = Console.ForegroundColor;
			Console.ForegroundColor = color;
			Console.WriteLine(message);
			Console.ForegroundColor = pre;
		}

		public static void WriteLine(object message, int color)
		{
			WriteLine(message, (ConsoleColor)color);
		}

		public static void WriteLine()
		{
			Console.WriteLine();
		}

		public static void WriteLine(object message)
		{
			Console.WriteLine(message);
		}


		public static void WriteLine(object message, string color)
		{
			ConsoleColor mcolor;
			Enum.TryParse(color, true, out mcolor);
			WriteLine(message, mcolor);
		}

		public static string ReadLine()
		{
			return Console.ReadLine();
		}
	}
}
