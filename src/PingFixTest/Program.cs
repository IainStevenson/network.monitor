using System.Net.NetworkInformation;

var ping = new Ping();
var reply = ping.Send("8.8.8.8", 5000, new byte[32], new PingOptions(1, false)); // Remove PingOptions to make is succeed
Console.WriteLine($"Status: {reply.Status} Address: {reply.Address}");