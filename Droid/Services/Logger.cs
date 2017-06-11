using System;
using NdcDemo.Services;

namespace NdcDemo.Droid
{
	public class Logger : ILogger
	{
		public void Debug(string message)
		{
			Console.WriteLine(message);
		}

		public void Debug(string formattedMessage, params object[] args)
		{
			var message = string.Format(formattedMessage, args);
			Debug(message);
		}
	}
}
