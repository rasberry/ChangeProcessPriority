# ChangeProcessPriority
Change priority of a process one time or continuously.

Usage:

```
ChangeProcessPriority [options] (priority) (list of process names)
Priorities:
  Normal, Idle, High, RealTime, BelowNormal, AboveNormal
 Options:
  -d  daemon mode - continues to watch for new instances of the process(s) and changes the priority for each one
```
