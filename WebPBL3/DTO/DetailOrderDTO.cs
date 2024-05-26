using System.Diagnostics;

namespace WebPBL3.DTO
{
    public class DetailOrderDTO
    {
        public string OrderId {  get; set; }
        public string CustomerName {  get; set; }
        public string Address { get; set; }
        public string EmailCustomer {  get; set; }
        public string Phone { get; set; }
        public string StaffId { get; set; }
        public string StaffName {  get; set; }
        public string EmailStaff {  get; set; }
        public DateTime PurchaseDate { get; set; }
        public double ToTalPrice {  get; set; }
        public string Status {  get; set; }
        public List<Items> items { get; set; }=new List<Items> { };
    }
}
