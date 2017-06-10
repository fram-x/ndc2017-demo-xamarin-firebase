using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NdcDemo.Services.Dtos;

namespace NdcDemo.Services.Providers
{
	public enum ObservationType
	{
		ChildAdded,
		ChildChanged,
		ChildRemoved
	}

	public interface IObservableHandle
	{ }

	public interface IObservable : IObservableHandle
	{
		void CancelObservation();
	}

	public interface IDataProvider<T> : IObservable where T : Identifiable, new()
	{
		string Create(T obj);
		void Delete(string id);

		Task<T> ReadAsync(string id);
		Task<IEnumerable<T>> ReadAllAsync();

		void Observe(Action<ObservationType, T> handler);
		void ObserveAfterId(string afterId, Action<ObservationType, T> handler);
	}
}