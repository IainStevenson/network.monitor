# Solution

# Structure

The solution is broken into separate projects to focus the code into distinct application responsibilities.

# netmon.core (capture)

The library of handlers, orchestrators and data classes used to ping, trace route and speed test your network access, and to record the captured data in a usable form.

# netmon.storage

# netmon.analysis

# netmon.reproting

# netmon.cli (command and control)

The command line interface used to initiate monitoring, and modify its behaviour.

# netmon.core.tests

The normal practice of unit isolation during testing is temporarily suspended here for a good reason.

The unit tests are being used, during development, in place of a console app to execute the code to see how ping and trace route actuall work. I dont want the app requirements to influence the code, quite the reverse.

The other reasons is that it wont be possible to automate these tests on a non networked computer, and even if such a host is behind a DMZ and proxy, it can be configured to work within the local area nework and test that instead. Also minute quantities of bandwidth are consumed.

# Structure

The solution is broken into separate projects to focus the code into distinct application responsibilities.

# netmon.core (capture)

The library of handlers, orchestrators and data classes used to ping, trace route and speed test your network access, and to record the captured data in a usable form.

# netmon.storage

# netmon.analysis

# netmon.reproting

# netmon.cli (command and control)

The command line interface used to initiate monitoring, and modify its behaviour.

# netmon.core.tests

The normal practice of unit isolation during testing is temporarily suspended here for a good reason.

The unit tests are being used, during development, in place of a console app to execute the code to see how ping and trace route actuall work. I dont want the app requirements to influence the code, quite the reverse.

The other reasons is that it wont be possible to automate these tests on a non networked computer, and even if such a host is behind a DMZ and proxy, it can be configured to work within the local area nework and test that instead. Also minute quantities of bandwidtn are consumed.