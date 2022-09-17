namespace hotelmodeling
open System
open FSharp.Core
open FSharpPlus
open FSharpPlus.Data
open Equinox.EventStore

module MiscUtils =
    let daysToString(days: Set<DateTime>) = 
        let printconflictsdays = 
            days 
                |> Set.fold (fun x y -> (sprintf "%A" (y.ToString("yyy/MM/dd")))+"," + x) ""
        if (printconflictsdays = String.Empty) then
            String.Empty
        else
            printconflictsdays.Substring(0, printconflictsdays.Length-1)

    let OkValue x =
        let (Ok res) = x
        res

    let traverse l f =
        ()

module rec Domain =
    open MiscUtils
    [<CustomEquality; NoComparison>]
    type Room =
        {
            id: int
            description: Option<string>
        }
        with 
            override this.Equals(obj) =
                match obj with
                    | :? Room as r -> this.id = r.id
                    | _ -> false
                override this.GetHashCode() =
                    this.id

    type Booking =
        {
            id: Option<Guid>
            roomId: int
            customerEmail: string
            plannedCheckin: DateTime
            plannedCheckout: DateTime
        }
        with 
            member this.getDaysInterval() =
                let spanInterval = this.plannedCheckout - this.plannedCheckin
                let days = spanInterval.Days
                [
                    for i = 1 to days do 
                        yield (this.plannedCheckin.Date + TimeSpan(i - 1, 0, 0, 0))
                ]

    type UnionEvent =
        | URoomAdded of Room
        | UBookingAdded of Booking
        member this.Process (x: State) =
            match this with
            | URoomAdded r -> x.AddRoom r
            | UBookingAdded b -> x.AddBooking b

    type Event = State -> Result<State, string>
    type Command = State -> Result<NonEmptyList<Event>, string>

    type SEvent = 
        {
            id : Guid
            event: Event
        }

    type SCommand = State -> Result<NonEmptyList<SEvent>, string>

    type State =
        {
            rooms: List<Room>
            bookings: List<Booking>
            id: int
        }
        with 
            static member GetEmpty() =
                {
                    rooms = []
                    bookings = []
                    id = 0
                }
            member this.AddRoom (room: Room): Result<State, string> =
                if ((this.rooms) |> List.contains room) then   
                    sprintf "a room with number %d already exists" room.id |> Error
                else 
                    {
                        this with   
                            rooms = room::this.rooms
                            id = this.id + 1
                    } 
                    |> Ok
            member this.AddBooking (booking: Booking): Result<State, string> =
                let roomExists = this.rooms |> List.exists (fun x -> x.id = booking.roomId) 
                let claimedDays = booking.getDaysInterval() |> Set.ofList
                let alreadyBookedDays = this.GetBookedDaysOfRoom (booking.roomId)
                let conflictingDays = alreadyBookedDays |> Set.intersect claimedDays

                match (booking.id, roomExists, conflictingDays.IsEmpty ) with
                    | Some _, _, _ -> "cannot add a booking that already has an id" |> Error
                    | _, false, _ -> sprintf "room %d doesn't exist" booking.roomId |> Error
                    | _ , _ , false -> (sprintf "overlap: %s" (daysToString(conflictingDays))) |> Error
                    | _ , _ , _ ->    
                        {
                            this with
                                bookings = ({booking with id = Guid.NewGuid()|> Some})::this.bookings
                                id = this.id + 1
                        } 
                        |> Ok
            member this.GetBookedDaysOfRoom roomId =
                    this.bookings 
                    |> List.filter (fun x -> x.roomId = roomId)                    
                    |> List.map (fun x -> x.getDaysInterval())
                    |> List.fold (@) []
                    |> Set.ofList 

            member this.Evolve events =
                events |> NonEmptyList.toList
                |> List.fold 
                    (fun x f -> 
                        match x with
                        | Ok x1 -> f x1
                        | Error x -> Error x
                    ) (this |> Ok)

            member this.UEvolve (events: List<UnionEvent>) =
                events 
                |> List.fold 
                    (fun (acc: Result<State, string>) (x: UnionEvent) -> 
                        match acc with  
                            | Error _ -> acc
                            | Ok y -> x.Process y
                    ) (this |> Ok)

            member this.ProcessSEvents sEvents =
                sEvents 
                |> NonEmptyList.map (fun x -> x.event) 
                |> this.Evolve

            member this.Interpret command =
                match command this with
                | Ok x -> 
                    match this.Evolve x with
                    | Ok _ -> Ok x
                    | Error e -> Error (sprintf "command error: %s" e)
                | Error x ->  Error (sprintf "command error: %s" x)

            member this.ProcessSCommand command =
                match command this with
                | Ok x -> 
                    match this.ProcessSEvents x with
                    | Ok _ -> Ok x
                    | Error e -> Error (sprintf "command error: %s" e)
                | Error x ->  Error (sprintf "command error: %s" x)

    let initState: State = State.GetEmpty()
    type CommandMaker =
        | AddRoom of Room
        | AddBooking of Booking

    let makeCommand commandMaker: Command =
        match commandMaker with
            | AddRoom t ->
                fun _ -> 
                    [fun (x: State) -> x.AddRoom t] 
                    |> NonEmptyList.ofList 
                    |> Ok
            | AddBooking f ->
                fun _ -> 
                    [fun (x: State) -> x.AddBooking f] 
                    |> NonEmptyList.ofList 
                    |> Ok

    let makeSCommand commandMaker: SCommand =
        match commandMaker with
            | AddRoom t ->
                fun _ -> 
                    [
                        {
                            id = Guid.NewGuid()
                            event = fun (x: State) -> x.AddRoom t
                        }
                            
                    ] 
                    |> NonEmptyList.ofList 
                    |> Ok
            | AddBooking f ->
                fun _ -> 
                    [
                        {
                            id = Guid.NewGuid()
                            event = fun (x: State) -> x.AddBooking f
                        }
                    ] 
                    |> NonEmptyList.ofList 
                    |> Ok




// open Domain
// module Fold =   
//     let initial = State.GetEmpty()
//     let evolve (s: State) e = 
//         let (Result.Ok res) = s.ProcessSEvents ([e] |> NonEmptyList.ofList)
//         res

//     let fold: State -> List<SEvent> -> State =
//         Seq.fold evolve

//     let interpret c (s: State) =
//         let (Ok res) = s.ProcessSCommand c
//         res


// open Domain
// open Fold
// type Service internal (resolve : string -> Equinox.Decider<Domain.SEvent, State>) =
//     let handle clientId sCommand =
//         let stream = resolve clientId

//         stream.Transact
//             (
//                 fun state ->
//                     let events = interpret sCommand state 
//                     let newState = Fold.fold state (events |> NonEmptyList.toList)
//                     newState, (events |> NonEmptyList.toList)
//             )

    