# network.monitor


This is a work in progress!

A command line tool and reusable libraries that use the .NET PING to test connections to remote addresses.

Uses PING and trace route techniques and can call out to external code such as the ookla speed test cli to monitor and record your networks connection to the internet in terms of access (when was I connected), reliability (when did it fail) and performance (how fast it was).

`Access` is determined by being able to ping one ore more remote IP addresses that you choose. The default is the dns.google address at 8.8.8.8, and all of the hops in between you and california. All are hops are then pinged independently every x seconds after discovery. The reason behind the multiple hop pinging is that your destination address may be down but all of the hops to get there may not. This becomes a vital clue in figuring out what any problem with your might be.

`Reliability` is determined by continually accessing those remote addresses onve every x seconds and recording the respones with UTC date and time stamps (unlike PING.EXE). 

`Performance` is determined optionally by regularly performing e.g. ookla, speed tests to grab actual bandwidth statistics. The frequency of this is once per hour at the same time (xx:30) each hour so as to ensure 'hour of the day' based histogram values for statistical reports. This being a large data consuming event its recommended you adjust this period to suit your data plan. I use 20 minutes for 3 times an hour on an unlimited data plan. This consumes <= 72 * ~150Mb  == 10.8 Gb of data per day!

Deployable in two containers, 1 for capture .net core CLI, and 1 for storage (MongoDb). Analysis and reporting can then bedone from the CLI on any host that can access the storage container.


# Limitations

Initially works only on IPV4 addresses. IPV6 will follow. Many elements of the code handle 1PV6 addresses already but as I dont have that capability here (my ISP is IPV4 only) I can't test it out.

# Summary

This tool should be all you need to be able to keep a track of how your ISP is serving you and gain both quantative and qualitative data on how well it has done. 

# Requirements
For use
* Visual Studio Code
* .NET 6 or above runtime framework
* Windows, linux or IoS operating system.
* A functional internet connected network already setup and working. NOTE: This tool does not help you setup your network at all.

# Get started

Download this repository

In the repository root folder execute ```.\netmon --monitor``` to start a background task to ping the internet and record results. A bit like a submarine.

```.\netmon --status``` will show you how you are doing over the last 5 minutes.

See the usage read me for more cli options.


# Development Notes:

I tackled some interesting coding problems during this process.

* Async processing.
* Serialisation of System.Net.NetworkInformation.IPAddress
* Writing of ASCII line graphs to console devices and text streams.

