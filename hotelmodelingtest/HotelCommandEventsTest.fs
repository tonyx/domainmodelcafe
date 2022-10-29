
namespace hotelmodeling

open Expecto
open hotelmodeling.Domain
open hotelmodeling.CommandEvents
open hotelmodeling.MiscUtils
open System
open FSharp.Core
open FSharpPlus
open FSharpPlus.Data
open Utils
open App

module EventssTests =
    let room1 = {
        id = 1
        description = None
    }

    let roomAdded r =
        fun (x: Hotel) -> x.AddRoom r
    let bookingAdded b =
        fun (x: Hotel) -> x.AddBooking b

    let sameBooking (booking1: Booking) (booking2: Booking) =
        let b1  =
            {
                booking1 with id = None
            } 
        let b2 = 
            {   
                booking2 with id = None
            }
        b1 = b2

    let mkRoom id =
        {
            id = id
            description = None
        }

    let mkBooking roomId plannedCheckin plannedCheckout =
        {
            id = None
            roomId = roomId 
            customerEmail = "email@me.com"
            plannedCheckin = plannedCheckin 
            plannedCheckout = plannedCheckout 
        }
    [<Tests>]
    let eventsTests =
        testList "Domain events" [
            testCase "add room event  - Ok" <| fun _ ->
                let hotel = Hotel.GetEmpty()
                let uEvent = Event.RoomAdded room1
                let result = hotel |> uEvent.Process
                Expect.isOk result "should be ok"
                let (Ok hotel') = result
                let expected = 
                    {
                        rooms = [room1]
                        bookings = []
                    }
                Expect.equal hotel' expected "should be equal"

            testCase "add booking simple event - Ok" <| fun _ ->
                let booking = mkBooking 1 (DateTime.Parse("2022-11-11 01:01:01")) (DateTime.Parse("2022-11-12 01:01:01"))
                let hotel = 
                    {
                        Hotel.GetEmpty() with
                            rooms = [room1]
                    }
                let event = bookingAdded booking
                let result = event hotel
                Expect.isOk result "should be ok"

                let result = event hotel
                Expect.isOk result "should be ok"
                let hotel' = result |> Result.get
                Expect.isSome hotel'.bookings.Head.id "should be some"

            testCase "add booking event - Ok" <| fun _ ->
                let booking = mkBooking 1 (DateTime.Parse("2022-11-11 01:01:01")) (DateTime.Parse("2022-11-12 01:01:01"))
                let hotel = 
                    {
                        Hotel.GetEmpty() with
                            rooms = [room1]
                    }
                let uEvent = Event.BookingAdded booking
                let result =  hotel |> uEvent.Process
                Expect.isOk result "should be ok"
                let hotel' = result |> Result.get
                Expect.isSome hotel'.bookings.Head.id "should be some"

            testCase "add two rooms - Ok" <| fun _ ->
                let room2 = mkRoom 2
                let hotel = Hotel.GetEmpty()
                let uRoom1Added = Event.RoomAdded room1
                let uRoom2Added = Event.RoomAdded room2
                let uEvents = [uRoom1Added; uRoom2Added]

                let result =  uEvents |> hotel.Evolve
                Expect.isOk result "should be ok"
                let hotel' = result |> Result.get
                Expect.equal hotel' {hotel with rooms = [room2; room1]} "should be equal"

            testCase "add a room and a booking - Ok" <| fun _ ->
                let booking = mkBooking 1 (DateTime.Parse("2022-11-11 01:01:01")) (DateTime.Parse("2022-11-12 01:01:01"))
                let hotel = Hotel.GetEmpty()
                let room1Added = Event.RoomAdded room1
                let booking1Added = Event.BookingAdded booking
                let events = [room1Added; booking1Added] 

                let result = events |> hotel.Evolve
                let hotel' = result |> Result.get

                let actualBookingNoId =
                    {
                        hotel'.bookings.Head 
                            with 
                                id = None
                    }
                Expect.equal (hotel'.rooms.Head) room1 "should be equal"
                Expect.equal actualBookingNoId booking "should be equal"

            testCase "add a room and a booking UNION EVENT - Ok" <| fun _ ->
                let booking = mkBooking 1 (DateTime.Parse("2022-11-11 01:01:01")) (DateTime.Parse("2022-11-12 01:01:01"))
                let hotel = Hotel.GetEmpty()
                let room1Added = Event.RoomAdded room1 
                let booking1Added = Event.BookingAdded booking
                let events = [room1Added; booking1Added]
                let result =  events |> hotel.Evolve
                Expect.isOk result "should be ok"
                let hotel' = result |> Result.get
                let actualBookingNoId =
                    {
                        hotel'.bookings.Head 
                            with 
                                id = None
                    }
                Expect.equal (hotel'.rooms.Length) 1 "should be equal"
                Expect.equal (hotel'.rooms.Head) room1 "should be equal"
                Expect.equal actualBookingNoId booking "should be equal"

            testCase "add already existing room - Error" <| fun _ ->
                let hotel = 
                    {
                        Hotel.GetEmpty()
                            with rooms = [room1]
                    }
                let room1Added = Event.RoomAdded room1
                let events = [room1Added] 
                let result =  events |> hotel.Evolve
                Expect.isError result "should be error"
                let (Error error) = result
                Expect.equal error "a room with number 1 already exists" "should be equal"

            testCase "add overlapping booking - Error" <| fun _ -> 
                let booking = 
                    {
                        mkBooking 1 (DateTime.Parse("2022-11-11 00:00:00")) (DateTime.Parse("2022-11-12 00:00:00"))
                        with id = Guid.Parse("d45c0760-dbf7-4453-a15f-b4cb1b78c730") |> Some
                    }

                let hotel = 
                    {
                        Hotel.GetEmpty()
                        with
                            rooms = [room1]
                            bookings = [booking]
                    }
                let booking1 = 
                    {
                        booking with
                            id = None
                    }
                let bookingAdded = Event.BookingAdded booking1
                let result =  [bookingAdded] |> hotel.Evolve
                let (Error result') = result
                Expect.equal result' "overlap: \"2022/11/11\"" "should be equal"

            testCase "add booking on free period - Ok" <| fun _ -> 
                let booking = 
                    {
                        (mkBooking 1 (DateTime.Parse("2022-11-11 00:00:00")) (DateTime.Parse("2022-11-12 00:00:00")))
                        with id = Guid.Parse("d45c0760-dbf7-4453-a15f-b4cb1b78c730") |> Some
                    }
                let hotel = 
                    {
                        Hotel.GetEmpty()
                        with
                            rooms = [room1]
                            bookings = [booking]
                    }
                let booking1 = mkBooking 1 (DateTime.Parse("2022-11-12 00:00:00")) (DateTime.Parse("2022-11-20 00:00:00"))
                let booking1Added = Event.BookingAdded booking1
                let events = [booking1Added] 
                let (Ok hotel') = events |> hotel.Evolve
                Expect.equal (hotel'.bookings.Length) 2 "should be equal"
        ]

    [<Tests>]
    let CommandTests =
        testList "Commands returning event lists" [
            testCase "addRoom command returns roomAdded event - Ok" <| fun _ ->
                let hotel = Hotel.GetEmpty()
                let command = Command.AddRoom room1
                let expected = [Event.RoomAdded room1] |> Ok
                let actual = command.Execute hotel
                Expect.equal actual expected "should be equal"

            testCase "addBooking command returns bookingAdded event - Ok" <| fun _ ->
                let hotel = Hotel.GetEmpty().AddRoom room1 |> Result.get 
                let booking = mkBooking 1 (DateTime.Parse("2022-11-11 00:00:00")) (DateTime.Parse("2022-11-12 00:00:00"))
                let command = Command.AddBooking booking
                let expected = [Event.BookingAdded booking] |> Ok
                let actual = command.Execute hotel
                Expect.equal actual expected "should be equal"

            testCase "addBooking command returns Error in booking unxisting room - KO" <| fun _ ->
                let hotel = Hotel.GetEmpty().AddRoom room1 |>  Result.get 
                let booking = mkBooking 666 (DateTime.Parse("2022-11-11 00:00:00")) (DateTime.Parse("2022-11-12 00:00:00"))
                let command = Command.AddBooking booking
                let result = command.Execute hotel 
                Expect.isError result "should be error"
                let (Error actual) = result
                Expect.equal actual "room 666 doesn't exist" "should be equal"
        ]
