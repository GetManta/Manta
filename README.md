![logo](docs/logo.png)

# Manta
Event store library for .NET Core based on rdbms persistance.

*A thin (but powerful) slice of library between your code and rdbms.*

# Status
ALMOST READY FOR PRODUCTION USAGE

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
 - Support up-conversion of events to the newest version
 - Subscriptions to one or many event stream sources for processmanagers/projections/others
 - MS SQL Server - Manta implementation (with single-writer pattern)
 - MS SQL Server - Manta.Projections implementation

last but not least, performance

 - on i5 2500k with SSD benchmarked ~15,000 writes per second
 - on i7 8700k with SSD benchmarked ~25,000 writes per second (reading streams are really fast)

*SSD disk speed is an important factor*

### To be done
 - Manta - InMemory implementation
 - Manta.Projections - InMemory implementation
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

ISerializer serializer = new JilSerializer(); // You can use any type of serialization method
ArraySegment<byte> payload = serializer.Serialize(ev);
string contractName = GetContractNameBasedOnEventType(ev.GetType());

var data = new UncommittedMessages(
    SequentialGuid.NewGuid(), // correlationId
    new []
    {
        new MessageRecord(
            SequentialGuid.NewGuid(), // messageId
            contractName,
            payload)
    });

await store.AppendToStream(
    "stream-1",
    ExpectedVersion.NoStream,
    data);
```

More examples you can find on [wiki](https://github.com/getmanta/manta/wiki).

# Documentation
If you're looking for documentation, you can find it [here](https://github.com/getmanta/manta/wiki).

# Contributing
A contribution is welcome. Checkout [contributing rules](https://github.com/getmanta/manta/blob/master/CONTRIBUTING.md).
