namespace Firefly.Models
{
    public interface IGeoCodable
    {
        double Lat { get; set; }
        double Lng { get; set; }
    }
}