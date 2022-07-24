# network.monitor


Work In progress!

A command line tool and reusable libraries that use the .NET PING to test connections to remote addresses.

Uses PING and TRACERT techniques and can call out to external code such as the ookla speed test cli to monitor and record your networks connection to the internet in terms of access, reliability and performance.

`Access` is determined by being able to ping remote IP addresses you choose. 

`Reliability` is determined by continually accessing those remote addresses and recording the respones with UTC date and time stamps (unlike PING.EXE). 

`Performance` is determined by regularly performing ookla speed tests to grab actual bandwidth statistics.

# Limitations

Initially works only on IPV4 addresses. IPV6 will follow.

# Summary

This tool should be all you need to be able to keep a track of how your ISP is serving you and gain both quantative and qualitative data on how well it has done. 

# Requirements
For use
* Visual Studio Code
* .NET Runtime framework
* Windows, linux or IOS operating system.
* A functional internet connected network already setup and working. NOTE: This tool does not help you setup your network at all.



# Get started

Download this repository
In the repository root folder execute ```.\netmon --monitor```


# Notes:

For those looking for a way to serialise System.Net.IPAddress reliably to and from json, either alone or in Lists/Dictionaries. Look in the tests and Serialisation namespace code.