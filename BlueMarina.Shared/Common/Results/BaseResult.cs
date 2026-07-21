using System.Net;
using System.Text.Json.Serialization;
using BlueMarina.Shared.Constants;
namespace BlueMarina.Shared.Results;

public class BaseResult
{
    [JsonPropertyName("request_id")]
    [JsonPropertyOrder(1)]
    public string RequestId { get; set; }

    [JsonPropertyName("message")]
    [JsonPropertyOrder(2)]
    public string Message { get; set; }

    [JsonPropertyName("is_success")]
    [JsonPropertyOrder(3)]
    public bool IsSuccess =>
        (int)StatusCode >= 200 &&
        (int)StatusCode < 300;

    [JsonPropertyName("status_code")]
    [JsonPropertyOrder(4)]
    public HttpStatusCode StatusCode { get; set; }

    [JsonPropertyName("error_code")]
    [JsonPropertyOrder(5)]
    public string? ErrorCode { get; set; }

    [JsonPropertyName("timestamp")]
    [JsonPropertyOrder(6)]
    public DateTime Timestamp { get; set; }

    public BaseResult()
    {
        RequestId = Guid.NewGuid().ToString();
        Message = "An error occurred; please try again later";
        StatusCode = HttpStatusCode.InternalServerError;
        Timestamp = DateTime.UtcNow;
    }

    public BaseResult(
        HttpStatusCode statusCode = HttpStatusCode.InternalServerError,
        string message = "An error occurred; please try again later",
        string? requestId = null,
        string? errorCode = null)
    {
        RequestId = requestId ?? Guid.NewGuid().ToString();
        Message = message;
        StatusCode = statusCode;
        ErrorCode = errorCode;
        Timestamp = DateTime.UtcNow;
    }
}

public class BaseResult<T> : BaseResult
{
    [JsonPropertyName("data")]
    [JsonPropertyOrder(7)]
    public T? Data { get; set; }

    public BaseResult()
    {
    }

    public BaseResult(
        HttpStatusCode statusCode = HttpStatusCode.InternalServerError,
        string message = "An error occurred; please try again later",
        T? data = default,
        string? requestId = null,
        string? errorCode = null)
        : base(statusCode, message, requestId, errorCode)
    {
        Data = data;
    }
}