using System.Net;
using System.Text;

namespace MsLogistic.UnitTest.Common.Http;

internal class FakeHttpMessageHandler : HttpMessageHandler {
	private HttpStatusCode _statusCode = HttpStatusCode.OK;
	private string _responseContent = "{}";
	private Exception? _exceptionToThrow;

	public int RequestCount { get; private set; }
	public HttpRequestMessage? LastRequest { get; private set; }

	public void RespondWith(HttpStatusCode statusCode, string content) {
		_statusCode = statusCode;
		_responseContent = content;
		_exceptionToThrow = null;
	}

	public void ThrowOnRequest(Exception exception) {
		_exceptionToThrow = exception;
	}

	protected override Task<HttpResponseMessage> SendAsync(
		HttpRequestMessage request,
		CancellationToken cancellationToken
	) {
		RequestCount++;
		LastRequest = request;

		if (_exceptionToThrow != null) {
			throw _exceptionToThrow;
		}

		var response = new HttpResponseMessage(_statusCode) {
			Content = new StringContent(_responseContent, Encoding.UTF8, "application/json")
		};
		return Task.FromResult(response);
	}
}
