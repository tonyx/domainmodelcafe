
module hotelmodeling.App
open hotelmodeling.Domain
open hotelmodeling.HotelSerialization
open hotelmodeling.CommandEvents
open FSharpPlus
open MiscUtils
open FSharpPlus.Extensions
open FSharp.Data.Sql
open System
open System.Data
open System.Globalization
open System.Data.Common
open Utils
open System.Threading
open System.Runtime.CompilerServices
open System.Threading;
open Newtonsoft.Json

let ceError = CeErrorBuilder()
let initState = Hotel.GetEmpty()

let getLastSnapshot() =
    let ctx = Db.getContext()
    let (id, json) = Db.getLastSnapshot ctx 
    ceError {
        let! state = json |> Hotel.Deserialize
        return (id, state)
    }

let getState() =
    ceError {
        let ctx = Db.getContext()
        let! (id, state) = getLastSnapshot()
        let jsonEvents = Db.getEventsAfterId id ctx 
        let lastId =
            match jsonEvents.Length with
            | x when x > 0 -> jsonEvents |> List.last |> fst
            | _ -> id
        let! result = jsonEvents |>> snd |> state.Evolve
        return (lastId, result)
    }

let mkRoom id description: Room =
    {
        id = id
        description = 
            match description with
            | "" -> None
            | x -> x |> Some
    }

[<MethodImpl(MethodImplOptions.Synchronized)>]
let addRoom roomId description =
    ceError {
        let room = mkRoom roomId description
        let addCommand =
            Command.AddRoom room
        let! (_, state) = getState() 
        let! events = 
            state 
            |> addCommand.Execute 
        let ctx = Db.getContext()
        let! _ =
            ctx |> Db.addEvents (events |>> Event.Serialize)
        return sprintf "room %d has been Added" roomId
    }

let mkBooking roomId email checkin checkout: Booking =
    {
        id = None
        roomId = roomId
        customerEmail = email
        plannedCheckin = checkin
        plannedCheckout = checkout
    }

[<MethodImpl(MethodImplOptions.Synchronized)>]
let addBooking (roomId: int) (email: string) (checkin: DateTime) (checkout: DateTime) =
    let booking = mkBooking roomId email checkin checkout  
    let addCommand =
        Command.AddBooking booking
    ceError {
        let ctx = Db.getContext()
        let! (_, state) = getState()
        let! events =
            state 
            |> addCommand.Execute
        let! _ =
            ctx |> Db.addEvents (events |>> Event.Serialize)
        return sprintf "booking %s added" (booking.ToString())
    }
    
let mkSnapshot() = 
    ceError 
        {
            let ctx = Db.getContext()
            let! (id, state) = getState()
            let snapshot = state.Serialize()
            let! result =  Db.setSnapshot id snapshot ctx
            return result
        }

