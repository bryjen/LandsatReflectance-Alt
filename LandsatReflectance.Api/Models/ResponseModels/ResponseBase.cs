namespace LandsatReflectance.Backend.Models.ResponseModels;

public class ResponseBase<T> where T : class
{
    public string? ErrorMessage { get; set; }
    public T? Data { get; set; } = default;
}