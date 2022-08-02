Ping class responding Timedout not reading response when ICMP Time-to-live exceeded.

With reference to https://github.com/dotnet/runtime/issues/61465 it appears that the fixed produced is not working, at least on linux.

I read the above issue and it matched my scenario so was expecting update my framework/sdk's and move on.

I noted that the fix was not included in 6.0.7 so I pushed on to preview .net 7.

# Dev environment

Visual Studio 2022 Version 17.2.6 
SDK .NET 7 preview 6 installed and allowed.
Docker Desktop 4.10.1 (82475) is currently the newest version available.
System.Net.Ping file version 7.0.22.32404.

Note: its a few hops from the container to my outer WAN address. 192.168.1.1

# IPV6

I dont have facility to test IPV6.

# Code used.

The code suggested for a test from Issue #61465 was used with the ttl changed to suit my environment.

```
using System.Net.NetworkInformation;

var ping = new Ping();
var reply = ping.Send("8.8.8.8", 5000, new byte[32], new PingOptions(3, false)); // Remove PingOptions to make is succeed
Console.WriteLine($"Status: {reply.Status} Address: {reply.Address}");

```

# Expectations

Works on windows. It does.
`Status: TtlExpired Address: 192.168.1.1`

Works in linux container: 
It fails with `Status: TimedOut Address: 0.0.0.0`

# Dockerfile file. 

Used to generate the container

Visual Studio generated and nothing untoward here.

```
#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["PingFixTest.csproj", "."]
RUN dotnet restore "./PingFixTest.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "PingFixTest.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PingFixTest.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PingFixTest.dll"]
```

# Docker log file

Status: TimedOut Address: 0.0.0.0

Please note: I tried running container with docker network default and again with `--network host`. It made no difference.

# Linux command line on runing container

I installed the necessary utilities.

```
apt update
apt-get install -y traceroute
apt-get install -y iputils-ping

```

```
# traceroute 8.8.8.8
traceroute to 8.8.8.8 (8.8.8.8), 30 hops max, 60 byte packets
 1  172.17.0.1 (172.17.0.1)  0.880 ms  0.749 ms  0.625 ms
 2  * * *
 3  * * *
 4  * * *
 5  * * *
 6  * * *
 7  * * *
 8  * * *
 9  * * *
10  * * *
11  * * *
12  * * *
13  * * *
14  * * *
15  * * *
16  * * *
17  * * *
18  * * *
19  * * *
20  * * *
21  * * *
22  * * *
23  * * *
24  * * *
25  * * *
26  * * *
27  * * *
28  * * *
29  * * *
30  * * *
#
```

'172.17.0.1' being the docker `bridge` network gateway.
By now I expected to see the above result. Note it expires after the max hop (default 30) and does not even get the Success from the final hop to the address (15).

I find it intresting the docker network gateway response works but not outside. 

However interesting that may be the test code fails in the Ping class at the docker gateway address hop 1. So that may be a red herring.

```
# traceroute -I 8.8.8.8
traceroute to 8.8.8.8 (8.8.8.8), 30 hops max, 60 byte packets
 1  172.17.0.1 (172.17.0.1)  0.422 ms  0.242 ms  0.219 ms
 2  192.168.0.1 (192.168.0.1)  8.790 ms  8.703 ms  9.670 ms
 3  192.168.1.1 (192.168.1.1)  9.675 ms  9.676 ms  10.607 ms
 4  * * *
 5  10.248.27.157 (10.248.27.157)  55.479 ms  55.487 ms  55.488 ms
 6  10.247.87.167 (10.247.87.167)  54.488 ms  39.425 ms  39.327 ms
 7  * * *
 8  10.247.87.113 (10.247.87.113)  38.963 ms  45.442 ms  45.532 ms
 9  10.247.87.142 (10.247.87.142)  45.534 ms  46.442 ms  51.346 ms
10  87.237.20.146 (87.237.20.146)  46.036 ms  46.340 ms  46.736 ms
11  87.237.20.67 (87.237.20.67)  51.395 ms  41.240 ms  34.692 ms
12  72.14.242.70 (72.14.242.70)  34.497 ms  41.573 ms  41.030 ms
13  108.170.246.129 (108.170.246.129)  41.314 ms  40.970 ms  41.984 ms
14  142.251.54.25 (142.251.54.25)  35.644 ms  43.839 ms  59.243 ms
15  dns.google (8.8.8.8)  48.850 ms  48.872 ms  52.435 ms
#
```
I also expected to see the above result as it forces traceroute to use ICMP messages (Which Ping.cs is doing) and it all works wonderfully via traceroute.exe :)

Note: First three hops are docker and my equipment (I have dual WAN) , hop 4 is my entry point to the internet, hops 5-9 are EE mobile network internal class A addresses. Then are hops from UK to the USA.

This is very normal for here.

