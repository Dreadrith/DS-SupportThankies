using System;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace DreadScripts.Common.SupportThankies
{
	internal readonly struct AsyncUnityWebRequest : IDisposable
	{
		internal readonly UnityWebRequest request;
		internal bool failed => request.isNetworkError || request.isHttpError;
		private readonly int refreshFrequency;
		private readonly Action onProcessed;

		internal AsyncUnityWebRequest(string url, string method = null, int refreshFrequency = 100) : this(url, null, method, refreshFrequency)
		{
		}

		internal AsyncUnityWebRequest(string url, Action onProcessed, string method = null, int refreshFrequency = 100)
		{
			if (string.IsNullOrWhiteSpace(method)) method = UnityWebRequest.kHttpVerbGET;
			request = new UnityWebRequest(url, method);
			this.onProcessed = onProcessed;
			this.refreshFrequency = refreshFrequency;
		}

		public void Dispose()
		{
			request.Dispose();
		}

		internal async Task Process()
		{
			var op = request.SendWebRequest();
			while (!op.isDone) await Task.Delay(refreshFrequency);
			onProcessed?.Invoke();
		}
	}
}
