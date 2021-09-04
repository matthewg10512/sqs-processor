namespace sqs_processor.Services.Network.Dividends
{
    public class DividendFromApi
    {

        public string symbol { get; set; }
        public Historical[] historical { get; set; }


    }
    public class Historical
    {
        public string date { get; set; }
        public string label { get; set; }
        public float? adjDividend { get; set; }
        public float? dividend { get; set; }
        public string recordDate { get; set; }
        public string paymentDate { get; set; }
        public string declarationDate { get; set; }
        public string symbol { get; set; }
    }

}
