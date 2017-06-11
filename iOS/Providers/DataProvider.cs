using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Database;
using NdcDemo.Services;
using NdcDemo.Services.Dtos;
using NdcDemo.Services.Providers;
using Foundation;
//using Services;

namespace Firebase.iOS.Providers
{
	public class DataProvider<T> : IDataProvider<T> where T : Identifiable, new()
	{
		#region Private Members

		readonly static ObservationType[] ObservationTypes = new ObservationType[] { ObservationType.ChildAdded, ObservationType.ChildChanged, ObservationType.ChildRemoved };		

		readonly ILogger _logger;
		readonly DatabaseReference _dbGroupNode;
		readonly Dictionary<ObservationType, nuint> _observerHandles;

		#endregion

		public DataProvider(ILogger logger, DatabaseReference dbRootNode, string path)
		{
			_logger = logger;
			_dbGroupNode = dbRootNode.GetChild(path);
			_observerHandles = new Dictionary<ObservationType, nuint>();
		}

		#region IDataProvider Interface Members

		public string Create(T obj)
		{
			DatabaseReference objNode;
			if (string.IsNullOrEmpty(obj.Id))
			{
				objNode = _dbGroupNode.GetChildByAutoId();
				obj.Id = objNode.Key;
			}
			else
			{
				objNode = _dbGroupNode.GetChild(obj.Id);
			}

			var updates = Encoder.EncodeObject(obj);

			objNode.UpdateChildValues(updates);

			return obj.Id;
		}

		public Task<T> ReadAsync(string id)
		{
			var t = new TaskCompletionSource<T>();

			_dbGroupNode.GetChild(id).ObserveSingleEvent(DataEventType.Value, (snapshot) =>
			{
				var nsObject = snapshot.ValueInExportFormat;
				var nsDictionary = nsObject as NSDictionary;

				if (nsDictionary == null)
				{
					t.TrySetResult(null);
				}
				else
				{
					var obj = Encoder.DecodeObject<T>(nsDictionary);
					t.TrySetResult(obj);
				}
			});

			return t.Task;
		}

		public Task<IEnumerable<T>> ReadAllAsync()
		{
			var t = new TaskCompletionSource<IEnumerable<T>>();

			_dbGroupNode.ObserveSingleEvent(DataEventType.Value, (snapshot) =>
			{
				var nsObject = snapshot.ValueInExportFormat;
				var nsDictionary = nsObject as NSDictionary;

				if (nsDictionary == null)
				{
					t.TrySetResult(new List<T>());
				}
				else
				{
					var obj = Encoder.DecodeObject<Dictionary<string, T>>(nsDictionary);
					t.TrySetResult(obj.Values.ToList());
				}
			});

			return t.Task;
		}

		public void Delete(string id)
		{
			var objNode = _dbGroupNode.GetChild(id);
			objNode.RemoveValue();
		}

		#endregion

		#region IObserverHandle Interface Implementation

		public void Observe(Action<ObservationType, T> handler)
		{
			CancelObservation();

			foreach (var observationType in ObservationTypes) {
				_observerHandles[observationType] = Observe(observationType, handler);
			}			         
		}

		public void ObserveAfterId(string afterId, Action<ObservationType, T> handler)
		{
			CancelObservation();

			foreach (var observationType in ObservationTypes) {
				_observerHandles[observationType] = ObserveAfterId(observationType, afterId, handler);
			}			         
		}

		public void CancelObservation()
		{
			foreach (var observationType in ObservationTypes) {
				if (_observerHandles.ContainsKey(observationType)) {
					var observerHandler = _observerHandles[observationType];
					if (observerHandler != 0) {
						_dbGroupNode.RemoveObserver(observerHandler);
						_observerHandles[observationType] = 0;
					}
				}
			}			         
		}

		#endregion

		#region Private Members

		nuint Observe(ObservationType observationType, Action<ObservationType, T> handler)
		{
			var observerHandle = _dbGroupNode.ObserveEvent(
				GetDataEventTypeFromObservationType(observationType),
				(snapshot, prevKey) =>
				HandleObserveChildEvent(snapshot, prevKey, observationType, handler));

			return observerHandle;
		}

		nuint ObserveAfterId(ObservationType observationType, string afterId, Action<ObservationType, T> handler)
		{
			var query = _dbGroupNode.GetQueryOrderedByKey();

			// If afterId provided, start observing from this id
			if (!string.IsNullOrEmpty(afterId))
				query = query.GetQueryStartingAtValue(new NSString(afterId));			

			var observerHandle = query.ObserveEvent(
				GetDataEventTypeFromObservationType(observationType), (snapshot, prevKey) => 
				HandleObserveChildEvent(snapshot, prevKey, observationType, handler, afterId));

			return observerHandle;
		}

		void HandleObserveChildEvent(DataSnapshot snapshot, string prevKey, ObservationType observationType, Action<ObservationType, T> handleChildEvent, params string[] excludeIds)
		{
			_logger.Debug($"Event: {observationType}, prevKey: {prevKey}");

			var nsDictionary = snapshot.ValueInExportFormat as NSDictionary;
			var obj = Encoder.DecodeObject<T>(nsDictionary);

			if (!excludeIds.Contains(obj.Id))
			{
				handleChildEvent(observationType, obj);
			}
		}

		DataEventType GetDataEventTypeFromObservationType(ObservationType observationType)
		{
			switch(observationType)
			{
				case ObservationType.ChildAdded:
					return DataEventType.ChildAdded;

				case ObservationType.ChildChanged:
					return DataEventType.ChildChanged;

				case ObservationType.ChildRemoved:
					return DataEventType.ChildRemoved;

				default:
					throw new NotImplementedException();
			}
		}

		#endregion
	}
}
