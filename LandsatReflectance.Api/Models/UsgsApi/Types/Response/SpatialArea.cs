﻿namespace LandsatReflectance.Api.Models.UsgsApi.Types;

public class SpatialArea
{
    public string Type { get; set; } = string.Empty;
    public object[] Coordinates { get; set; } = [];
}