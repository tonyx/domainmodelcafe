
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

let ceError = CeErrorBuilder()

let initState = Hotel.GetEmpty()

// this will translate the call from the web app to commands event store
let getLatestSnapshot() =
    let ctx = Db.getContext()
    let idWithJson = Db.getLatestSnapshotWithId(ctx)
    match idWithJson with
        | Some (id, json) -> Hotel.Deserialize json
        | _ -> Hotel.GetEmpty() |> Result.Ok

let getState() =
    ceError {
        let ctx = Db.getContext()
        let idSnapshot = Db.getLatestSnapshotWithId ctx
        let! (id, state) =
            match idSnapshot with
            | None -> (0, initState) |> Ok
            | Some (id, jsonState) ->
                let state = jsonState |> Hotel.Deserialize |> Result.get
                (id, state) |> Ok
        let jsonEvents = Db.getEventsAfterId id ctx 
        let events = 
            jsonEvents 
            |>> 
                (fun 
                    (_, x) -> 
                        Event.Deserialize x 
                        |> Result.get
                ) 
        let result = events |> state.Evolve |> Result.get
        return result
    }

let getIdAndLatestSnapshot() =
    let ctx = Db.getContext()
    let idWithJson = Db.getLatestSnapshotWithId(ctx)
    match idWithJson with
        | Some (id, json) -> 
            match Hotel.Deserialize json with
            | Result.Error x -> Result.Error x
            | Result.Ok x ->  (id, x) |> Result.Ok
        | _ -> (0, Hotel.GetEmpty()) |> Result.Ok


let mkRoom id description: Room =
            {
                id = id
                description = 
                    match description with
                    | "" -> None
                    | x -> x |> Some
            }

let addRoom roomId description =
    ceError {
        let room = mkRoom roomId description
        let addCommand =
            Command.AddRoom room
        let state = getState() |> Result.get
        let! events = 
            state 
            |> addCommand.Execute 
        let ctx = Db.getContext()
        let! _ = 
            events 
            |> catchErrors (fun x -> Db.addEvent (x.Serialize()) ctx)
        return sprintf "room %d has been Added" roomId
    }

let getEventsAfterId(id: int) =
    ()
