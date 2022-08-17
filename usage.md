# Usage

To get started you can simply go for the automatic setup ```.\netmon --monitor``` which you should just let continue as a long running task. 

This code is suitable for embedding in a countainer and placing on home NAS systems if you have that capability.

You can also set it up as tasksched (Windows), or other utility job on your PC to start when it boots.

It will only record if the host it is on is running. If the host sleeps so does it.

## Help

You can get syntax help as follows;

```
netmon --help [option]
```

```
netmon [option] [sub options]

monitor --monitor -m: continuously monitors according to the specified addresses (default is 8.8.8.8) and stores in teh configured locations or as a default, the 'Environment.SpecialFolder.CommonApplicationData' netmon subfolder which is created if it does not exist.
store  --store -s: stores files in the output path that are not already in the storage system, and cleans the output path of files that are stored. this allows a recovery from a storage outage. the output path is a failsafe for storage system outages.
analyse --analyse -a: runs the analysis of the recorded data for reporting. This queries the storage system not the file system.
report --report -r: produces one or more of the available report types form the previous analysis.
bandwidth --bandwidth -b: calls out to ookla speedtest cli to test and record your current bandwidth (uses lots of data beware!)
```

## monitor
## analyse
## store
## bandwidth
## report
