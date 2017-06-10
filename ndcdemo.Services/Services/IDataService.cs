using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NdcDemo.Services.Dtos;
using NdcDemo.Services.Providers;

namespace NdcDemo.Services
{
	public interface IDataService
	{
		string PostMessage(Message message);
		Task<IEnumerable<Message>> GetMessagesAsync();
		IObservableHandle ObserveMessages(Action<ObservationType, Message> callback);
	}
}