namespace BrodClientAPI.Models
{
    public class HireTradie
    {
        public string ClientID { get; set; }
        public string ClientContactNumber { get; set; }
        public string ClientPostCode { get; set; }
        public string ServiceID { get; set; }
        public string Status { get; set; }
        public string DescriptionServiceNeeded { get; set; }
        public string StartDate { get; set; }
        public string CompletionDate { get; set; }
        public string ClientBudget { get; set; }
        public string BudgetCurrency { get; set; }
    }
}
