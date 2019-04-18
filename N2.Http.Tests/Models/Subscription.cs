using System;

namespace XUnitHttpClientTests.Models
{
    public class Subscription
    {
        public Guid Id { get; set; }
        public string RetailerCode { get; set; }
        public string Retailer { get; set; }
        public string ProductType { get; set; }
        public string Label { get; set; }
        public bool Started { get; set; }
        public bool Completed { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Remark { get; set; }
        public string State { get; set; }
    }
}
