namespace NdcDemo.Services
{
	public interface ILogger
	{
		void Debug(string message);
		void Debug(string formattedMessage, params object[] args);
	}
}
