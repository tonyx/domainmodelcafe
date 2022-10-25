# Domain Modeling F# sample

A domain modeling exercise in F#

## Cafe:

Entities and/or values involved are the cafe, the available foods, the table, and the orderitems. There are plain member methods in those entities/values that return new entities/values. Events are "wrappers" for root methods. Commands produce events (or an error).

In a real application, we just need to deal with how to store (and reapply) events, and how to store and read snapshots.

### How to run tests:
in cafemodelingtest directory using terminal/command line console:
```
    dotnet run
```

## Hotel:
The hotel example is similar to the cafe example. There are rooms and bookings. There are methods defined at the hotel level that are wrapped in events that are wrapped in commands in a similar way as in the Cafe example. Edit: events are stored in a Postgres database that needs setup. The project will not compile if the db isn't ready.

### How to run tests:
in hotelmodelingtest directory using terminal/command line console:
```
    dotnet run
```

## Not done yet:
1) move some events and command related methods that are similar, in a common library/project
2) store events somewhere
3) ...




