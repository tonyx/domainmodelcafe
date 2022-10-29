namespace hotelmodeling

open hotelmodeling
open hotelmodeling.Domain
open hotelmodeling.App
open System
open FSharp.Core
open Expecto
open FSharpPlus
open FSharpPlus.Data

module AppTests =
    [<Tests>]
    let AppTests =
        testList
            "application level/controller tests, hotel and room tests"
            [ 
                testCase "add a new room command and get the state that will have a room" 
                    <| fun _ ->
                        let _ = DbTests.deleteAllevents()
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
                        let result = App.getState()
                        Expect.isOk result "should be ok"
                        let (_, actual) = App.getState() |> Result.get
                        Expect.equal actual expected "should be true"

                testCase "add a new room command and get the state. the last snapshot is not the state without calling mksnapshot" 
                    <| fun _ ->
                        let _ = DbTests.deleteAllevents()
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
                        let (_, actual) = App.getState() |> Result.get
                        let (_, lastSnapshot) = App.getLastSnapshot() |> Result.get
                        Expect.notEqual lastSnapshot expected "should be not equal"
                        Expect.equal actual expected "should be true"

                testCase "add a new room command and get the state. the last snapshot is the same as the state if I call mksnapshot" 
                    <| fun _ ->
                        let _ = DbTests.deleteAllevents()
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
                        let (_, actual) = App.getState() |> Result.get
                        let (_, lastSnapshot) = App.getLastSnapshot() |> Result.get
                        Expect.notEqual lastSnapshot expected "should be not equal"
                        // Expect.notEqual id1 id2 "should be different"
                        let result = mkSnapshot() 
                        Expect.isOk result "shold be ok"
                        let (_, lastSnapshot) = App.getLastSnapshot() |> Result.get

                        // Expect.equal id1 id3 "should be equal"
                        Expect.equal lastSnapshot expected "should be not equal"
                        Expect.equal actual expected "should be true"

                testCase "add rooms commands and get the state that will have two rooms - OK" 
                    <| fun _ ->

                        let _ = DbTests.deleteAllevents()
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
                        let (_, actual) = App.getState() |> Result.get
                        Expect.equal actual expected "should be true"

                testCase "add two rooms commands and make a snapshot of the current state - OK" 
                    <| fun _ ->
                        // prepare
                        let _ = DbTests.deleteAllevents()

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
                        let (id, actual) = App.getState() |> Result.get

                        let (Ok _)  = App.mkSnapshot() // id actual
                        let (_, lastSnapshot) = 
                            App.getLastSnapshot() |> Result.get
                        Expect.equal lastSnapshot expected "should be equal"
                        Expect.equal actual expected "should be equal"

                testCase "add already existing rooms command error - ok" 
                    <| fun _ ->
                        let _ = DbTests.deleteAllevents()
                        let result = App.addRoom 1 ""
                        Expect.isOk result "should be ok"
                        let result2 = App.addRoom 1 "description"
                        Expect.isError result2 "should be error"
                        let (Error x) = result2
                        Expect.equal x "a room with number 1 already exists" "should be equal"

                testCase "add a room and a booking, then get state - ok" 
                    <| fun _ ->
                        let _ = DbTests.deleteAllevents()
                        let result = App.addRoom 1 ""
                        Expect.isOk result "should be ok"
                        let result2 = App.addBooking 1 "customer@anyemail.it" (DateTime.Parse("2022-01-01")) (DateTime.Parse("2022-01-02"))
                        Expect.isOk result2 "should be ok"
                        let (_, state) = App.getState() |> Result.get
                        let expectedRooms: List<Room> = 
                            [
                                {
                                    id = 1
                                    description = None
                                }
                            ]
                        Expect.equal (state.rooms) expectedRooms "should be equal"
                        Expect.equal state.rooms.Head.id 1 "should be equal"
                        let actualbookins = state.bookings
                        Expect.equal actualbookins.Head.plannedCheckin (DateTime.Parse("2022-01-01")) "should be equal"
                        Expect.equal actualbookins.Head.plannedCheckout (DateTime.Parse("2022-01-02")) "should be equal"

                testCase "add a room and two overlapping booking, - Error" 
                    <| fun _ ->
                        let _ = DbTests.deleteAllevents()
                        let result = App.addRoom 1 ""
                        Expect.isOk result "should be ok"
                        let result2 = App.addBooking 1 "customer@anyemail.it" (DateTime.Parse("2022-01-01")) (DateTime.Parse("2022-01-05"))
                        Expect.isOk result2 "should be ok"
                        let result3  = App.addBooking 1 "customer@anyemail.it" (DateTime.Parse("2022-01-04")) (DateTime.Parse("2022-01-10"))
                        Expect.isError result3 "should be error"
                        let (Error err) = result3
                        Expect.equal err "overlap: \"2022/01/04\"" "should be equal"
            ] 
            |> testSequenced

        


