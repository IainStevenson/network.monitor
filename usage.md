# Usage

To get started you can simply go for the automatic setup ```.\netmon --monitor``` which you should just let continue as a long running task. 

This code is suitable for embeeding in a countainer and placing on home NAS systems if you have that capability.

You can also set it up as tasksched (Windows), or other utility job on your PC to start when it boots.

It will only record if the host it is on is running. If the host sleeps so does it.

## Help

You can get syntax help as follows;

```
netmon --help [option]
```

```
netmon [option] [sub options]

monitor --monitor -m: continuously monitors according to the current configuration, performs a --configure --auto to the specified address or 8.8.8.8
analyse --analyse -a: runs the analysis of the recorded data for reporting.
report --report -r: produces one or more of the available report types.
configure --configure -c: sets individual configuration items also allowing scheduling of bandwidth tests.
bandwidth --bandwidth -b: calls out to ookla speedtest cli to test and record your current bandwidth (uses lots of data beware!)
```

