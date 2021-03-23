namespace BeachHouseAPI.DTOs
{
    internal class EmailDTO
    {
        public string Body { get; set; }
        public string Subject { get; set; }
        public bool IsBodyHtml { get; set; }
        public string From { get; set; }
        public string To { get; set; }
    }
}