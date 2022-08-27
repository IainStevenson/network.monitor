using System.Diagnostics.CodeAnalysis;

namespace netmon.domain.Models
{
    public enum HostTypes
    {
        /// <summary>
        /// Addresses manually/semi automatically declared as belonging to this site. 
        /// This may be the first on the traceroute list if the monitor is started first time with the roaming option set to false.
        /// It may or may not be a private address.
        /// </summary>
        Local,
        /// <summary>
        /// The address is in the range of Class A B C D or E private addresses. 
        /// These may be addresses on the internet providing private circuits across internet links.
        /// </summary>
        Private,
        /// <summary>
        /// Are private or public addresses which either first in the trace route host list, 
        /// or occur immediately following the last local address in a trace route host response list, 
        /// or are in the same subnet as the first identified Isp host type
        /// </summary>
        Isp,
        /// <summary>
        /// Are non private addresses which are not identified as Isp types
        /// </summary>
        Public,
    }
}
