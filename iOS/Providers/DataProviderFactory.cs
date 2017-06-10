using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Database;
using NdcDemo.Services;
using NdcDemo.Services.Dtos;
using NdcDemo.Services.Providers;
using Foundation;
using Newtonsoft.Json;

namespace Firebase.iOS.Providers
{
	public class DataProviderFactory : IDataProviderFactory
	{
		readonly ILogger _logger;
		readonly DatabaseReference _dbRootNode;
		readonly Dictionary<string, object> _providers = new Dictionary<string, object>();

		public DataProviderFactory (ILogger logger)
		{
			_logger = logger;
			_dbRootNode = Firebase.Database.Database.DefaultInstance.GetRootReference ();
		}

		public IDataProvider<T> GetProvider<T> (string path) where T : Identifiable, new()
		{
			if (!_providers.ContainsKey(path))
				_providers.Add(path, new DataProvider<T>(_logger, _dbRootNode, path));			

			return (DataProvider<T>)_providers[path];
		}
	}
	
}
