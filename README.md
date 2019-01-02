# dotnetcoreapmextension
CORECLR Extension AppDynamics
This extension reports CORE CLR Metrics for GC Events (Gen0, Gen1 and Gen2). Large Object Heap Allocations, Handle and Thread Counts by monitor Event Trace for Windows
The extension is written using .NET Framework v4.6x and designed to be run as a custom script extension documented here https://docs.appdynamics.com/display/PRO45/Build+a+Monitoring+Extension+Using+Scripts

This project is based off the git projects here https://github.com/Microsoft/perfview/blob/ebc8046a8a70853fa1276089f2e0a07ec14f14fa/src/TraceEvent/Samples/20_ObserveGCEvent.cs and here https://github.com/gusemery/AppDynamicsCoreMetricsMonitor
When SIM is enabled metrics are reported to the Custom Metrics | Nodes | %COMPUTERNAME% | (Memory|Process) | %PROCESSNAME% | 
