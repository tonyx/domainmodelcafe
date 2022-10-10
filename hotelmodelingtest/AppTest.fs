
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

module AppTests =
    open Db
    open DbTests
    [<Tests>]
    let AppTests =
        testList
            "application level/controller tests"
            [ 
                testCase "db empty, state is emtpy"
                    <| fun _ ->
                        let _ = hotelmodeling.DbTests.deleteAllevents()
                        let (Ok state) = App.getLatestSnapshot()
                        Expect.equal state (Hotel.GetEmpty()) "should be equal"

                testCase "add a new room command and get the state that will have a room" 
                    <| fun _ ->
                        let _ = hotelmodeling.DbTests.deleteAllevents()
                        let result = App.addRoom 1 ""
                        Expect.isOk result "should be ok"
                        let room = 
                            {
                                id = 1
                                description = None
                            }
                        let expected =
                            {
                                Hotel.GetEmpty() with   
                                    rooms = [room]
                            }
                        let actual = App.getState() |> Result.get
                        Expect.equal actual expected "should be true"

                testCase "add rooms commands and get the state that will have two rooms - OK" 
                    <| fun _ ->
                        let _ = hotelmodeling.DbTests.deleteAllevents()
                        let result = App.addRoom 1 ""
                        Expect.isOk result "should be ok"
                        let result2 = App.addRoom 2 ""
                        Expect.isOk result2 "should be ok"
                        let room1 = 
                            {
                                id = 1
                                description = None
                            }
                        let room2 = 
                            {
                                id = 2
                                description = None
                            }
                        let expected =
                            {
                                Hotel.GetEmpty() with   
                                    rooms = [room2; room1]
                            }
                        let actual = App.getState() |> Result.get
                        Expect.equal actual expected "should be true"

                testCase "add two rooms commands and make a snapshot of the current state - OK" 
                    <| fun _ ->
                        // prepare
                        let _ = hotelmodeling.DbTests.deleteAllevents()

                        // act
                        let result = App.addRoom 1 ""
                        Expect.isOk result "should be ok"
                        let result2 = App.addRoom 2 ""
                        Expect.isOk result2 "should be ok"
                        // assert
                        let room1 = 
                            {
                                id = 1
                                description = None
                            }
                        let room2 = 
                            {
                                id = 2
                                description = None
                            }
                        let expected =
                            {
                                Hotel.GetEmpty() with   
                                    rooms = [room2; room1]
                            }
                        let actual = App.getState() |> Result.get
                        Expect.equal actual expected "should be true"

                testCase "add already existing rooms command error - ok" 
                    <| fun _ ->
                        let _ = hotelmodeling.DbTests.deleteAllevents()
                        let result = App.addRoom 1 ""
                        Expect.isOk result "should be ok"
                        let result2 = App.addRoom 1 "description"
                        Expect.isError result2 "should be error"
                        let (Error x) = result2
                        Expect.equal x "a room with number 1 already exists" "should be equal"

                testCase "when db is empty then the last snapshot is the init"
                    <| fun _ ->
                        // prepare
                        let _ = hotelmodeling.DbTests.deleteAllevents()
                        
                        // act
                        let (Ok state) = getLatestSnapshot()

                        // assert
                        Expect.equal state (Hotel.GetEmpty()) "shoule be equal"
                testCase "add a 'first' event in the db"
                    <| fun _ ->
                        // prepare
                        let _ = deleteAllevents()
                        let ctx = Db.getContext()
                        let room1 = {
                            id = 1
                            description = None
                        } 
                        let roomAdded = room1 |> Event.RoomAdded 
                        let sRoomAdded = roomAdded.Serialize()
                        let hotel' = Hotel.GetEmpty().Evolve [roomAdded] |> Result.get
                        let serializedState = hotel'.Serialize()

                        // act
                        let _ = Db.addEventWithSnapshot sRoomAdded serializedState ctx 

                        // assert
                        let (id, dbSnapshot) = (Db.getLatestSnapshotWithId ctx).Value
                        Expect.equal serializedState dbSnapshot "should be equal"

            ] 
            |> testSequenced
        


