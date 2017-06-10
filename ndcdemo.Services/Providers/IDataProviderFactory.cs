using NdcDemo.Services.Dtos;

namespace NdcDemo.Services.Providers
{
	public interface IDataProviderFactory
	{
		IDataProvider<T> GetProvider<T>(string path) where T: Identifiable, new(); 
	}
}