```
# ping -t 3 8.8.8.8
PING 8.8.8.8 (8.8.8.8) 56(84) bytes of data.
From 192.168.1.1 icmp_seq=1 Time to live exceeded
From 192.168.1.1 icmp_seq=2 Time to live exceeded
From 192.168.1.1 icmp_seq=3 Time to live exceeded
From 192.168.1.1 icmp_seq=4 Time to live exceeded
From 192.168.1.1 icmp_seq=5 Time to live exceeded
```

So, the container is connected to the internet and its networking is performaing as expected.

My conclusion is that the fix for issue #61465 is not enough, or `Missing In Action` in this release for some reason.

# Further information 

Wireshark data out and in from container code run.

## Request

```
Frame 51: 74 bytes on wire (592 bits), 74 bytes captured (592 bits) on interface \Device\NPF_{AF47E508-7C19-4E4B-8832-9499515FEC65}, id 0
    Interface id: 0 (\Device\NPF_{AF47E508-7C19-4E4B-8832-9499515FEC65})
        Interface name: \Device\NPF_{AF47E508-7C19-4E4B-8832-9499515FEC65}
        Interface description: Ethernet
    Encapsulation type: Ethernet (1)
    Arrival Time: Aug  2, 2022 14:01:41.082932000 GMT Summer Time
    [Time shift for this packet: 0.000000000 seconds]
    Epoch Time: 1659445301.082932000 seconds
    [Time delta from previous captured frame: 0.136255000 seconds]
    [Time delta from previous displayed frame: 0.000000000 seconds]
    [Time since reference or first frame: 4.793458000 seconds]
    Frame Number: 51
    Frame Length: 74 bytes (592 bits)
    Capture Length: 74 bytes (592 bits)
    [Frame is marked: False]
    [Frame is ignored: False]
    [Protocols in frame: eth:ethertype:ip:icmp:data]
    [Coloring Rule Name: ICMP]
    [Coloring Rule String: icmp || icmpv6]
Ethernet II, Src: Microsof_00:16:0e (00:15:5d:00:16:0e), Dst: TP-Link_e4:b4:6e (14:eb:b6:e4:b4:6e)
    Destination: TP-Link_e4:b4:6e (14:eb:b6:e4:b4:6e)
        Address: TP-Link_e4:b4:6e (14:eb:b6:e4:b4:6e)
        .... ..0. .... .... .... .... = LG bit: Globally unique address (factory default)
        .... ...0 .... .... .... .... = IG bit: Individual address (unicast)
    Source: Microsof_00:16:0e (00:15:5d:00:16:0e)
        Address: Microsof_00:16:0e (00:15:5d:00:16:0e)
        .... ..0. .... .... .... .... = LG bit: Globally unique address (factory default)
        .... ...0 .... .... .... .... = IG bit: Individual address (unicast)
    Type: IPv4 (0x0800)
Internet Protocol Version 4, Src: 192.168.0.122, Dst: 8.8.8.8
    0100 .... = Version: 4
    .... 0101 = Header Length: 20 bytes (5)
    Differentiated Services Field: 0x00 (DSCP: CS0, ECN: Not-ECT)
        0000 00.. = Differentiated Services Codepoint: Default (0)
        .... ..00 = Explicit Congestion Notification: Not ECN-Capable Transport (0)
    Total Length: 60
    Identification: 0xccb6 (52406)
    Flags: 0x00
        0... .... = Reserved bit: Not set
        .0.. .... = Don't fragment: Not set
        ..0. .... = More fragments: Not set
    ...0 0000 0000 0000 = Fragment Offset: 0
    Time to Live: 2
        [Expert Info (Note/Sequence): "Time To Live" only 2]
            ["Time To Live" only 2]
            [Severity level: Note]
            [Group: Sequence]
    Protocol: ICMP (1)
    Header Checksum: 0x0000 [validation disabled]
    [Header checksum status: Unverified]
    Source Address: 192.168.0.122
    Destination Address: 8.8.8.8
Internet Control Message Protocol
    Type: 8 (Echo (ping) request)
    Code: 0
    Checksum: 0xf7f1 [correct]
    [Checksum Status: Good]
    Identifier (BE): 14 (0x000e)
    Identifier (LE): 3584 (0x0e00)
    Sequence Number (BE): 0 (0x0000)
    Sequence Number (LE): 0 (0x0000)
    [No response seen]
        [Expert Info (Warning/Sequence): No response seen to ICMP request]
            [No response seen to ICMP request]
            [Severity level: Warning]
            [Group: Sequence]
    Data (32 bytes)
        Data: 0000000000000000000000000000000000000000000000000000000000000000
        [Length: 32]
```



## Response

