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

    // type Event = State -> Result<State, string>
    // type Command = State -> Result<NonEmptyList<Event>, string>

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

            member this.Evolve (events: List<Event>) =
                events 
                |> List.fold 
                    (fun (acc: Result<State, string>) (x: Event) -> 
                        match acc with  
                            | Error _ -> acc
                            | Ok y -> x.Process y
                    ) (this |> Ok)

    type Event =
        | RoomAdded of Room
        | BookingAdded of Booking
        member this.Process (x: State) =
            match this with
            | RoomAdded r -> x.AddRoom r
            | BookingAdded b -> x.AddBooking b

    type Command =
        | AddRoom of Room
        | AddBooking of Booking
        member this.Execute (x: State) =
            match this with
            | AddRoom r -> 
                match x.AddRoom r with 
                    | Ok _ -> [Event.RoomAdded r] |> Ok
                    | Error x ->  x |> Error
            | AddBooking b ->
                match x.AddBooking b with
                    | Ok _ -> [Event.BookingAdded b] |> Ok
                    | Error x ->  x |> Error

    let initState: State = State.GetEmpty()
