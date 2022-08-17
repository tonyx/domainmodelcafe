namespace hotelmodeling
open System
open FSharp.Core
open FSharpPlus
open FSharpPlus.Data

module MiscUtils =
    let daysToString(days: Set<DateTime>) = 
        let printconflictsdays = 
            days 
                |> Set.fold (fun x y -> (sprintf "%A" (y.ToString("yyy/MM/dd")))+"," + x) ""
        if (printconflictsdays = String.Empty) then
            String.Empty
        else
            printconflictsdays.Substring(0, printconflictsdays.Length-1)

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
            expectedCheckin: DateTime
            expectedCheckout: DateTime
        }
        with 
            member this.getDaysInterval() =
                let spanInterval = this.expectedCheckout - this.expectedCheckin
                let days = spanInterval.Days
                [
                    for i = 1 to days do 
                        yield (this.expectedCheckin.Date + TimeSpan(i - 1, 0, 0, 0))
                ]

    type Event = Hotel -> Result<Hotel, string>
    type Command = Hotel -> Result<NonEmptyList<Event>, string>
    type Hotel =
        {
            rooms: List<Room>
            bookings: List<Booking>
        }
        with 
            static member GetEmpty() =
                {
                    rooms = []
                    bookings = []
                }
            member this.AddRoom (room: Room): Result<Hotel, string> =
                if ((this.rooms) |> List.contains room) then   
                    sprintf "a room with number %d already exists" room.id |> Error
                else 
                    {
                        this with   
                            rooms = room::this.rooms
                    } 
                    |> Ok
            member this.AddBooking (booking: Booking): Result<Hotel, string> =
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
                        } 
                        |> Ok
            member this.GetBookedDaysOfRoom roomId =
                let roomBookings = 
                    this.bookings 
                    |> List.filter (fun x -> x.roomId = roomId)                    
                let daysOfBookings = 
                    roomBookings 
                    |> List.map (fun x -> x.getDaysInterval())
                    |> List.fold (@) []
                    |> Set.ofList 

                daysOfBookings

            member this.ProcessEvents (events: NonEmptyList<Event>) =
                events |> NonEmptyList.toList
                |> List.fold 
                    (fun x f -> 
                        match x with
                        | Ok x1 -> f x1
                        | Error x -> Error x
                    ) (this |> Ok)

            member this.ProcessCommand (command: Command) =
                let res = 
                    match command this with
                    | Ok x -> 
                        match this.ProcessEvents x with
                        | Ok _ -> Ok x
                        | Error e -> Error (sprintf "command error: %s" e)
                    | Error x ->  Error (sprintf "command error: %s" x)
                res
    type CommandMaker =
        | AddRoom of Room
        | AddBooking of Booking

    let getEvent x  (f: 'a -> Result<'a, string>) =
        let event x =
            f x
        event 

    let makeCommand commandMaker: Command =
        match commandMaker with
            | AddRoom t ->
                let addRoom: Event = getEvent t (fun x -> x.AddRoom t)
                fun _ -> [addRoom] |> NonEmptyList.ofList |> Ok
            | AddBooking f ->
                let addBooking: Event = getEvent f (fun x -> x.AddBooking f)
                fun _ -> [addBooking] |> NonEmptyList.ofList |> Ok