```
Frame 52: 102 bytes on wire (816 bits), 102 bytes captured (816 bits) on interface \Device\NPF_{AF47E508-7C19-4E4B-8832-9499515FEC65}, id 0
    Interface id: 0 (\Device\NPF_{AF47E508-7C19-4E4B-8832-9499515FEC65})
        Interface name: \Device\NPF_{AF47E508-7C19-4E4B-8832-9499515FEC65}
        Interface description: Ethernet
    Encapsulation type: Ethernet (1)
    Arrival Time: Aug  2, 2022 14:01:41.084832000 GMT Summer Time
    [Time shift for this packet: 0.000000000 seconds]
    Epoch Time: 1659445301.084832000 seconds
    [Time delta from previous captured frame: 0.001900000 seconds]
    [Time delta from previous displayed frame: 0.001900000 seconds]
    [Time since reference or first frame: 4.795358000 seconds]
    Frame Number: 52
    Frame Length: 102 bytes (816 bits)
    Capture Length: 102 bytes (816 bits)
    [Frame is marked: False]
    [Frame is ignored: False]
    [Protocols in frame: eth:ethertype:ip:icmp:ip:icmp:data]
    [Coloring Rule Name: ICMP errors]
    [Coloring Rule String: icmp.type eq 3 || icmp.type eq 4 || icmp.type eq 5 || icmp.type eq 11 || icmpv6.type eq 1 || icmpv6.type eq 2 || icmpv6.type eq 3 || icmpv6.type eq 4]
Ethernet II, Src: TP-Link_e4:b4:6e (14:eb:b6:e4:b4:6e), Dst: Microsof_00:16:0e (00:15:5d:00:16:0e)
    Destination: Microsof_00:16:0e (00:15:5d:00:16:0e)
        Address: Microsof_00:16:0e (00:15:5d:00:16:0e)
        .... ..0. .... .... .... .... = LG bit: Globally unique address (factory default)
        .... ...0 .... .... .... .... = IG bit: Individual address (unicast)
    Source: TP-Link_e4:b4:6e (14:eb:b6:e4:b4:6e)
        Address: TP-Link_e4:b4:6e (14:eb:b6:e4:b4:6e)
        .... ..0. .... .... .... .... = LG bit: Globally unique address (factory default)
        .... ...0 .... .... .... .... = IG bit: Individual address (unicast)
    Type: IPv4 (0x0800)
Internet Protocol Version 4, Src: 192.168.1.1, Dst: 192.168.0.122
    0100 .... = Version: 4
    .... 0101 = Header Length: 20 bytes (5)
    Differentiated Services Field: 0xc0 (DSCP: CS6, ECN: Not-ECT)
        1100 00.. = Differentiated Services Codepoint: Class Selector 6 (48)
        .... ..00 = Explicit Congestion Notification: Not ECN-Capable Transport (0)
    Total Length: 88
    Identification: 0x8494 (33940)
    Flags: 0x00
        0... .... = Reserved bit: Not set
        .0.. .... = Don't fragment: Not set
        ..0. .... = More fragments: Not set
    ...0 0000 0000 0000 = Fragment Offset: 0
    Time to Live: 63
    Protocol: ICMP (1)
    Header Checksum: 0x7385 [validation disabled]
    [Header checksum status: Unverified]
    Source Address: 192.168.1.1
    Destination Address: 192.168.0.122
Internet Control Message Protocol
    Type: 11 (Time-to-live exceeded)
    Code: 0 (Time to live exceeded in transit)
    Checksum: 0xf4ff [correct]
    [Checksum Status: Good]
    Unused: 00000000
    Internet Protocol Version 4, Src: 192.168.0.122, Dst: 8.8.8.8
        0100 .... = Version: 4
        .... 0101 = Header Length: 20 bytes (5)
        Differentiated Services Field: 0x00 (DSCP: CS0, ECN: Not-ECT)
            0000 00.. = Differentiated Services Codepoint: Default (0)
            .... ..00 = Explicit Congestion Notification: Not ECN-Capable Transport (0)
        Total Length: 60
        Identification: 0xccb6 (52406)
        Flags: 0x00
            0... .... = Reserved bit: Not set
            .0.. .... = Don't fragment: Not set
            ..0. .... = More fragments: Not set
        ...0 0000 0000 0000 = Fragment Offset: 0
        Time to Live: 1
            [Expert Info (Note/Sequence): "Time To Live" only 1]
                ["Time To Live" only 1]
                [Severity level: Note]
                [Group: Sequence]
        Protocol: ICMP (1)
        Header Checksum: 0x1bd9 [validation disabled]
        [Header checksum status: Unverified]
        Source Address: 192.168.0.122
        Destination Address: 8.8.8.8
    Internet Control Message Protocol
        Type: 8 (Echo (ping) request)
        Code: 0
        Checksum: 0xf7f1 [unverified] [in ICMP error packet]
        [Checksum Status: Unverified]
        Identifier (BE): 14 (0x000e)
        Identifier (LE): 3584 (0x0e00)
        Sequence Number (BE): 0 (0x0000)
        Sequence Number (LE): 0 (0x0000)
        Data (32 bytes)
            Data: 0000000000000000000000000000000000000000000000000000000000000000
            [Length: 32]

```

Please let me know if there is anything I can help with in resolving this.