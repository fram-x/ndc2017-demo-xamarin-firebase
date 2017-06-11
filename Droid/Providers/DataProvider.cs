using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Database;
using NdcDemo.Services.Dtos;
using NdcDemo.Services.Providers;

namespace NdcDemo.Droid
{
	internal enum Order
	{
		Ascending,
		Descending,
		Unordered
	}

	/// <summary>
	/// Generic listener for custom handling of value events
	/// </summary>
	internal class ValueEventListener : Java.Lang.Object, IValueEventListener
	{
		readonly Action<DataSnapshot> _handleValue;

		public ValueEventListener(Action<DataSnapshot> handleValue)
		{
			_handleValue = handleValue;
		}

		public void OnCancelled(DatabaseError error)
		{
			throw new NotImplementedException();
		}

		public void OnDataChange(DataSnapshot snapshot)
		{
			_handleValue(snapshot);
		}
	}

	/// <summary>
	/// Single value "event listene"r to be used when single object or null is expected from a query
	/// </summary>
	internal class SingleValueEventListener<T> : Java.Lang.Object, IValueEventListener where T : Identifiable, new()
	{
		readonly TaskCompletionSource<T> _completionSource;

		public SingleValueEventListener(TaskCompletionSource<T> completionSource)
		{
			_completionSource = completionSource;
		}

		public void OnCancelled(DatabaseError error)
		{
			// TODO: Handle
			throw new NotImplementedException();
		}

		public void OnDataChange(DataSnapshot snapshot)
		{
			var value = Encoder.Decode<T>(snapshot.Value);
			_completionSource.TrySetResult(value);
		}
	}

	/// <summary>
	/// Multi value "event listener" to be used when a set of objects are expected from a query
	/// </summary>
	internal class MultiValueEventListener<T> : Java.Lang.Object, IValueEventListener where T : Identifiable, new()
	{
		#region Private members

		readonly TaskCompletionSource<IEnumerable<T>> _completionSource;
		readonly Order _order;
		readonly string[] _excludeIds;

		#endregion

		/// <summary>
		/// Constructs listener to set result on given completion source.
		/// </summary>
		/// <param name="completionSource">Completion source.</param>
		/// <param name="order">If set to <c>true</c> order of result is reversed</param>
		/// <param name="excludeIds">Exclude any elements with given ids</param>
		public MultiValueEventListener(TaskCompletionSource<IEnumerable<T>> completionSource, Order order = Order.Unordered, params string[] excludeIds)
		{
			_excludeIds = excludeIds;
			_order = order;
			_completionSource = completionSource;
		}

		public void OnCancelled(DatabaseError error)
		{
			// TODO: Handle
			throw new NotImplementedException();
		}

		public void OnDataChange(DataSnapshot snapshot)
		{
			var result = Encoder.DecodeList<T>(snapshot.Value) ??  new Dictionary<string, T>();

			var objs = result.Values
			                .Where(o => !_excludeIds.Contains(o.Id));

			var sorted = 
				  _order == Order.Unordered ? objs.ToList()
			    : _order == Order.Descending ? objs.OrderByDescending(o => o.Id).ToList()
			    : objs.OrderBy(o => o.Id).ToList();

			_completionSource.TrySetResult(sorted);
		}
	}

	/// <summary>
	/// First value "event listener" to be used when a set of objects are expected from a query, but only the first is to be used
	/// </summary>
	internal class FirstValueEventListener<T> : Java.Lang.Object, IValueEventListener where T : Identifiable, new()
	{
		#region Private members

		readonly TaskCompletionSource<T> _completionSource;

		#endregion

		/// <summary>
		/// Constructs listener to set result on given completion source.
		/// </summary>
		/// <param name="completionSource">Completion source.</param>
		public FirstValueEventListener(TaskCompletionSource<T> completionSource)
		{
			_completionSource = completionSource;
		}

