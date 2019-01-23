using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NdcDemo.Services.Dtos;
using NdcDemo.Services.Providers;

namespace NdcDemo.Services
{
	public class DataService : IDataService
	{
		const string PathMessages = "messages";

		readonly IDataProviderFactory _dataProviderFactory;
		readonly ILogger _logger;

		public DataService(IDataProviderFactory dataProviderFactory, ILogger logger)
		{
			_dataProviderFactory = dataProviderFactory;
			_logger = logger;
		}

		#region IDataService Interface Implementation 

		public string PostMessage (Message message)
		{
			var path = PathMessages;
			var provider = _dataProviderFactory.GetProvider<Message>(path);
			var messageId = provider.Create(message);
			return messageId;
		}

		public IObservableHandle ObserveMessages(Action<ObservationType, Message> callback)
		{
			var path = PathMessages;
			var provider = _dataProviderFactory.GetProvider<Message>(path);
			provider.Observe(callback);
			return provider;
		}

		public void CancelObservation(IObservableHandle observableHandle)
		{
			(observableHandle as IObservable)?.CancelObservation();
		}

		#endregion
	}
}
