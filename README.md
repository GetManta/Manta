# Manta
Event store library for .NET Core based on rdbms persistance.

*A thin (but powerful) slice of library between your code and rdbms.*

# Installation
Manta is not available on NuGet, yet.

You can download source code from this repository and compile under Visual Studio 2017.

# Main goals

### Implemented
 - Async all the way down (sync methods also available)
 - No external dependencies
 - Pluggable persistance storage
 - Support optimistic concurrency with conflict resolver mechanism
 - Support any kind of message serialization
 - Support any kind of loggers through ILogger interface
 - MS SQL Server persistance implementation

### To be done
 - Manta - InMemory implementation
 - Manta.PostgreSql - [PostgreSql](https://www.postgresql.org/) implementation
 - Manta.MsSql - Single-writer pattern for MS SQL Server implementation (Linearizer)
 - Manta.Sceleton - Support up-conversion of events to the newest version
 - Manta.Subscriptions - Subscriptions to one or many event stream sources for processmanagers/projections/others (with pluggable stream pollers)

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

int contractId = GetContractIdentifierBasedOnEventType(ev.GetType());
byte[] payload = SerializeEventUsingProtoBuf(ev);

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
    SequentialGuid.NewGuid().ToString("N"),
    ExpectedVersion.NoStream,
    data);

```

# Documentation
If you're looking for documentation, you can find it [here](https://github.com/GetManta/manta/wiki) when it will be ready.

# Contributing
Contribution is welcome but rules are not set, yet.