		public void OnCancelled(DatabaseError error)
		{
			// TODO: Handle
			throw new NotImplementedException();
		}

		public void OnDataChange(DataSnapshot snapshot)
		{
			var result = Encoder.DecodeList<T>(snapshot.Value);
			var obj = result?.Values.ToList().FirstOrDefault() ?? null;

			_completionSource.TrySetResult(obj);
		}
	}

	/// <summary>
	/// First value "event listener" to be used when a set of objects are expected from a query, but only the first is to be used
	/// </summary>
	internal class AllWithValueEventListener<T> : Java.Lang.Object, IValueEventListener where T : Identifiable, new()
	{
		#region Private members

		readonly string _childValue;
		readonly string _childKey;
		readonly TaskCompletionSource<IEnumerable<T>> _completionSource;

		#endregion

		/// <summary>
		/// Constructs listener to set result on given completion source.
		/// </summary>
		/// <param name="completionSource">Completion source.</param>
		public AllWithValueEventListener(string childKey, string childValue, TaskCompletionSource<IEnumerable<T>> completionSource)
		{
			_childKey = childKey;
			_childValue = childValue;
			_completionSource = completionSource;
		}

		public void OnCancelled(DatabaseError error)
		{
			// TODO: Handle
			throw new NotImplementedException();
		}

		public void OnDataChange(DataSnapshot snapshot)
		{
			var result = Encoder.DecodeList<T>(snapshot.Value);
			var list = result?.Values.ToList();

			var propInfo = typeof(T).GetProperty(_childKey);
			if (propInfo == null) {
				throw new InvalidOperationException($"{typeof(T).Name} doesn't have property {_childKey}");
			}

			var filteredList = list
				.Where(o => propInfo.GetValue(o).ToString().Equals(_childValue))
				.ToList();

			_completionSource.TrySetResult(filteredList);
		}
	}

	/// <summary>
	/// Observes changes to child objects of a node. Handler will be called on add, change, move and remove of child objects
	/// </summary>
	internal class ChildEventListener<T> : Java.Lang.Object, IChildEventListener where T : Identifiable, new()
	{
		#region Private members

		readonly Action<ObservationType,T> _handler;
		readonly string[] _excludeIds;

		#endregion

		public ChildEventListener(Action<ObservationType,T> handler, params string[] excludeIds)
		{
			_handler = handler;
			_excludeIds = excludeIds;
		}

		public void OnCancelled(DatabaseError error)
		{
			// TODO: Handle
			throw new NotImplementedException();
		}

		public void OnChildAdded(DataSnapshot snapshot, string previousChildName)
		{
			var obj = Encoder.Decode<T>(snapshot.Value);

			if (!_excludeIds.Contains(obj.Id))
			{
				_handler(ObservationType.ChildAdded, obj);
			}
		}

		public void OnChildChanged(DataSnapshot snapshot, string previousChildName)
		{
			var obj = Encoder.Decode<T>(snapshot.Value);
			_handler(ObservationType.ChildChanged, obj);
		}

		public void OnChildMoved(DataSnapshot snapshot, string previousChildName)
		{
			// TODO: Handle
			throw new NotImplementedException();
		}

		public void OnChildRemoved(DataSnapshot snapshot)
		{
			var obj = new T();
			obj.Id = snapshot.Key;
			_handler(ObservationType.ChildRemoved, obj);
		}
	}

	public class DataProvider<T> : IDataProvider<T>, IDisposable where T : Identifiable, new()
	{
		#region Private Members

		readonly DatabaseReference _dbGroupNode;
		IChildEventListener _childEventListener;
		volatile object _syncRoot = new object();

		#endregion

		public DataProvider(DatabaseReference dbGroupNode)
		{
			_dbGroupNode = dbGroupNode;
		}

		#region IDataProvider implementation

