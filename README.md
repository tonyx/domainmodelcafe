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
or
```
    dotnet test
```

## Hotel:
The hotel example is an evolution to the cafe example. There are rooms and bookings. Events, commands, and domain objects are seralizable via json. Events and snapshots are stored in a Postgres database that needs proper setup.  Note: proper postgres setup is needed to compile. See the dmhotel.slq script and Db.fs source file to figure out proper dbname and user and user credentials needed.

### How to run tests:
in hotelmodelingtest directory using terminal/command line console:
```
    dotnet run
```
or
```
    dotnet test
```

## Todo:
1) move some events and command related methods that are similar, in a common library/project
2) a web interface
3) evolve the cafe subproject in the same way as hotel subproject did (serialize and store events)





