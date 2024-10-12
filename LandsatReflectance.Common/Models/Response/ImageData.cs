namespace LandsatReflectance.Common.Models.ResponseModels;

public class ImageData
{
    public required string WholeImageUri { get; set; } = string.Empty;
    public required string PixelImageUriTemplate { get; set; } = string.Empty;
    
    public required DateTime AcquisitionDate { get; set; } = DateTime.MinValue;
    public required DateTime PublishDate { get; set; } = DateTime.MinValue;
    public required int CloudCover { get; set; }
}
