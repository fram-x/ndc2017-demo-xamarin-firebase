using System;
using NdcDemo.Services.Providers;

namespace NdcDemo.Services
{
	public static class ServiceContainer
	{
		public static ILogger Logger { get; set; }
		public static IDataProviderFactory DataProviderFactory { get; set; }
		public static IDataService DataService { get; set; }
	}
}
