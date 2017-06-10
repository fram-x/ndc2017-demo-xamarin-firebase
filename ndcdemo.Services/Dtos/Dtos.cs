using System;

namespace NdcDemo.Services.Dtos
{
	public class Message : Identifiable
	{
		public string Name { get; set; }
		public string Text { get; set; }
		public DateTime Date { get; set; }
	}
}
