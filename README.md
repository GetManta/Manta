# Manta
Event store library for .NET Core based on rdbms persistance.

*A thin (but powerful) slice of library between your code and rdbms.*

# Status
NOT READY FOR PRODUCTION YET

[![Build status](https://ci.appveyor.com/api/projects/status/rmy0b570j1ur2c58/branch/master?svg=true)](https://ci.appveyor.com/project/dario-l/manta/branch/master)

# Installation
Manta is not available on NuGet, yet.

You can download source code from this repository and compile under Visual Studio 2017.

# Main goals

### Done
 - Async all the way down
 - No external dependencies
 - Pluggable persistance storage
 - Idempotent writes
 - Support optimistic concurrency
 - Support any kind of message serialization
 - Support any kind of loggers through ILogger interface for Manta internal logging
 - MS SQL Server persistance implementation (with single-writer pattern for MS SQL Server implementation)
 - Support up-conversion of events to the newest version
 - Subscriptions to one or many event stream sources for processmanagers/projections/others

last but not least, performance

 - on i5 2500k with SSD benchmarked ~15,000 writes per second and ~30,000 reads per second
 - on i7 8700k with SSD benchmarked ~25,000 writes per second and ~50,000 reads per second

*SSD disk speed is an important factor*

### To be done
 - Manta - InMemory implementation
 - Manta.PostgreSql - [PostgreSql](https://www.postgresql.org/) implementation
 - Manta.MySql - [MySql](https://www.mysql.com/) implementation
 - Manta.SqLite - [SqLite](https://www.sqlite.org/) implementation

### Others
 - Manta.Domain (as different repository)
   - Manta.Domain - Conflict resolver, repository pattern, unit of work, e.t.c.


# Getting started

```c#

// Simple class representing event/message which will be stored

[DataContract]
public class SomeEvent
{
    [DataMember(Order = 1)]
    public string Name { get; private set; }

    public SomeEvent(string name)
    {
        Name = name;
    }
}

// Basic example of usage

string contractName = GetContractNameBasedOnEventType(ev.GetType());
ArraySegment<byte> payload = SerializeEventUsingProtoBuf(ev); // You can use any type of serialization method

var data = new UncommittedMessages(
    SequentialGuid.NewGuid(), // correlationId
    new []
    {
        new MessageRecord(
            SequentialGuid.NewGuid(), // messageId
            contractId,
            payload)
    });

await store.AppendToStream(
    "stream-1",
    ExpectedVersion.NoStream,
    data);
```

# Documentation
If you're looking for documentation, you can find it [here](https://github.com/getmanta/manta/wiki) when it will be ready.

# Contributing
A contribution is welcome. Checkout [contributing rules](https://github.com/GetManta/manta/blob/master/CONTRIBUTING.md).
