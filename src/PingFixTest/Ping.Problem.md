# Ping problem on Linux

# Observable Facts:

On a Linux container when performing a trace route operation via System.Net.NetworkInformation.Ping with a TTL < actual requried to reach the address: l, recieving a status of Timeout instead of TTLExpired with the responding hots address.

## Arrange
```
Address: 8.8.8.8
TTL: 1 ( use any number < 15 as actual 15 hops to 8.8.8.8 from here)
Timeout: 1000 ms
DontFragment : True
```
## Act 

Call Synchronous Ping and await the response.

## Expecting 
```
Reply Status = TTLExpired
Reply address is valid and my gateway
```
## Actual
```
Address: Null
Status: Timedout
```

# Additional 

On the same Linux container (built from VS docker file), Install the traceroute utility by 

```
apt update
apt-get install traceroute -y
```

```traceroute 8.8.8.8 ```

fails for all nodes except target address with the same problem 

```traceroute -I 8.8.8.8```

Works as expected and as it does on windows.

The -I switch to ensure that ICMP packets are sent.

# .net6

On examining Release 6.0 of .net within System.Net.Ping solution

In System.Net.NetworkInformation.Ping 

Primary class witll call partial class for OS.

```PingReply Send(IPAddress address, int timeout, byte[] buffer, PingOptions? options)```

-> 
```
return SendPingCore(addressSnapshot, buffer, timeout, options); 
```
which is in Ping.Unix.cs

Which gets us here

```
      private PingReply SendPingCore(IPAddress address, byte[] buffer, int timeout, PingOptions? options)
        {
            PingReply reply = RawSocketPermissions.CanUseRawSockets(address.AddressFamily) ?
                    SendIcmpEchoRequestOverRawSocket(address, buffer, timeout, options) :
                    SendWithPingUtility(address, buffer, timeout, options);
            return reply;
        }
```

Which will use Sockets to send a raw ICMP message or call out to the OS ping utility if a socket cannot be created for the address/addressfamily.


```
private PingReply SendIcmpEchoRequestOverRawSocket(IPAddress address, byte[] buffer, int timeout, PingOptions? options)
        {
            SocketConfig socketConfig = GetSocketConfig(address, buffer, timeout, options);
            using (Socket socket = GetRawSocket(socketConfig))
            {
                int ipHeaderLength = socketConfig.IsIpv4 ? MinIpHeaderLengthInBytes : 0;
                try
                {
                    socket.SendTo(socketConfig.SendBuffer, SocketFlags.None, socketConfig.EndPoint);

                    byte[] receiveBuffer = new byte[MaxIpHeaderLengthInBytes + IcmpHeaderLengthInBytes + buffer.Length];

                    long elapsed;
                    Stopwatch sw = Stopwatch.StartNew();
                    // Read from the socket in a loop. We may receive messages that are not echo replies, or that are not in response
                    // to the echo request we just sent. We need to filter such messages out, and continue reading until our timeout.
                    // For example, when pinging the local host, we need to filter out our own echo requests that the socket reads.
                    while ((elapsed = sw.ElapsedMilliseconds) < timeout)
                    {
                        int bytesReceived = socket.ReceiveFrom(receiveBuffer, SocketFlags.None, ref socketConfig.EndPoint);

                        if (bytesReceived - ipHeaderLength < IcmpHeaderLengthInBytes)
                        {
                            continue; // Not enough bytes to reconstruct IP header + ICMP header.
                        }

                        if (TryGetPingReply(socketConfig, receiveBuffer, bytesReceived, sw, ref ipHeaderLength, out PingReply? reply))
                        {
                            return reply;
                        }
                    }
                }
                catch (SocketException ex) when (ex.SocketErrorCode == SocketError.TimedOut)
                {
                }
                catch (SocketException ex) when (ex.SocketErrorCode == SocketError.MessageSize)
                {
                    return CreatePingReply(IPStatus.PacketTooBig);
                }

                // We have exceeded our timeout duration, and no reply has been received.
                return CreatePingReply(IPStatus.TimedOut);
            }
        }
```

Or ping utility:

```
private PingReply SendWithPingUtility(IPAddress address, byte[] buffer, int timeout, PingOptions? options)
        {
            using (Process p = GetPingProcess(address, buffer, timeout, options))
            {
                p.Start();
                if (!p.WaitForExit(timeout) || p.ExitCode == 1 || p.ExitCode == 2)
                {
                    return CreatePingReply(IPStatus.TimedOut);
                }

                try
                {
                    string output = p.StandardOutput.ReadToEnd();
                    return ParsePingUtilityOutput(address, output);
                }
                catch (Exception)
                {
                    // If the standard output cannot be successfully parsed, throw a generic PingException.
                    throw new PingException(SR.net_ping);
                }
            }
        }
```

Truth table

Can send ICMP socket
Does and it fails with timeout?
Cannot send Socket ICMP
Recerts to ping on Os and it fails
Reverts to ping on Os and it does not exist, and defaults to timeout?
```
                p.ExitCode == 2)
                {
                    return CreatePingReply(IPStatus.TimedOut);
                }
```
This should not be happening because an exception will be thrown and no evidence of that is occuring.

Therefore the socket option is being used and failing. Why!?


