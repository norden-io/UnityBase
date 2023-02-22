#nullable enable

using System;
using System.Text;
using BatteryAcid.Serializables;
using NaughtyAttributes;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace UnityBase.HTTP
{
	[Serializable]
	public class JsonRequest<T> : UnityWebRequest
	{
		private string? _error;

		private UnityWebRequestAsyncOperation? _operation;

		private T? _response;

		public JsonRequest(
			string           url,
			string           method,
			DownloadHandler? downloadHandler,
			UploadHandler?   uploadHandler
		) : base(url, method, downloadHandler, uploadHandler)
		{
			if (uploadHandler is not null)
				uploadHandler.contentType = "application/json";
		}

		[ShowNativeProperty]
		public T? Response
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
						return _response;
				}
			}
		}

		[ShowNativeProperty]
		public new string? error => base.error ?? _error;

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

		public event Action<JsonRequest<T>>?         onResponse;
		public event Action<JsonRequest<T>, T>?      onResponseOK;
		public event Action<JsonRequest<T>, string>? onResponseERR;

		public new static JsonRequest<T> Get(string url) =>
			new(url, "GET", new DownloadHandlerBuffer(), null);

		public static JsonRequest<T> Post(string url, byte[] bodyData) =>
			new(url, "POST", new DownloadHandlerBuffer(), new UploadHandlerRaw(bodyData));

		public static JsonRequest<T> Post(string url, object json)
		{
			var bodyString = JsonConvert.SerializeObject(json);
			var bodyData   = Encoding.UTF8.GetBytes(bodyString);
			return Post(url, bodyData);
		}

		public new void Send()
		{
			Debug.Log($"Request {this}");
			
			if (_operation is not null)
				throw new InvalidOperationException("Cannot send HTTP request: this request has already been sent!");
			_operation           =  SendWebRequest();
			_operation.completed += OnCompleted;
		}

		private void OnCompleted(AsyncOperation _)
		{
			Debug.Log($"Response {this}");
			
			_error = base.error;
			if (!string.IsNullOrWhiteSpace(_error)) {
				// HTTP request failed
				Debug.LogError(_error);
				onResponse?.Invoke(this);
				onResponseERR?.Invoke(this, _error);
				return;
			}

			_response = JsonConvert.DeserializeObject<T>(downloadHandler.text);
			if (_response is null) {
				_error = $"Failed to deserialize HTTP response as JSON object {typeof(T)}:\n" +
				         $"{downloadHandler.text}";
				Debug.LogError(_error);
				onResponse?.Invoke(this);
				onResponseERR?.Invoke(this, _error);
				return;
			}

			onResponse?.Invoke(this);
			onResponseOK?.Invoke(this, _response);
		}
	}

	public class JsonRequest : JsonRequest<SerializableDictionary<string, string>>
	{
		public JsonRequest(string url, string method, DownloadHandler? downloadHandler, UploadHandler? uploadHandler) :
			base(url, method, downloadHandler, uploadHandler)
		{
		}
	}
}