		public string Create(T obj)
		{
			DatabaseReference objNode;

			if (string.IsNullOrEmpty(obj.Id))
			{
				objNode = _dbGroupNode.Push();
				obj.Id = objNode.Key;
			}
			else
			{
				objNode = _dbGroupNode.Child(obj.Id);
			}

			var javaObject = Encoder.EncodeObject(obj);
			objNode.UpdateChildren(javaObject);

			return obj.Id;

		}

		public Task<T> ReadAsync(string id)
		{
			var t = new TaskCompletionSource<T>();

			_dbGroupNode.Child(id).AddListenerForSingleValueEvent(new SingleValueEventListener<T>(t));

			return t.Task;
		}

		public Task<IEnumerable<T>> ReadAllAsync()
		{
			var t = new TaskCompletionSource<IEnumerable<T>>();

			_dbGroupNode.AddListenerForSingleValueEvent(new MultiValueEventListener<T>(t));

			return t.Task;
		}

		public Task<T> ReadFirstFromChildValueAsync(string childKey, string childValue)
		{
			var t = new TaskCompletionSource<T>();

			// Search from childValue and pick first 
			_dbGroupNode
				.OrderByChild(childKey)
				.EqualTo(childValue)
				.LimitToFirst(1)
				.AddListenerForSingleValueEvent(new FirstValueEventListener<T>(t));

			return t.Task;
		}

		public Task<IEnumerable<T>> ReadAllWithChildValueAsync(string childKey, string childValue)
		{
			var t = new TaskCompletionSource<IEnumerable<T>>();

			// Search from childValue and pick first 
			_dbGroupNode
				.OrderByChild(childKey)
				.EqualTo(childValue)
				.AddListenerForSingleValueEvent(new AllWithValueEventListener<T>(childKey, childValue, t));

			return t.Task;
		}

		public Task<IEnumerable<T>> ReadPageFromNewestAsync(int pageSize, string lastIdOnPrecedingPage)
		{
			var t = new TaskCompletionSource<IEnumerable<T>>();

			var query = _dbGroupNode.OrderByKey();
			var includesPreceding = !string.IsNullOrEmpty(lastIdOnPrecedingPage);

			if (includesPreceding)
			{
				query = query.EndAt(lastIdOnPrecedingPage);
				pageSize++;
			}

			query.LimitToLast(pageSize)
			     .AddListenerForSingleValueEvent(new MultiValueEventListener<T>(t, order: Order.Descending, excludeIds: lastIdOnPrecedingPage));

			return t.Task;
		}

		public Task<bool> ExistsAsync(string id)
		{
			var t = new TaskCompletionSource<bool>();

			_dbGroupNode
				.Child(id)
				.AddValueEventListener(new ValueEventListener((DataSnapshot snapshot) => 
				{
					t.TrySetResult(snapshot.Exists());	
				}));

			return t.Task;
		}

		public void Delete(string id)
		{
			var objNode = _dbGroupNode.Child(id);
			objNode.RemoveValue();
		}

		#endregion

		#region IObserverHandle Interface Implementation

		public void Observe(Action<ObservationType, T> handler)
		{
			CancelObservation();

			_childEventListener = new ChildEventListener<T>(handler);
			_dbGroupNode.AddChildEventListener(_childEventListener);
		}

		public void ObserveAfterId(string afterId, Action<ObservationType, T> handler)
		{
			CancelObservation();

			var query = _dbGroupNode.OrderByKey();

			// If afterId provided, start observing from this id
			if (!string.IsNullOrEmpty(afterId))
				query = query.StartAt(afterId);			

			_childEventListener = new ChildEventListener<T>(handler, afterId);
			query.AddChildEventListener(_childEventListener);
		}

		public void CancelObservation()
		{
			if (_childEventListener != null) {
				_dbGroupNode.RemoveEventListener(_childEventListener);
				_childEventListener = null;
			}
		}

		public void Dispose()
		{
			CancelObservation();
		}

		#endregion
	}
}
