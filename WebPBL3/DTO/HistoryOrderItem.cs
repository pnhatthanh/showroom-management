namespace WebPBL3.DTO
{
    public class HistoryOrderItem
    {   
        
        public string CarID { get; set; } = string.Empty;
        public string Photo { get; set; } = string.Empty ;
        public string CarName { get; set; } = string.Empty;
        public string MakeName { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public double Price { get; set; }
        public int Quantity { get; set; }
        
    }
}
