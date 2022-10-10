namespace hotelmodeling

open hotelmodeling
open hotelmodeling.Domain
open hotelmodeling.CommandEvents
open hotelmodeling.HotelSerialization
open hotelmodeling.MiscUtils
open FSharp.Core
open hotelmodeling.App
open FSharp.Core.Result
open Microsoft.FSharp.Core.Result
open Microsoft.FSharp.Core
open FSharp
open System
open Expecto
open FSharpPlus
open FSharpPlus.Data
open Npgsql.FSharp

module DbTests =
    open Db

    let deleteAllevents () =
        let _ =
            TPConnectionString 
            |> Sql.connect
            |> Sql.query "DELETE from events"
            |> Sql.executeNonQuery
        ()

    // [<Tests>]
    // let AppTests =
    //     testList
    //         "domain objects tests"
    //         [ 
    //             testCase "Setup"
    //                 <| fun _ ->
    //                     let _ = deleteAllevents()
    //                     Expect.isTrue true "asdftrue"
    //             testCase "when db is empty the last snapshot is the init"
    //                 <| fun _ ->
    //                     let (Ok state) = getLatestSnapshot()
    //                     Expect.equal state (Hotel.GetEmpty()) "shoule be equal"
    //             testCase "add a 'first' event in the db and so the last snapshot is the init-state + event"
    //                 <| fun _ ->
    //                     // prepare
    //                     let _ = deleteAllevents()
    //                     let ctx = Db.getContext()
    //                     let room1 = {
    //                         id = 1
    //                         description = None
    //                     } 
    //                     let roomAdded = room1 |> Event.RoomAdded 
    //                     let sRoomAdded = roomAdded.Serialize()
    //                     let hotel' = Hotel.GetEmpty().Evolve [roomAdded] |> Result.get
    //                     let serializedState = hotel'.Serialize()

    //                     // act
    //                     let _ = Db.addEventWithSnapshot sRoomAdded serializedState ctx 

    //                     // assert
    //                     let (id, dbSnapshot) = (Db.getLatestSnapshotWithId ctx).Value
    //                     Expect.equal serializedState dbSnapshot "should be equal"

    //         ] 
    //         |> testSequenced
        
    
