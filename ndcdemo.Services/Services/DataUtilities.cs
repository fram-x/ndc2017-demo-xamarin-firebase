using System;

namespace NdcDemo.Services
{
	public static class DataUtilities
	{
		public static string NewId()
		{
			return Guid.NewGuid().ToString("N");
		}
	}
}
