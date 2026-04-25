namespace MsLogistic.IntegrationTest.Helpers;

public record ApiResponse {
	public bool IsSuccess { get; init; }
	public ApiError? Error { get; init; }
}

public record ApiResponse<T> : ApiResponse {
	public T? Value { get; init; }
}

public record ApiError {
	public string Code { get; init; } = string.Empty;
	public string Message { get; init; } = string.Empty;
	public int Type { get; init; }
}
