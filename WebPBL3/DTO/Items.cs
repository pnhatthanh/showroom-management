using Newtonsoft.Json;

namespace WebPBL3.DTO
{
    public class Items
    {
        public string carID {  get; set; }
        public string carName { get; set; }
        public double price {  get; set; }
        public int quantity {  get; set; }
        public string? color {  get; set; }
    }
}
