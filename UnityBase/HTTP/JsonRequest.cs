#nullable enable

using System;
using System.Collections.Generic;
using System.Text;
using BatteryAcid.Serializables;
using NaughtyAttributes;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Networking;

namespace UnityBase.HTTP
{
	[Serializable]
	public class BaseJsonRequest<T, TData> : UnityWebRequest where T : BaseJsonRequest<T, TData>, new()
	{
		private T Self
		{
			get
			{
				T self = (this as T)!;
				Assert.IsNotNull(self, "This should never happen");
				return self;
			}
		}

		// Hold reference to additional data to avoid garbage collection
		public object? _attached;
		
		private string? _error;

		protected UnityWebRequestAsyncOperation? _operation;

		protected TData? _responseJson;

		[ShowNativeProperty]
		public TData ResponseJson
		{
			get
			{
				switch (result) {
					case Result.InProgress:
						throw new InvalidOperationException(
							"Cannot deserialize HTTP response, because the request is still in progress");
					default:
						throw new InvalidOperationException(
							$"Cannot deserialize HTTP response, because the request failed:\n{base.error}");
					case Result.Success:
						return _responseJson!;
				}
			}
		}

		[ShowNativeProperty]
		public new string? error => base.error ?? _error;

		public static T Create(
			string           url,
			string           method,
			DownloadHandler? downloadHandler,
			UploadHandler?   uploadHandler,
			object? attached =null
		)
		{
			T request = new();
			request._attached       = attached;
			request.url             = url;
			request.method          = method;
			request.downloadHandler = downloadHandler;
			request.uploadHandler   = uploadHandler;
			if (uploadHandler is not null) uploadHandler.contentType = "application/json";
			return request;
		}
		
		public static T Create(UnityWebRequest from, object? attached=null)
		{
			T request = new();
			request._attached       = attached;
			request.url             = from.url;
			request.method          = from.method;
			request.downloadHandler = from.downloadHandler;
			request.uploadHandler   = from.uploadHandler;
			request.SetRequestHeader("Authorization", from.GetRequestHeader("Authorization"));
			return request;
		}

		public override string ToString()
		{
			var str = $"{method} {url}";

			if (downloadHandler is not null && downloadHandler.isDone)
				str += $" RES={GetResponseHeader("Content-Type")}:\n" +
				       $"\t{downloadHandler.text}\n"                  +
				       $"\tHeaders={JsonConvert.SerializeObject(GetResponseHeaders())}\n";

			if (uploadHandler is not null)
				str += $" REQ={uploadHandler.contentType}:\n"               +
				       $"\t{Encoding.UTF8.GetString(uploadHandler.data)}\n" +
				       $"\t{BitConverter.ToString(uploadHandler.data)}";

			return str;
		}

		public event Action<T>?         onResponse;
		public event Action<T>?         onResponseOK;
		public event Action<T, string>? onResponseERR;

		public new static T Get(string url) =>
			Create(url, "GET", new DownloadHandlerBuffer(), null);

		public static T Post(string url, byte[] bodyData) =>
			Create(url, "POST", new DownloadHandlerBuffer(), new UploadHandlerRaw(bodyData));

		public static T Post(string url, object json)
		{
			var bodyString = JsonConvert.SerializeObject(json);
			var bodyData   = Encoding.UTF8.GetBytes(bodyString);
			return Post(url, bodyData);
		}

		public new T Send()
		{
			Debug.Log($"Request {this}");

			if (_operation is not null)
				throw new InvalidOperationException("Cannot send HTTP request: this request has already been sent!");
			_operation           =  SendWebRequest();
			_operation.completed += OnCompleted;
			return (T)this;
		}
		
		public T Send(Action<T> onResponseOK)
		{
			this.onResponseOK += onResponseOK;
			Send();
			return (T)this;
		}
		
		public T Send(Action<T> onResponseOK, Action<T, string> onResponseERR)
		{
			this.onResponseERR += onResponseERR;
			Send(onResponseOK);
			return (T)this;
		}
		
		public T Send(Action<T> onResponseOK, Action<T, string> onResponseERR, Action<T> onResponse)
		{
			this.onResponse += onResponse;
			Send(onResponseOK, onResponseERR);
			return (T)this;
		}
		
		protected void OnResponseOK()
		{
			onResponseOK?.Invoke(Self);
			onResponse?.Invoke(Self);
		}

		protected void OnResponseERR(string err)
		{
			Debug.LogError(err + "\n" + downloadHandler?.error ?? "");
			onResponseERR?.Invoke(Self, err);
			onResponse?.Invoke(Self);
		}

		protected virtual void OnCompleted(AsyncOperation _)
		{
			if (!ParseResponseJson()) return;

			OnResponseOK();
		}

		protected bool ParseResponseJson()
		{
			Debug.Log($"Response {this}");

			_error = base.error;
			if (!string.IsNullOrWhiteSpace(_error)) {
				// HTTP request failed
				OnResponseERR(_error);
				return false;
			}
			
			if (typeof(TData) == typeof(bool)) {
				_responseJson = (TData)(object)(error == null);
			} else if (typeof(TData) == typeof(string)) {
				_responseJson = (TData)(object)downloadHandler.text;
			} else if (typeof(TData) == typeof(byte[])) {
				_responseJson = (TData)(object)downloadHandler.data;
			} else {
				_responseJson = JsonConvert.DeserializeObject<TData>(downloadHandler.text);
			}

			if (_responseJson is null) {
				_error = $"Failed to deserialize HTTP response as JSON object {typeof(TData)}:\n" +
				         $"{downloadHandler.text}";
				OnResponseERR(_error);
				return false;
			}

			return true;
		}
	}
	
	public class JsonRequest<TData> : BaseJsonRequest<JsonRequest<TData>, TData>
	{
	}

	/*
	 * Support paging with Spring HATEOAS
	 */
	[Serializable]
	public class PagedJsonRequest<TData> : BaseJsonRequest<PagedJsonRequest<TData>, Page<TData>>
	{
		[SerializeField] private List<TData>? _unpagedResponse;

		public List<TData>? UnpagedResponse => _unpagedResponse;
		protected override void OnCompleted(AsyncOperation _)
		{
			if (!ParseResponseJson()) return;

			if (_unpagedResponse == null) _unpagedResponse = new List<TData>();

			foreach (var key in _responseJson!._embedded.Keys) {
				Debug.Log($"Unpaging {_responseJson!._embedded[key].Count} items from _embedded[{key}]");
				_unpagedResponse.AddRange(_responseJson!._embedded[key]);
			}

			var nextUrl = _responseJson!.nextUrl;
			if (nextUrl == null) {
				OnResponseOK();
			}
			else {
				// TODO? may need to Create() new UnityWebRequest here and bind to its onResponse*
				url        = nextUrl;
				_operation = null;
				Send();
			}
		}
	}
	
#nullable disable
	public class Page<TData>
	{
		public Dictionary<string, List<TData>> _embedded = new();

		public SerializableDictionary<string, SerializableDictionary<string, string>>? _links = new();

		public string? nextUrl => _links?.ContainsKey("next") ?? false
			? _links["next"]["href"]
			: null;
	}
}
