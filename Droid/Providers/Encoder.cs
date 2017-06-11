using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Database;
using NdcDemo.Services.Dtos;
using NdcDemo.Services.Providers;
using Java.Util;
//using NControl.Mvvm;
using Newtonsoft.Json.Linq;
using NdcDemo.Services;

namespace NdcDemo.Droid
{

	public static class Encoder
	{
		public static IDictionary<string, Java.Lang.Object> EncodeObject (object obj)
		{
			if (obj == null) return null;

			var jsonObject = JObject.FromObject (obj);

			if (jsonObject == null) return null;

			var encodedObject = new Dictionary<string, Java.Lang.Object> ();

			foreach (var keyValue in jsonObject) {
				EncodeProperty (encodedObject, keyValue.Key, keyValue.Value);
			}

			return encodedObject;
		}

		static void EncodeProperty (Dictionary<string, Java.Lang.Object> encodedProperties, string propertyName, JToken propertyValue)
		{
			Java.Lang.Object javaValue = EncodeValue (propertyValue);
			if (javaValue != null) {
				encodedProperties.Add(propertyName, javaValue);
			}
		}

		static HashMap EncodeObjectAsJavaMap (object obj)
		{
			if (obj == null) return null;

			var jsonObject = JObject.FromObject (obj);

			if (jsonObject == null) return null;

			var encodedObject = new HashMap ();

			foreach (var keyValue in jsonObject) {
				EncodeProperty (encodedObject, keyValue.Key, keyValue.Value);
			}

			return encodedObject;
		}

		static void EncodeProperty (HashMap encodedProperties, string propertyName, JToken propertyValue)
		{
			Java.Lang.Object javaValue = EncodeValue (propertyValue);
			if (javaValue != null) {
				encodedProperties.Put(propertyName, javaValue);
			}
		}

