using System;
using System.Linq;
using Foundation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Firebase.iOS.Providers
{
	public static class Encoder
	{
		public static T DecodeObject<T> (NSDictionary nsDictionary) where T : class
		{
			if (nsDictionary == null) {
				return null;
			}

			NSError error;
			NSData data = NSJsonSerialization.Serialize (nsDictionary, 0, out error);
			if (data != null) {
				var jsonString = NSString.FromData (data, NSStringEncoding.UTF8);
				var obj = JsonConvert.DeserializeObject<T> (jsonString);
				return obj;
			}

			// TODO: Handle errors

			return null;
		}

		public static NSDictionary EncodeObject (object obj)
		{
			if (obj == null) return null;

			var jObject = JObject.FromObject (obj);

			if (jObject == null) return null;

			var encodedObject = new NSMutableDictionary ();

			foreach (var keyValue in jObject) {
				EncodeProperty (encodedObject, keyValue.Key, keyValue.Value);
			}

			return encodedObject;
		}

		static void EncodeProperty (NSMutableDictionary encodedProperties, string propertyName, JToken propertyValue)
		{
			NSObject nsValue = EncodeValue (propertyValue);
			if (nsValue != null) {
				encodedProperties.Add (new NSString (propertyName), nsValue);
			}
		}

		static NSObject EncodeValue (JToken value)
		{
			switch (value.Type) {
			case JTokenType.Null:
			case JTokenType.Undefined: return null;

			case JTokenType.Object: return EncodeObject (value);

			case JTokenType.Array: return EncodeArray (value.ToArray ());

			case JTokenType.Integer: return NSNumber.FromLong ((nint)value.ToObject<long> ());
			case JTokenType.Float: return NSNumber.FromDouble (value.ToObject<double> ());

			case JTokenType.Date: return new NSString (value.ToObject<DateTime> ().ToString ("O")); // ISO 8601

			case JTokenType.String:
			case JTokenType.Boolean:
			case JTokenType.Guid:
			case JTokenType.Uri: return new NSString (value.ToString ());

			case JTokenType.TimeSpan:
			case JTokenType.Raw:
			case JTokenType.Bytes:
			case JTokenType.Property:
			case JTokenType.Comment:
			case JTokenType.Constructor:
			case JTokenType.None:
			default:
				throw new ArgumentOutOfRangeException ($"Encoding of {value.Type} not supported");
			}
		}

		static NSArray EncodeArray (JToken [] array)
		{
			var nsArray = new NSMutableArray ((nuint)array.Length);
			foreach (var value in array) {
				nsArray.Add (EncodeValue (value));
			}

			return nsArray;
		}
	}
}
