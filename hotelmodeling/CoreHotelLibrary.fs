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

    let catchErrors f l =
        let (okList, errors) =
            l  
            |> List.map f 
            |> Result.partition
        if (errors.Length > 0) then
            Result.Error (errors.Head)
        else
            okList |> Result.Ok

module Domain =
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
                                // id = this.id + 1
                        } 
                        |> Ok
            member this.GetBookedDaysOfRoom roomId =
                    this.bookings 
                    |> List.filter (fun x -> x.roomId = roomId)                    
                    |> List.map (fun x -> x.getDaysInterval())
                    |> List.fold (@) []
                    |> Set.ofList 

