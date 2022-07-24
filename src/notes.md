`***** This is a work in progress ******`

More than a little fed up with eratic performance from my ISP of late, I decided to use technology and my long years of experience writing software to give me some empircal evidence of how good or bad the performance actually is/was. 

In other words take the subjective 'Grrr  f*s' out of the moment.

I wanted to be able to go to a command line and issue a command that tells me what is wrong right now. Such as;

```netmon --status```

Which will report on summary connectivity over the last 15 minutes by default. With options to lengthen the past period by degrees, ```--minutes=#minutes```, ```--hours=#hours```, ```--days=#days``` and even addresses ```--address=address1[,address2]```.

More about the command line interface is detailed elsewhere.

The report will detail and focus on failures of normal connectivity detected during that period according to the qualifiers.

The reality is that the network may be down for a number of reasons. I would like to go to my ISP and say X, Y and X are true - what are you going to do about it. 

The reports will also plug into subjective decision making processes about how I obtain my internet connection in future.

This involves a number of stages in using ICMP messages to a check connection 'now' to a range of remote hosts.

* My hosts are at fault.
* My ISP hosts are at fault.
* The Interent generally is at fault.

The objective is to try and gather data and analyse it to determine where the fault actually is at any given time. E.g. every 5 seconds.

* Capture of raw ping response data
* Storage of the captured data.
* Enalysis of the stored data.
* Reporting on the analaysis.

The primary intention is to provide evidence to back up answers to the following questions.

* What time periods were there no connectivity to a variety of world network addresses.
* What characterises the lack of connectivity. Where did it fail!
    * Local hosts, isp hosts, dns hosts - down?
* Where blame may lie for the loss in connectivity.
    * Which type of host was not responding.

# General

The following abilities should be provided. 

* Run on cross platform hosts.
* Run within containers.
* Run unattended and start on host startup.
* Provide a command line interface (CLI) for all of its configuration, operation, and reporting functions.
* Provide a configuration for automated processing.
* Provide a suite of report types for use in other applciations.
* Consume as little data as possible for maximum information.

