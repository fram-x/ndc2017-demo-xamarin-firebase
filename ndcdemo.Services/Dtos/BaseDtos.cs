namespace NdcDemo.Services.Dtos
{
	public class Identifiable
	{
		// TODO: Add JsonIgnore as this will always be in path, set Id in providers after deserializing
		public string Id { get; set; }
	}

}
