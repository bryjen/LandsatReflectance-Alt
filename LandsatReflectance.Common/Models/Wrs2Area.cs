using ProtoBuf;

namespace LandsatReflectance.Models;

[ProtoContract]
public struct LatLong
{
    [ProtoMember(1)]
    public float Latitude { get; set; }
    
    [ProtoMember(2)]
    public float Longitude { get; set; }

    public LatLong(float latitude, float longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
    }
}

[ProtoContract]
public struct Wrs2Area
{
    [ProtoContract]
    public struct AreaMetadata
    {
        [ProtoMember(1)]
        public byte Path { get; set; }
        
        [ProtoMember(2)]
        public byte Row { get; set; }

        public AreaMetadata()
        { }

        public AreaMetadata(byte path, byte row)
        {
            Path = path;
            Row = row;
        }
    }
    
    [ProtoMember(1)]
    public List<LatLong> LatLongCoordinates { get; set; } = [];
    
    [ProtoMember(2)]
    public AreaMetadata Metadata { get; set; } = new();
    
    public Wrs2Area()
    { }

    public Wrs2Area(List<LatLong> latLongCoordinates, AreaMetadata areaMetadata)
    {
        LatLongCoordinates = latLongCoordinates;
        Metadata = areaMetadata;
    }
    
    public Wrs2Area(List<LatLong> latLongCoordinates, byte path, byte row)
    {
        LatLongCoordinates = latLongCoordinates;
        Metadata = new AreaMetadata(path, row);
    }
}