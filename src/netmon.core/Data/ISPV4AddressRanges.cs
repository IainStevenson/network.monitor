namespace netmon.core.Data
{
    /// <summary>
    /// Fore future use. Information I discovered.
    /// </summary>
    public enum ISPV4AddressRanges
    {
        [IPVAddressRange("185.153.238.0/24", "Hutchison 3G UK Limited", "Reading England","United Kingdom")]
        Three,
        [IPVAddressRange("216.239.32.0/19", "Google LLC","Mountain View, California"," United States")]
        [IPVAddressRange("142.250.0.0/15", "Google LLC", "Fremont, California", "United States")]
        [IPVAddressRange("8.8.8.0/24", "Google LLC", "Mountain View, California", "United States")]
        Google,
        [IPV4AddressRange("172.70.94.0/24", "Cloudflare, Inc.", "London EC1A","United Kingdom")]
        Cloudflare,
        [IPV4AddressRange("87.237.16.0/21", "EE Limited", "London, EC1A", "United Kingdom")]
        EE
    }
}