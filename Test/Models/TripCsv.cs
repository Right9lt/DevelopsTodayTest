namespace Test.Models
{
    public class TripCsv
    {
        public int? VendorID { get; set; }
        public DateTime tpep_pickup_datetime { get; set; }
        public DateTime tpep_dropoff_datetime { get; set; }
        public int? passenger_count { get; set; }
        public float trip_distance { get; set; }
        public int? RatecodeID { get; set; }
        public string store_and_fwd_flag { get; set; } = string.Empty;
        public int PULocationID { get; set; }
        public int DOLocationID { get; set; }
        public int? payment_type { get; set; }
        public float fare_amount { get; set; }
        public float extra {  get; set; }
        public float mta_tax { get; set; }
        public float tip_amount { get; set; }
        public float tolls_amount { get; set; }
        public float improvement_surcharge { get; set; }
        public float total_amount { get; set; }
        public float congestion_surcharge { get; set; }
    }
}
