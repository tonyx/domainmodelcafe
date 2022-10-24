
namespace hotelmodeling
open System
open FSharp.Core
open FSharpPlus
open FSharpPlus.Data
open Equinox.EventStore

open hotelmodeling.Domain
open hotelmodeling.MiscUtils

module CommandEvents =
    type Event =
        | RoomAdded of Room
        | BookingAdded of Booking
    type Command =
        | AddRoom of Room
        | AddBooking of Booking
    type Event with
        member this.Process (x: Hotel) =
            match this with
            | RoomAdded r -> x.AddRoom r
            | BookingAdded b -> x.AddBooking b

    type Command with
        member this.Execute (x: Hotel) =
            match this with
            | AddRoom r -> 
                match x.AddRoom r with 
                    | Ok _ -> [Event.RoomAdded r] |> Ok
                    | Error x ->  x |> Error
            | AddBooking b ->
                match x.AddBooking b with
                    | Ok _ -> [Event.BookingAdded b] |> Ok
                    | Error x ->  x |> Error    

        static member Executes (l: List<Command>) (h: Hotel) =
            let res =
                l |> catchErrors (fun (c: Command) -> h |> c.Execute)
            match res with
                | Error x -> Error x
                | Ok x -> x |> List.fold (@) [] |> Ok

    type Hotel with
        member this.Evolve (events: List<Event>) =
            events 
            |> List.fold 
                (fun (acc: Result<Hotel, string>) (x: Event) -> 
                    match acc with  
                        | Error _ -> acc
                        | Ok y -> x.Process y
                ) (this |> Ok)
    let initHotel: Hotel = Hotel.GetEmpty()
