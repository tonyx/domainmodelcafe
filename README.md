# Domain Modeling F# sample

A domain modeling exercise in F#

## Cafe:

Entities and/or values involved are: the cafe, the available foods, the table and the orderitems.
There are plain functions (or member methods) to those entities/values.
The root (the cafe) has functions that are based on the various entities/values functions, and act 
consistently retuning a new cafe.
Events are "wrappers" for such "root" functions.
Commands produce events (or errors).

In a real application we just need to deal with how to store (and reapply) events, and how to store and reading snapshots.

### How to run tests:
in cafemodelingtest directory using terminal/command line console:
```
    dotnet run
```

## Hotel:

The hotel is similar.
There are room and bookings.
There are functions defined at the hotel level that are wrapped in events that are wrapped in commands
in a similar way as in the Cafe example.

### How to run tests:
in hotelmodelingtest directory using terminal/command line console:
```
    dotnet run
```

## Not done yet:
move some events and command related methods that are similar, in a common library/project




