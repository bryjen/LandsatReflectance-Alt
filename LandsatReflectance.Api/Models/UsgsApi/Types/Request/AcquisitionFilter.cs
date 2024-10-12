namespace LandsatReflectance.Api.Models.UsgsApi.Types.Request;

public class AcquisitionFilter
{
    public required DateTime Start { get; set; }
    public required DateTime End { get; set; }
}