		static Java.Lang.Object EncodeValue (JToken value)
		{
			switch (value.Type) {
				case JTokenType.Null:
				case JTokenType.Undefined: return null;

				case JTokenType.Object: return EncodeObjectAsJavaMap (value);

				case JTokenType.Array: return EncodeArray (value.ToArray ());

				case JTokenType.Integer: return value.ToObject<long> ();
				case JTokenType.Float: return value.ToObject<double> ();

				case JTokenType.Date: return value.ToObject<DateTime> ().ToString ("O"); // ISO 8601

				case JTokenType.String:
				case JTokenType.Boolean:
				case JTokenType.Guid:
				case JTokenType.Uri: return value.ToString ();

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

		static ArrayList EncodeArray (JToken [] array)
		{
			var javaArray = new ArrayList(array.Length);
			foreach (var value in array) {
				javaArray.Add (EncodeValue (value));
			}

			return javaArray;
		}

		public static Dictionary<string, T> DecodeList<T>(Java.Lang.Object javaObject) where T: class, new()
		{
			var javaDict = javaObject as System.Collections.IDictionary;
			if (javaDict == null) return null;

			var netDict = new Dictionary<string, T>();

			var keyEnum = javaDict.Keys.GetEnumerator();
			while (keyEnum.MoveNext()) {
				var key = keyEnum.Current;
				var javaObj = javaDict[key]; 
				var netObject = Decode<T>(javaObj);
				netDict.Add(key.ToString(), netObject);
			}

			return netDict;
		}

		public static T Decode<T>(Java.Lang.Object javaObject) where T: class, new()
		{
			return Decode<T>((object)javaObject);
		}

		public static T Decode<T>(object javaObject) where T: class, new()
		{
			var javaDict = javaObject as System.Collections.IDictionary;
			if (javaDict != null) return Decode<T>(javaDict);

			return null;
		}

		public static T Decode<T>(System.Collections.IDictionary javaDict) where T: class, new()
		{
			var netObject = new T();

			var keyEnum = javaDict.Keys.GetEnumerator();
			while (keyEnum.MoveNext()) {
				var key = keyEnum.Current;
				var javaValue = javaDict[key]; 
				DecodePropery(javaValue, key.ToString(), netObject);
			}

			return netObject;
		}

		public static object Decode(Type objectType, object javaObject)
		{
			var javaDict = javaObject as System.Collections.IDictionary;
			if (javaDict != null) return Decode(objectType, javaDict);

			return null;
		}

		public static object Decode(Type objectType, System.Collections.IDictionary javaDict)
		{
			var netObject = Activator.CreateInstance(objectType);

			var keyEnum = javaDict.Keys.GetEnumerator();
			while (keyEnum.MoveNext()) {
				var key = keyEnum.Current;
				var javaValue = javaDict[key]; 
				DecodePropery(javaValue, key.ToString(), netObject);
			}

			return netObject;
		}

		/// <summary>
		/// Decodes the id references, if none, returns null
		/// </summary>
		/// <returns>The references.</returns>
		/// <param name="javaObject">Java object.</param>
		static Dictionary<string,bool> DecodeReferences(object javaObject)
		{
			var javaDict = javaObject as System.Collections.IDictionary;
			if (javaDict == null) return null;

			Dictionary<string,bool> references = null;

			var keyEnum = javaDict.Keys.GetEnumerator();
			while (keyEnum.MoveNext()) {
				var key = keyEnum.Current;
				if (references == null) {
					references = new Dictionary<string, bool>();
				}

				references.Add(key.ToString(), true);
			}

			return references;
		}

		static void DecodePropery(object javaValue, string key, object netObject)
		{
			if (javaValue == null) return;

			var prop = netObject.GetType().GetProperty(key);
			if (prop != null) {
				var propType = prop.PropertyType;

				if (propType == typeof(string)) {
					prop.SetValue(netObject, javaValue.ToString());
					return;
				}

				if (propType == typeof(Int32)) {
					Int32 netValue;
					if (Int32.TryParse(javaValue.ToString(), out netValue)) {
						prop.SetValue(netObject, netValue);
						return;
					}
				}

				if (propType == typeof(Int64)) {
					Int64 netValue;
					if (Int64.TryParse(javaValue.ToString(), out netValue)) {
						prop.SetValue(netObject, netValue);
						return;
					}
				}

				if (propType == typeof(bool)) {
					bool netValue;
					if (bool.TryParse(javaValue.ToString(), out netValue)) {
						prop.SetValue(netObject, netValue);
						return;
					}
				}

				if (propType == typeof(DateTime)) {
					DateTime netValue;
					if (DateTime.TryParse(javaValue.ToString(), out netValue)) {
						prop.SetValue(netObject, netValue);
						return;
					}
				}

				if (propType == typeof(float)) {
					float netValue;
					if (float.TryParse(javaValue.ToString(), out netValue)) {
						prop.SetValue(netObject, netValue);
						return;
					}
				}

				if (propType == typeof(double)) {
					double netValue;
					if (double.TryParse(javaValue.ToString(), out netValue)) {
						prop.SetValue(netObject, netValue);
						return;
					}
				}

				if (propType.IsEnum) {
					var netValue = System.Enum.Parse(propType, javaValue.ToString());
					prop.SetValue(netObject, netValue);
					return;
				}

				if (propType == typeof(Dictionary<string, bool>)) {
					var netValue = DecodeReferences(javaValue);
					prop.SetValue(netObject, netValue);
					return;
				}

				if (propType == typeof(Uri)) {
					var netValue = new Uri(javaValue.ToString());
					prop.SetValue(netObject, netValue);
					return;
				}

				if (propType.IsGenericType 
				    && propType.GenericTypeArguments.Length == 2 
				    && propType.GenericTypeArguments[0] == typeof(Int32)
				    && propType.GenericTypeArguments[1].IsClass)
				{
					var netValue = DecodeObjectArrayAsDictionary(propType, javaValue);
					prop.SetValue(netObject, netValue);
					return;
				}

				var logger = ServiceContainer.Logger;
				logger.Debug($"*** Bug: Unsupported type: {propType.Name} for property {prop.Name}");
			}
		}

		static object DecodeObjectArrayAsDictionary(Type dictType, object javaValue)
		{
			if (dictType.GenericTypeArguments.Length != 2) return null;

			var valueType = dictType.GenericTypeArguments[1];

			var untypedArray = new List<object>();
			var javaArray = (javaValue as System.Collections.IList);
			foreach (var javaArrayItem in javaArray)
			{
				var netArrayItem = Decode(valueType, javaArrayItem);
				if (netArrayItem != null) {
					untypedArray.Add(netArrayItem);
				}
			}

			if (untypedArray.Count == 0) return null;

			var netDict = Activator.CreateInstance(dictType) as System.Collections.IDictionary;
			for (int i=0; i<untypedArray.Count; i++) {
				netDict.Add(i, untypedArray[i]);
			}


			return netDict;
		}
	}
}
