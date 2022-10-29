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


module Tests =

    let room1 = {
        id = 1
        description = None
    }

    let roomAdded r =
        fun (x: Hotel) -> x.AddRoom r
    let bookingAdded b =
        fun (x: Hotel) -> x.AddBooking b

    let sameBooking (booking1: Booking) (booking2: Booking) =
        let b1  = { booking1 with id = None } 
        let b2 =  { booking2 with id = None }
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
    let domainObjectTests =
        testList "Domain objects" [
                
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
                let hotel = Hotel.GetEmpty()
                let expected = 
                    {
                        hotel with
                            rooms = [room1]
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
                        Hotel.GetEmpty()
                            with
                                rooms = [room1]
                    }
                let result = room1b |> hotel.AddRoom 
                Expect.isError result "should be error"
                let (Error actual) = result
                Expect.equal actual "a room with number 1 already exists" "shold be true"

            testCase "create booking about an existing room - Ok" <| fun _ ->
                let hotel = 
                    {
                        Hotel.GetEmpty()
                            with
                                rooms = [room1]
                    }
                let booking = mkBooking 1  (DateTime.Parse("2022-11-11 01:01:01")) (DateTime.Parse("2022-11-12 01:01:01"))
                let result = booking |> hotel.AddBooking  
                Expect.isOk result "should be ok"
                let hotelWithBooking = result |> Result.get
                let actualBooking = hotelWithBooking.bookings.Head 
                Expect.isTrue (sameBooking actualBooking booking) "should be true"

            testCase "create a booking about a room that doesn't exist - Error" <| fun _ ->
                let hotel = 
                    {
                        Hotel.GetEmpty()
                            with
                                rooms = [room1]
                    }
                let booking = mkBooking 666 (DateTime.Parse("2022-11-11 01:01:01")) (DateTime.Parse("2022-11-12 01:01:01"))

                let result =  booking |> hotel.AddBooking 
                Expect.isError result  "should be error"
                let (Error error) = result
                Expect.equal error "room 666 doesn't exist" "should be equal"

            testCase "after creating a booking, the booking must have an id - Ok" <| fun _ ->
                let hotel = 
                    {
                        Hotel.GetEmpty()
                            with
                                rooms = [room1]
                    }
                let booking = mkBooking 1 (DateTime.Parse("2022-11-11 01:01:01")) (DateTime.Parse("2022-11-12 01:01:01"))

                let result = booking |> hotel.AddBooking 
                Expect.isOk result "should be ok"
                let (Ok hotel') = result
                Expect.isSome hotel'.bookings.Head.id  "should be some"

            testCase "after creating a booking, then the busy days of the booking are the days in the checkin checkout interval (excluding checkout day) - Ok" <| fun _ ->
                let hotel = 
                    {
                        Hotel.GetEmpty()
                            with
                                rooms = [room1]
                    }
                let booking = mkBooking 1 (DateTime.Parse("2022-11-11 01:01:01")) (DateTime.Parse("2022-11-12 01:01:01"))
                let result = hotel.AddBooking booking
                Expect.isOk result "should be ok"
                let (Ok hotel') = result
                Expect.isTrue (hotel'.bookings.Length > 0) "should be true"
                Expect.isSome hotel'.bookings.Head.id "should be some"
                let bookedDays = hotel'.GetBookedDaysOfRoom 1
                Expect.equal bookedDays ([DateTime.Parse("2022-11-11 00:00:00")] |> Set.ofList) "should be true"

            testCase "larger booking interval. Get the booked days. From checkin date to checkout date excluded - Ok" <| fun _ ->
                let hotel = 
                    {
                        Hotel.GetEmpty()
                            with
                                rooms = [room1]
                    }

                let booking = mkBooking 1 (DateTime.Parse("2022-11-11 01:01:01")) (DateTime.Parse("2022-11-20 01:01:01"))

                let result = hotel.AddBooking booking
                Expect.isOk result "should be Ok"
                let hotel' = result |> Result.get
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
                        Hotel.GetEmpty()
                            with
                                rooms = [room1]
                    }

                let booking = mkBooking 1 (DateTime.Parse("2022-11-11 01:01:01")) (DateTime.Parse("2022-11-20 01:01:01"))
                let booking2 = mkBooking 1 (DateTime.Parse("2022-12-01 01:01:01")) (DateTime.Parse("2022-12-04 01:01:01"))

                let hotelWithBookings =
                    ceError 
                        {
                            let! hotel' = hotel.AddBooking booking
                            let! hotel'' = hotel'.AddBooking booking2
                            return hotel''
                        }
                Expect.isOk hotelWithBookings "should be ok"
                let hotel' = hotelWithBookings |> Result.get
                let bookedDays = hotel'.GetBookedDaysOfRoom 1

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
                        Hotel.GetEmpty()
                            with
                                rooms = [room1]
                    }
                let booking = 
                    {
                        mkBooking 1  (DateTime.Parse("2022-11-11 01:01:01")) (DateTime.Parse("2022-11-12 01:01:01"))
                            with
                                id = Guid.Parse("a338074d-98bd-4e64-b76a-b72cd3b0d9dd") |> Some
                    }

                let result = hotel.AddBooking booking
                Expect.isError result "should be error"
                let (Error error) = result
                Expect.equal error "cannot add a booking that already has an id" "should be equal"

            testCase "two booking on two different available rooms - Ok" <| fun _ ->
                let room2 = mkRoom 2
                let hotel = 
                    {
                        Hotel.GetEmpty()
                            with
                                rooms = [room1; room2]
                    }

                let booking1 = mkBooking 1  (DateTime.Parse("2022-11-11 01:01:01")) (DateTime.Parse("2022-11-12 01:01:01"))
                let booking2 = mkBooking 2  (DateTime.Parse("2022-11-11 01:01:01")) (DateTime.Parse("2022-11-12 01:01:01"))

                let hotel' =
                    ceError 
                        {
                            let! hotel' = hotel.AddBooking booking1
                            let! hotel'' = hotel'.AddBooking booking2
                            return hotel''
                        }
                Expect.isOk hotel' "should be ok"
                let hotel'' = hotel' |> Result.get
                
                Expect.equal (hotel''.bookings.Length) 2 "should be equal"

            testCase "cannot add a booking about the same in the same time interval - Error" <| fun _ ->
                let hotel = 
                    {
                        Hotel.GetEmpty()
                            with
                                rooms = [room1]
                    }

                let booking1 = mkBooking 1 (DateTime.Parse("2022-11-11 01:01:01")) (DateTime.Parse("2022-11-12 01:01:01"))
                let booking2 = mkBooking 1 (DateTime.Parse("2022-11-11 01:01:01")) (DateTime.Parse("2022-11-12 01:01:01"))
                let result1 = hotel.AddBooking booking1 
                Expect.isOk result1 "should be ok"

                let (Ok newHotel) = result1
                let result2 =  newHotel.AddBooking booking2
                Expect.isError result2 "should be error"
                let (Error error) = newHotel.AddBooking booking2
                
                Expect.equal error "overlap: \"2022/11/11\"" "should be equal"

            testCase "add more bookings on same room if there is no overlapping days - Ok" <| fun _ ->
                let hotel = 
                    {
                        Hotel.GetEmpty()
                            with
                                rooms = [room1]
                    }

                let booking1 = mkBooking 1 (DateTime.Parse("2022-11-11 01:01:01")) (DateTime.Parse("2022-11-12 01:01:01"))
                let booking2 = mkBooking 1 (DateTime.Parse("2022-11-15 01:01:01")) (DateTime.Parse("2022-11-16 01:01:01"))
                
                let result =
                    ceError
                        {
                            let! hotel' = hotel.AddBooking booking1 
                            let! hotel'' = hotel'.AddBooking booking2
                            return hotel''
                        }
                Expect.isOk result "should be ok"
                let hotel' = result |> Result.get
                Expect.equal (hotel'.bookings.Length) 2 "should be equal"

            testCase "the checkin date of a booking can be the checkout date of another booking - Ok" <| fun _ ->
                let hotel = 
                    {
                        Hotel.GetEmpty()
                            with
                                rooms = [room1]
                    }

                let booking1 = mkBooking 1 (DateTime.Parse("2022-11-11 01:01:01")) (DateTime.Parse("2022-11-12 01:01:01"))
                let booking2 = mkBooking 1 (DateTime.Parse("2022-11-12 01:01:01")) (DateTime.Parse("2022-11-16 01:01:01"))

                let (Ok hotel') = hotel.AddBooking booking1 
                let (Ok hotel'') = hotel'.AddBooking booking2 
                Expect.equal (hotel''.bookings.Length) 2 "should be equal"

            testCase "add bookings that overlaps - Error" <| fun _ ->
                let hotel = 
                    {
                        Hotel.GetEmpty()
                            with
                                rooms = [room1]
                    }
                let booking1 = mkBooking 1 (DateTime.Parse("2022-11-11 01:01:01")) (DateTime.Parse("2022-11-15 01:01:01"))
                let booking2 = mkBooking 1 (DateTime.Parse("2022-11-14 01:01:01")) (DateTime.Parse("2022-11-16 01:01:01"))
                let result =
                    ceError 
                        {
                            let! hotel' = hotel.AddBooking booking1
                            let! hotel'' = hotel'.AddBooking booking2
                            return hotel''
                        }
                Expect.isError result "should be error"                        
                let (Error error) = result
                Expect.equal error "overlap: \"2022/11/14\"" "should be equal"

            testCase "a hotel with three rooms: add more bookings - OK" <| fun _ ->
                let room2 = mkRoom 2
                let room3 = mkRoom 3

                let hotel = 
                    {
                        Hotel.GetEmpty()
                            with rooms = [room1; room2; room3]
                    }
                let booking = mkBooking 1 (DateTime.Parse("2022-11-11 01:01:01")) (DateTime.Parse("2022-11-15 01:01:01"))
                let booking2 = mkBooking 2 (DateTime.Parse("2022-11-11 01:01:01")) (DateTime.Parse("2022-11-17 01:01:01"))
                let booking3 = mkBooking 3 (DateTime.Parse("2022-11-11 01:01:01")) (DateTime.Parse("2022-11-15 01:01:01"))

                let hotelWithBookings =
                    ceError
                        {
                            let! hotel' = hotel.AddBooking booking 
                            let! hotel'' = hotel'.AddBooking booking2
                            let! hotel''' = hotel''.AddBooking booking3
                            return hotel'''
                        }

                Expect.isOk hotelWithBookings "should be ok"
                let hotelWithBookings' = hotelWithBookings |> Result.get
                Expect.equal (hotelWithBookings'.bookings.Length) 3 "should be equal"

            testCase "an empty hotel will give empty list of room available for any checkin-checkout period" <| fun _ ->
                let hotel = Hotel.GetEmpty()
                let availableRooms = hotel.FindFullVacancies (DateTime.Parse("2022-11-14 01:01:01")) (DateTime.Parse("2022-11-16 01:01:01"))
                Expect.equal availableRooms [] "should be equal"

            testCase "an hotel with a room and no booking will give the room as available for any checkin-checkout period " <| fun _ ->
                let hotel = 
                    {
                        Hotel.GetEmpty()
                            with rooms = [room1]
                    }
                let availableRooms = hotel.FindFullVacancies (DateTime.Parse("2022-11-14 01:01:01")) (DateTime.Parse("2022-11-16 01:01:01"))
                Expect.equal availableRooms [room1] "should be equal"

            testCase "a hotel with only a room which is booked in the queried period will give an empty room as result" <| fun _ ->
                let hotel = 
                    {
                        Hotel.GetEmpty()
                            with rooms = [room1]
                    }
                let booking = mkBooking 1 (DateTime.Parse("2022-11-11 01:01:01")) (DateTime.Parse("2022-11-15 01:01:01"))

                let hotel' = hotel.AddBooking booking |> Result.get
                let availableRooms = hotel'.FindFullVacancies (DateTime.Parse("2022-11-11 01:01:01")) (DateTime.Parse("2022-11-15 01:01:01"))
                Expect.equal availableRooms [] "should be equal"

            testCase "a hotel with only a room which is free in the queried period will give the room as result" <| fun _ ->
                let hotel = 
                    {
                        Hotel.GetEmpty()
                            with rooms = [room1]
                    }
                let booking = mkBooking 1 (DateTime.Parse("2022-11-11 01:01:01")) (DateTime.Parse("2022-11-15 01:01:01"))
                let result =  hotel.AddBooking booking
                Expect.isOk result "should be ok"
                let hotel' = result |> Result.get
                let availableRooms = hotel'.FindFullVacancies (DateTime.Parse("2022-11-16 01:01:01")) (DateTime.Parse("2022-11-19 01:01:01"))
                Expect.equal availableRooms [room1] "should be equal"

            testCase "a hotel with two free rooms in the queried period will give both the room as result" <| fun _ ->
                let room2 = {
                    id = 2
                    description = None
                }
                let hotel = 
                    {
                        Hotel.GetEmpty()
                            with rooms = [room1; room2]
                    }
                let booking = mkBooking 1 (DateTime.Parse("2022-11-11 01:01:01")) (DateTime.Parse("2022-11-15 01:01:01"))
                let booking2 = mkBooking 2 (DateTime.Parse("2022-11-11 01:01:01")) (DateTime.Parse("2022-11-15 01:01:01"))

                let result =
                    ceError
                        {
                            let! hotel' = hotel.AddBooking booking
                            let! hotel'' = hotel'.AddBooking booking2
                            return hotel''

                        }
                Expect.isOk result "should be ok"
                let hotel' = result |> Result.get
                let availableRooms = hotel'.FindFullVacancies (DateTime.Parse("2022-11-16 01:01:01")) (DateTime.Parse("2022-11-19 01:01:01"))
                Expect.equal availableRooms [room1; room2] "should be equal"

            testCase "a hotel with two rooms: only one is busy in the queried period, will return only the free one" <| fun _ ->
                let room2 = {
                    id = 2
                    description = None
                }
                let hotel = 
                    {
                        Hotel.GetEmpty()
                            with rooms = [room1; room2]
                    }
                let booking = mkBooking 1 (DateTime.Parse("2022-11-11 01:01:01")) (DateTime.Parse("2022-11-15 01:01:01"))
                let booking2 = mkBooking 2 (DateTime.Parse("2022-11-11 01:01:01")) (DateTime.Parse("2022-11-17 01:01:01"))
                let result = 
                    ceError {
                        let! hotel' = hotel.AddBooking booking
                        let! hotel'' = hotel'.AddBooking booking2
                        return hotel''
                    }
                Expect.isOk result "should be ok"
                let hotel' = result |> Result.get
                let availableRooms = hotel'.FindFullVacancies (DateTime.Parse("2022-11-16 01:01:01")) (DateTime.Parse("2022-11-19 01:01:01"))
                Expect.equal availableRooms [room1] "should be equal"

            testCase "a hotel with three rooms: only one is busy in the queried period, will return only the free one" <| fun _ ->
                let room2 = mkRoom 2
                let room3 = mkRoom 3
                let hotel = 
                    {
                        Hotel.GetEmpty()
                            with rooms = [room1; room2; room3]
                    }
                let booking = mkBooking 1 (DateTime.Parse("2022-11-11 01:01:01")) (DateTime.Parse("2022-11-15 01:01:01"))
                let booking2 = mkBooking 2  (DateTime.Parse("2022-11-11 01:01:01")) (DateTime.Parse("2022-11-17 01:01:01"))
                let booking3 = mkBooking 3 (DateTime.Parse("2022-11-11 01:01:01")) (DateTime.Parse("2022-11-15 01:01:01"))

                let hotelWithBookings =
                    ceError
                        {
                            let! hotel' = hotel.AddBooking booking 
                            let! hotel'' = hotel'.AddBooking booking2
                            let! hotel''' = hotel''.AddBooking booking3
                            return hotel'''
                        }
                Expect.isOk hotelWithBookings "should be ok"   
                let hotelWithBookings' = hotelWithBookings |> Result.get
                let availableRooms = hotelWithBookings'.FindFullVacancies (DateTime.Parse("2022-11-16 01:01:01")) (DateTime.Parse("2022-11-19 01:01:01"))
                Expect.equal availableRooms [room1; room3] "should be equal"
        ] 
