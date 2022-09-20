namespace hostelmodeling

open Expecto
open hotelmodeling.Domain
open hotelmodeling.MiscUtils
open System
open FSharp.Core
open FSharpPlus
open FSharpPlus.Data

module Tests =

    let room1 = {
        id = 1
        description = None
    }
    let roomAdded r =
        fun (x: State) -> x.AddRoom r
    let bookingAdded b =
        fun (x: State) -> x.AddBooking b

    [<Tests>]
    let domainObjectTests =
        testList "Domain objects" [
            testCase "rooms with the same id are the same room" <| fun _ ->
                let room1WithDescription = {
                    id = 1
                    description = "nice view" |> Some
                }
                Expect.equal room1 room1WithDescription "should be equal"
                
            testCase "rooms with different ids are the different room" <| fun _ ->
                let room2 = {
                    id = 2
                    description = None
                }
                Expect.notEqual room1 room2 "should be equal"

            testCase "two bookings having all fields equals are equals - Ok" <| fun _ ->
                let booking1 = 
                    {
                        id = Guid.Parse("d45c0760-dbf7-4453-a15f-b4cb1b78c730") |> Some
                        roomId = 1
                        customerEmail = "me@you.us"
                        plannedCheckin = DateTime.Parse("2022-11-11 00:00:00")
                        plannedCheckout = DateTime.Parse("2022-11-12 00:00:00")
                    }
                let booking2 = 
                    {
                        id = Guid.Parse("d45c0760-dbf7-4453-a15f-b4cb1b78c730") |> Some
                        roomId = 1
                        customerEmail = "me@you.us"
                        plannedCheckin = DateTime.Parse("2022-11-11 00:00:00")
                        plannedCheckout = DateTime.Parse("2022-11-12 00:00:00")
                    }
                Expect.equal booking1 booking2 "should be equal"

            testCase "two bookings with different roomid are not equal" <| fun _ ->
                let booking1 = 
                    {
                        id = Guid.Parse("d45c0760-dbf7-4453-a15f-b4cb1b78c730") |> Some
                        roomId = 666 
                        customerEmail = "me@you.us"
                        plannedCheckin = DateTime.Parse("2022-11-11 00:00:00")
                        plannedCheckout = DateTime.Parse("2022-11-12 00:00:00")
                    }
                let booking2 = 
                    {
                        id = Guid.Parse("d45c0760-dbf7-4453-a15f-b4cb1b78c730") |> Some
                        roomId = 1
                        customerEmail = "me@you.us"
                        plannedCheckin = DateTime.Parse("2022-11-11 00:00:00")
                        plannedCheckout = DateTime.Parse("2022-11-12 00:00:00")
                    }
                Expect.notEqual booking1 booking2 "should be equal"

            testCase "two bookings with different day of checkin/out, are not equal" <| fun _ ->
                let booking1 = 
                    {
                        id = Guid.Parse("d45c0760-dbf7-4453-a15f-b4cb1b78c730") |> Some
                        roomId = 1
                        customerEmail = "me@you.us"
                        plannedCheckin = DateTime.Parse("2022-11-11 00:00:00")
                        plannedCheckout = DateTime.Parse("2022-11-12 00:00:00")
                    }
                let booking2 = 
                    {
                        id = Guid.Parse("d45c0760-dbf7-4453-a15f-b4cb1b78c730") |> Some
                        roomId = 1
                        customerEmail = "me@you.us"
                        plannedCheckin = DateTime.Parse("2022-11-11 01:01:01")
                        plannedCheckout = DateTime.Parse("2022-11-13 02:02:02")
                    }
                Expect.notEqual booking1 booking2 "should be not equal"

            testCase "the busy day of a one day night is only the checkin day" <| fun _ ->
                let booking = 
                    {
                        id = Guid.Parse("d45c0760-dbf7-4453-a15f-b4cb1b78c730") |> Some
                        roomId = 1
                        customerEmail = "me@you.us"
                        plannedCheckin = DateTime.Parse("2022-11-11 10:10:10")
                        plannedCheckout = DateTime.Parse("2022-11-12 11:11:11")
                    }
                let busyDays = booking.getDaysInterval()
                let expected = [(DateTime.Parse("2022-11-11 10:10:10").Date)]
                Expect.equal busyDays expected "should be equal"

            testCase "the busy day of a two days night will be the checkin day and the one after" <| fun _ ->
                let booking = 
                    {
                        id = Guid.Parse("d45c0760-dbf7-4453-a15f-b4cb1b78c730") |> Some
                        roomId = 1
                        customerEmail = "me@you.us"
                        plannedCheckin = DateTime.Parse("2022-11-11 10:10:10")
                        plannedCheckout = DateTime.Parse("2022-11-13 11:11:11")
                    }
                let busyDays = booking.getDaysInterval()
                let expected = 
                    [
                        (DateTime.Parse("2022-11-11 10:10:10").Date) 
                        (DateTime.Parse("2022-11-12 10:10:10").Date) 
                    ]
                Expect.equal busyDays expected "should be equal"
        ]
    [<Tests>]
    let domainModelTests =
        testList "Domain model logic" [
            testCase "add a room to an empty hotel - Ok" <| fun _ ->
                let hotel = State.GetEmpty()
                let expected = 
                    {
                        hotel with
                            rooms = [room1]
                            id = hotel.id + 1
                    } 
                    |> Ok
                let actual = hotel.AddRoom room1
                Expect.equal expected actual "shold be equal"

            testCase "add a room when a room with the same id already exists in the hotel - Error" <| fun _ ->
                let room1b =
                    {
                        id = 1
                        description = "I have the same id of room1 by mistake" |> Some
                    } 
                let hotel = 
                    {
                        State.GetEmpty()
                            with
                                rooms = [room1]
                    }
                let (Error actual) = hotel.AddRoom room1b
                Expect.equal actual "a room with number 1 already exists" "shold be true"

            testCase "create booking about a room that does exists - Ok" <| fun _ ->
                let hotel = 
                    {
                        State.GetEmpty()
                            with
                                rooms = [room1]
                    }
                let booking: Booking =
                    {
                        id = None
                        roomId = 1
                        customerEmail = "email@me.com"
                        plannedCheckin = DateTime.Parse("2022-11-11 01:01:01")
                        plannedCheckout = DateTime.Parse("2022-11-12 01:01:01")
                    }
                let (Ok hotelWithBooking) = hotel.AddBooking booking
                let actualBooking = 
                    {
                        hotelWithBooking.bookings.Head with
                            id = None
                    }        
                Expect.equal actualBooking booking "should be equal"

            testCase "create a booking about a room that doesn't exist - Error" <| fun _ ->
                let hotel = 
                    {
                        State.GetEmpty()
                            with
                                rooms = [room1]
                    }
                let booking: Booking =
                    {
                        id = None
                        roomId = 666
                        customerEmail = "email@me.com"
                        plannedCheckin = DateTime.Parse("2022-11-11 01:01:01")
                        plannedCheckout = DateTime.Parse("2022-11-12 01:01:01")
                    }
                let (Error error) = hotel.AddBooking booking
                Expect.equal error "room 666 doesn't exist" "should be equal"

            testCase "after creating a booking, then the booking must have an id - Ok" <| fun _ ->
                let hotel = 
                    {
                        State.GetEmpty()
                            with
                                rooms = [room1]
                    }
                let booking: Booking =
                    {
                        id = None
                        roomId = 1
                        customerEmail = "email@me.com"
                        plannedCheckin= DateTime.Parse("2022-11-11 01:01:01")
                        plannedCheckout = DateTime.Parse("2022-11-12 01:01:01")
                    }
                let (Ok hotel') = hotel.AddBooking booking
                Expect.isSome hotel'.bookings.Head.id  "should be some"

            testCase "after creating a booking, then the busy days of the booking are the days in the checkin checkout interval (excluding checkout day) - Ok" <| fun _ ->
                let hotel = 
                    {
                        State.GetEmpty()
                            with
                                rooms = [room1]
                    }
                let booking: Booking =
                    {
                        id = None
                        roomId = 1
                        customerEmail = "email@me.com"
                        plannedCheckin= DateTime.Parse("2022-11-11 01:01:01")
                        plannedCheckout = DateTime.Parse("2022-11-12 01:01:01")
                    }
                let (Ok hotel') = hotel.AddBooking booking
                Expect.isSome hotel'.bookings.Head.id  "should be some"
                let bookedDays = hotel'.GetBookedDaysOfRoom 1
                Expect.equal bookedDays ([DateTime.Parse("2022-11-11 00:00:00")] |> Set.ofList) "should be true"

            testCase "larger booking interval. Get the booked days. From checkin date to checkout date excluded - Ok" <| fun _ ->
                let hotel = 
                    {
                        State.GetEmpty()
                            with
                                rooms = [room1]
                    }
                let booking: Booking =
                    {
                        id = None
                        roomId = 1
                        customerEmail = "email@me.com"
                        plannedCheckin = DateTime.Parse("2022-11-11 01:01:01")
                        plannedCheckout = DateTime.Parse("2022-11-20 01:01:01")
                    }

                let (Ok hotel') = hotel.AddBooking booking
                Expect.isSome hotel'.bookings.Head.id  "should be some"
                let bookedDays = hotel'.GetBookedDaysOfRoom 1
                Expect.equal 
                    bookedDays 
                        (
                        [
                            DateTime.Parse("2022-11-11 00:00:00")
                            DateTime.Parse("2022-11-12 00:00:00")
                            DateTime.Parse("2022-11-13 00:00:00")
                            DateTime.Parse("2022-11-14 00:00:00")
                            DateTime.Parse("2022-11-15 00:00:00")
                            DateTime.Parse("2022-11-16 00:00:00")
                            DateTime.Parse("2022-11-17 00:00:00")
                            DateTime.Parse("2022-11-18 00:00:00")
                            DateTime.Parse("2022-11-19 00:00:00")
                        ] 
                        |> Set.ofList
                        )
                    "should be true"

            testCase "two booking intervals. Get the booked days - Ok" <| fun _ ->
                let hotel = 
                    {
                        State.GetEmpty()
                            with
                                rooms = [room1]
                    }
                let booking: Booking =
                    {
                        id = None
                        roomId = 1
                        customerEmail = "email@me.com"
                        plannedCheckin = DateTime.Parse("2022-11-11 01:01:01")
                        plannedCheckout = DateTime.Parse("2022-11-20 01:01:01")
                    }
                let booking2: Booking =
                    {
                        id = None
                        roomId = 1
                        customerEmail = "email@me.com"
                        plannedCheckin = DateTime.Parse("2022-12-01 01:01:01")
                        plannedCheckout = DateTime.Parse("2022-12-04 01:01:01")
                    }
                let (Ok hotel') = hotel.AddBooking booking
                let (Ok hotel'') = hotel'.AddBooking booking2
                let bookedDays = hotel''.GetBookedDaysOfRoom 1

                Expect.equal 
                    bookedDays 
                    ([
                        DateTime.Parse("2022-11-11 00:00:00")
                        DateTime.Parse("2022-11-12 00:00:00")
                        DateTime.Parse("2022-11-13 00:00:00")
                        DateTime.Parse("2022-11-14 00:00:00")
                        DateTime.Parse("2022-11-15 00:00:00")
                        DateTime.Parse("2022-11-16 00:00:00")
                        DateTime.Parse("2022-11-17 00:00:00")
                        DateTime.Parse("2022-11-18 00:00:00")
                        DateTime.Parse("2022-11-19 00:00:00")

                        DateTime.Parse("2022-12-01 00:00:00")
                        DateTime.Parse("2022-12-02 00:00:00")
                        DateTime.Parse("2022-12-03 00:00:00")
                    ] |> Set.ofList
                    )
                    "should be true"

            testCase "add a booking that already has an id Error - Ok" <| fun _ ->
                let hotel = 
                    {
                        State.GetEmpty()
                            with
                                rooms = [room1]
                    }
                let booking: Booking =
                    {
                        id = Guid.Parse("a338074d-98bd-4e64-b76a-b72cd3b0d9dd") |> Some
                        roomId = 1
                        customerEmail = "email@me.com"
                        plannedCheckin = DateTime.Parse("2022-11-11 01:01:01")
                        plannedCheckout = DateTime.Parse("2022-11-12 01:01:01")
                    }
                let (Error error) = hotel.AddBooking booking
                Expect.equal error "cannot add a booking that already has an id" "should be equal"

            testCase "two booking on two different available rooms - Ok" <| fun _ ->
                let room2 =
                    {
                        id = 2
                        description = None
                    }
                let hotel = 
                    {
                        State.GetEmpty()
                            with
                                rooms = [room1; room2]
                    }
                let booking1: Booking =
                    {
                        id = None
                        roomId = 1
                        customerEmail = "email@me.com"
                        plannedCheckin = DateTime.Parse("2022-11-11 01:01:01")
                        plannedCheckout = DateTime.Parse("2022-11-12 01:01:01")
                    }
                let booking2: Booking =
                    {
                        id = None
                        roomId = 2
                        customerEmail = "email@me.com"
                        plannedCheckin = DateTime.Parse("2022-11-11 01:01:01")
                        plannedCheckout = DateTime.Parse("2022-11-12 01:01:01")
                    }
                let (Ok hotel') = hotel.AddBooking booking1
                let (Ok hotel'') = hotel'.AddBooking booking2
                Expect.equal (hotel''.bookings.Length) 2 "should be equal"

            testCase "cannot add a booking about the same room when the period is the same - Error" <| fun _ ->
                let hotel = 
                    {
                        State.GetEmpty()
                            with
                                rooms = [room1]
                    }
                let booking1: Booking =
                    {
                        id = None
                        roomId = 1
                        customerEmail = "email@me.com"
                        plannedCheckin = DateTime.Parse("2022-11-11 01:01:01")
                        plannedCheckout = DateTime.Parse("2022-11-12 01:01:01")
                    }
                let booking2: Booking =
                    {
                        id = None
                        roomId = 1
                        customerEmail = "email@me.com"
                        plannedCheckin = DateTime.Parse("2022-11-11 01:01:01")
                        plannedCheckout = DateTime.Parse("2022-11-12 01:01:01")
                    }

                let (Ok newState) = hotel.AddBooking booking1 
                let (Error error) = newState.AddBooking booking2
                Expect.equal error "overlap: \"2022/11/11\"" "should be equal"

            testCase "add more bookings on same room, that have no overlaps - Ok" <| fun _ ->
                let hotel = 
                    {
                        State.GetEmpty()
                            with
                                rooms = [room1]
                    }
                let booking1: Booking =
                    {
                        id = None
                        roomId = 1
                        customerEmail = "email@me.com"
                        plannedCheckin = DateTime.Parse("2022-11-11 01:01:01")
                        plannedCheckout = DateTime.Parse("2022-11-12 01:01:01")
                    }
                let booking2: Booking =
                    {
                        id = None
                        roomId = 1
                        customerEmail = "email@me.com"
                        plannedCheckin= DateTime.Parse("2022-11-15 01:01:01")
                        plannedCheckout= DateTime.Parse("2022-11-16 01:01:01")
                    }

                let (Ok hotel') = hotel.AddBooking booking1 
                let (Ok hotel'') = hotel'.AddBooking booking2 
                Expect.equal (hotel''.bookings.Length) 2 "should be equal"

            testCase "the checkin date of a booking can be the checkout date of another booking - Ok" <| fun _ ->
                let hotel = 
                    {
                        State.GetEmpty()
                            with
                                rooms = [room1]
                    }
                let booking1: Booking =
                    {
                        id = None
                        roomId = 1
                        customerEmail = "email@me.com"
                        plannedCheckin = DateTime.Parse("2022-11-11 01:01:01")
                        plannedCheckout = DateTime.Parse("2022-11-12 01:01:01")
                    }
                let booking2: Booking =
                    {
                        id = None
                        roomId = 1
                        customerEmail = "email@me.com"
                        plannedCheckin = DateTime.Parse("2022-11-12 01:01:01")
                        plannedCheckout = DateTime.Parse("2022-11-16 01:01:01")
                    }
                let (Ok hotel') = hotel.AddBooking booking1 
                let (Ok hotel'') = hotel'.AddBooking booking2 
                Expect.equal (hotel''.bookings.Length) 2 "should be equal"

            testCase "add bookings that overlaps - Error" <| fun _ ->
                let hotel = 
                    {
                        State.GetEmpty()
                            with
                                rooms = [room1]
                    }
                let booking1: Booking =
                    {
                        id = None
                        roomId = 1
                        customerEmail = "email@me.com"
                        plannedCheckin = DateTime.Parse("2022-11-11 01:01:01")
                        plannedCheckout = DateTime.Parse("2022-11-15 01:01:01")
                    }
                let booking2: Booking =
                    {
                        id = None
                        roomId = 1
                        customerEmail = "email@me.com"
                        plannedCheckin= DateTime.Parse("2022-11-14 01:01:01")
                        plannedCheckout = DateTime.Parse("2022-11-16 01:01:01")
                    }
                let (Ok hotel') = hotel.AddBooking booking1 
                let (Error error) = hotel'.AddBooking booking2 
                Expect.equal error "overlap: \"2022/11/14\"" "should be equal"
        ] 

    [<Tests>]
    let eventsTests =
        testList "Domain events" [
            testCase "add room event  - Ok" <| fun _ ->
                let hotel = State.GetEmpty()
                let uEvent = Event.RoomAdded room1
                let (Ok hotel') = hotel |> uEvent.Process
                let expected = 
                    {
                        rooms = [room1]
                        bookings = []
                        id = 1
                    }
                Expect.equal hotel' expected "should be equal"

            testCase "add booking simple event - Ok" <| fun _ ->
                let booking: Booking =
                    {
                        id = None
                        roomId = 1
                        customerEmail = "email@me.com"
                        plannedCheckin= DateTime.Parse("2022-11-11 01:01:01")
                        plannedCheckout = DateTime.Parse("2022-11-12 01:01:01")
                    }
                let hotel = 
                    {
                        State.GetEmpty() with
                            rooms = [room1]
                    }
                let event = bookingAdded booking
                let (Ok hotel') = event hotel
                Expect.isSome hotel'.bookings.Head.id "should be some"

            testCase "add booking event - Ok" <| fun _ ->
                let booking: Booking =
                    {
                        id = None
                        roomId = 1
                        customerEmail = "email@me.com"
                        plannedCheckin= DateTime.Parse("2022-11-11 01:01:01")
                        plannedCheckout = DateTime.Parse("2022-11-12 01:01:01")
                    }
                let hotel = 
                    {
                        State.GetEmpty() with
                            rooms = [room1]
                    }
                let uEvent = Event.BookingAdded booking
                let (Ok hotel') = hotel |> uEvent.Process
                Expect.isSome hotel'.bookings.Head.id "should be some"

            testCase "add two rooms - Ok" <| fun _ ->
                let room2 =
                    {
                        id = 2
                        description = None
                    }
                let hotel = State.GetEmpty()
                let uRoom1Added = Event.RoomAdded room1
                let uRoom2Added = Event.RoomAdded room2
                let uEvents = [uRoom1Added; uRoom2Added]
                let (Ok hotel') = uEvents |> hotel.Evolve
                Expect.equal hotel' {hotel with rooms = [room2; room1]; id = 2} "should be equal"

            testCase "add a room and a booking - Ok" <| fun _ ->
                let booking: Booking =
                    {
                        id = None
                        roomId = 1
                        customerEmail = "email@me.com"
                        plannedCheckin= DateTime.Parse("2022-11-11 01:01:01")
                        plannedCheckout = DateTime.Parse("2022-11-12 01:01:01")
                    }
                let hotel = State.GetEmpty()
                let room1Added = Event.RoomAdded room1
                let booking1Added = Event.BookingAdded booking
                let events = [room1Added; booking1Added] 
                let (Ok hotel') = events |> hotel.Evolve
                let actualBookingNoId =
                    {
                        hotel'.bookings.Head 
                            with 
                                id = None
                    }
                Expect.equal (hotel'.rooms.Head) room1 "should be equal"
                Expect.equal actualBookingNoId booking "should be equal"

            testCase "add a room and a booking UNION EVENT - Ok" <| fun _ ->
                let booking: Booking =
                    {
                        id = None
                        roomId = 1
                        customerEmail = "email@me.com"
                        plannedCheckin= DateTime.Parse("2022-11-11 01:01:01")
                        plannedCheckout = DateTime.Parse("2022-11-12 01:01:01")
                    }
                let hotel = State.GetEmpty()
                let room1Added = Event.RoomAdded room1 
                let booking1Added = Event.BookingAdded booking
                let events = [room1Added; booking1Added]
                let (Ok hotel') = events |> hotel.Evolve
                let actualBookingNoId =
                    {
                        hotel'.bookings.Head 
                            with 
                                id = None
                    }
                Expect.equal (hotel'.rooms.Head) room1 "should be equal"
                Expect.equal actualBookingNoId booking "should be equal"

            testCase "add already existing room - Error" <| fun _ ->
                let hotel = 
                    {
                        State.GetEmpty()
                            with rooms = [room1]
                    }
                let room1Added = Event.RoomAdded room1
                let events = [room1Added] 
                let (Error error) = events |> hotel.Evolve
                Expect.equal error "a room with number 1 already exists" "should be equal"

            testCase "add overlapping booking - Error" <| fun _ -> 
                let booking = 
                    {
                        id = Guid.Parse("d45c0760-dbf7-4453-a15f-b4cb1b78c730") |> Some
                        roomId = 1
                        customerEmail = "me@you.us"
                        plannedCheckin = DateTime.Parse("2022-11-11 00:00:00")
                        plannedCheckout = DateTime.Parse("2022-11-12 00:00:00")
                    }
                let hotel = 
                    {
                        State.GetEmpty()
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
                let (Error result) = [bookingAdded] |> hotel.Evolve
                Expect.equal result "overlap: \"2022/11/11\"" "should be equal"

            testCase "add booking on free period - Ok" <| fun _ -> 
                let booking = 
                    {
                        id = Guid.Parse("d45c0760-dbf7-4453-a15f-b4cb1b78c730") |> Some
                        roomId = 1
                        customerEmail = "me@you.us"
                        plannedCheckin = DateTime.Parse("2022-11-11 00:00:00")
                        plannedCheckout = DateTime.Parse("2022-11-12 00:00:00")
                    }
                let hotel = 
                    {
                        State.GetEmpty()
                        with
                            rooms = [room1]
                            bookings = [booking]
                    }
                let booking1 = 
                    {
                        booking with
                            id = None
                            plannedCheckin = DateTime.Parse("2022-11-12 00:00:00")
                            plannedCheckout = DateTime.Parse("2022-11-20 00:00:00")
                    }
                let booking1Added = Event.BookingAdded booking1
                let events = [booking1Added] 
                let (Ok hotel') = events |> hotel.Evolve

                Expect.equal (hotel'.bookings.Length) 2 "should be equal"
        ]

    [<Tests>]
    let CommandTests =
        testList "Union" [
            testCase "addRoom command returns roomAdded event - Ok" <| fun _ ->
                let hotel = State.GetEmpty()
                let command = Command.AddRoom room1
                let expected = [Event.RoomAdded room1] |> Ok
                let actual = command.Execute hotel
                Expect.equal actual expected "should be equal"

            testCase "addBooking command returns bookingAdded event - Ok" <| fun _ ->
                let hotel = State.GetEmpty().AddRoom room1 |> Result.get 
                let booking = 
                    {
                        id = None
                        roomId = 1
                        customerEmail = "me@you.us"
                        plannedCheckin = DateTime.Parse("2022-11-11 00:00:00")
                        plannedCheckout = DateTime.Parse("2022-11-12 00:00:00")
                    }
                let command = Command.AddBooking booking
                let expected = [Event.BookingAdded booking] |> Ok
                let actual = command.Execute hotel
                Expect.equal actual expected "should be equal"

            testCase "addBooking command returns Error in booking unxisting room - KO" <| fun _ ->
                let hotel = State.GetEmpty().AddRoom room1 |>  Result.get 
                let booking = 
                    {
                        id = None
                        roomId = 666 
                        customerEmail = "me@you.us"
                        plannedCheckin = DateTime.Parse("2022-11-11 00:00:00")
                        plannedCheckout = DateTime.Parse("2022-11-12 00:00:00")
                    }
                let command = Command.AddBooking booking
                let (Error actual) = command.Execute hotel
                Expect.equal actual "room 666 doesn't exist" "should be equal"
        ]
