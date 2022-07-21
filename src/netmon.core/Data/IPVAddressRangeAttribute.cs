namespace netmon.core.Data
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class IPVAddressRangeAttribute : Attribute
    {
        public string CIDR { get; set; }
        public string Organisation { get; set; }
        public string Location { get; set; }
        public string Country { get; set; }
        public IPVAddressRangeAttribute(string cidr, string organisation, string location, string country) 
        {
            this.CIDR = cidr;
            this.Organisation = organisation;
            this.Location = location;
            this.Country = country;
        }
    }
}