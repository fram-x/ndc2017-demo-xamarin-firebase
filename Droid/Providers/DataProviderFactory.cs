using System.Collections.Generic;
using Firebase.Database;
using NdcDemo.Services.Dtos;
using NdcDemo.Services.Providers;

namespace NdcDemo.Droid
{
	public class DataProviderFactory : IDataProviderFactory
	{
		readonly Dictionary<string, object> _providers = new Dictionary<string, object>();

		public IDataProvider<T> GetProvider<T>(string path) where T : Identifiable, new()
		{
			var db = FirebaseDatabase.Instance;
			var reference = db.GetReference(path);
			return new DataProvider<T>(reference);			

			//if (!_providers.ContainsKey(path))
			//{
			//	var db = FirebaseDatabase.Instance;
			//	var reference = db.GetReference(path);
			//	_providers.Add(path, new DataProvider<T>(reference));			
			//}

			//return (DataProvider<T>)_providers[path];
		}
	}
}
