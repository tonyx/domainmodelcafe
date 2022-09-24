
namespace hostelmodeling

open Expecto
open hotelmodeling.Domain
open hotelmodeling.CommandEvents
open System
open FSharp.Core
open FSharpPlus.Data
open Newtonsoft
open Newtonsoft.Json
open hostelmodeling.HotelSerialization

open System.IO
open System.Text

module SerializeTests =
    [<Tests>]
    let serializeTests =
        testList "serializetests" [
            testCase "serialize room" <| fun _ ->
                let room: Room = 
                    {
                        id = 1
                        description = None
                    }
                let serialized = room.Serialize() 
                let expected = 
                    """
                        {"id":1,"description":null}
                    """.Trim()
                Expect.equal serialized expected  "should be equal"

            testCase "serialize room - OK" <| fun _ ->
                let input =
                    """
                        {"id":1,"description":null}
                    """.Trim()
                let room: Room = 
                    {
                        id = 1
                        description = None
                    }
                let (Ok deserialized) = Room.Deserialize input 
                Expect.equal deserialized room  "should be equal"

            testCase "serialize room2" <| fun _ ->
                let room: Room = 
                    {
                        id = 42 
                        description = "nice view" |> Some
                    }
                let serialized = room.Serialize()
                let expected = 
                    """
                        {"id":42,"description":{"Case":"Some","Fields":["nice view"]}}
                    """.Trim()
                Expect.equal serialized expected  "should be equal"

            testCase "deserialize room2" <| fun _ ->
                let room: Room = 
                    {
                        id = 42 
                        description = "nice view" |> Some
                    }
                let input = 
                    """
                        {"id":42,"description":{"Case":"Some","Fields":["nice view"]}}
                    """.Trim()
                let (Ok deserialized) = Room.Deserialize input
                Expect.equal deserialized room  "should be equal"

            testCase "serialize hotel state 1" <| fun _ ->
                let empty = Hotel.GetEmpty()
                let actual = empty.Serialize()
                let expected = 
                    """
                        {"rooms":[],"bookings":[],"id":0}
                    """.Trim()
                Expect.equal actual expected "should be true"

            testCase "deserialize hotel state 1 - Ok" <| fun _ ->
                let empty = Hotel.GetEmpty()
                let input = 
                    """
                        {"rooms":[],"bookings":[],"id":0}
                    """.Trim()
                let (Ok actual) = Hotel.Deserialize input
                Expect.equal actual empty "should be true"

            testCase "deserialize hotel state 1 - Error" <| fun _ ->
                let input = 
                    """
                        {"rooms":[],"bookings":[],"id":0 asdfasdflQQ
                    """.Trim()
                let (Error actual) = Hotel.Deserialize input
                Expect.isTrue true "true"

            testCase "serialize hotel state 2" <| fun _ ->
                let input: Hotel = 
                    {
                        Hotel.GetEmpty() with
                            rooms = [{
                                id = 1
                                description = None
                            }]
                    }
                let actual = input.Serialize() 
                let expected = 
                    """
                        {"rooms":[{"id":1,"description":null}],"bookings":[],"id":0}
                    """.Trim()
                Expect.equal expected actual "should be true"

            testCase "deserialize hotel state 2" <| fun _ ->
                let expected: Hotel = 
                    {
                        Hotel.GetEmpty() with
                            rooms = [{
                                id = 1
                                description = None
                            }]
                    }
                let input = 
                    """
                        {"rooms":[{"id":1,"description":null}],"bookings":[],"id":0}
                    """.Trim()
                let (Ok actual) = Hotel.Deserialize input 
                Expect.equal expected actual "should be true"
                
            testCase "serialize hotel state 3" <| fun _ ->
                let input: Hotel = 
                    {
                        Hotel.GetEmpty() with
                            rooms = 
                                [
                                    {
                                        id = 111 
                                        description = None
                                    }
                                    {
                                        id = 666 
                                        description = "hot room" |> Some
                                    }
                                ]
                    }
                let actual = input.Serialize()
                let expected = 
                    """
                        {"rooms":[{"id":111,"description":null},{"id":666,"description":{"Case":"Some","Fields":["hot room"]}}],"bookings":[],"id":0}
                    """.Trim()
                Expect.equal actual expected  "should be bla true"

            testCase "deserialize hotel state 4" <| fun _ ->
                let expected: Hotel = 
                    {
                        Hotel.GetEmpty() with
                            rooms = 
                                [
                                    {
                                        id = 42 
                                        description = None
                                    }
                                    {
                                        id = 666 
                                        description = "hot room" |> Some
                                    }
                                ]
                    }
                let input = 
                    """
                        {"rooms":[{"id":42,"description":null},{"id":666,"description":{"Case":"Some","Fields":["hot room"]}}],"bookings":[],"id":0}
                    """.Trim()
                let (Ok actual) = Hotel.Deserialize input
                Expect.equal actual expected "should be bla true"

            testCase "union event room added test serialize "  <| fun _ ->
                let room = {
                    id = 1
                    description = None
                }
                let input = Event.RoomAdded room
                let actual = input.Serialize() 
                let expected = 
                    """
                        {"Case":"RoomAdded","Fields":[{"id":1,"description":null}]}
                    """.Trim()
                Expect.equal actual expected "should be equal"
                
            testCase "union event booking added test serialize - OK"  <| fun _ ->
                let booking = 
                    {
                        id = None
                        roomId = 1
                        customerEmail = "me@you.us"
                        plannedCheckin = DateTime.Parse("2022-11-11 00:00:00")
                        plannedCheckout = DateTime.Parse("2022-11-12 00:00:00")
                    }
                let input = Event.BookingAdded booking
                let actual = input.Serialize() 
                let expected =
                    """
                        {"Case":"BookingAdded","Fields":[{"id":null,"roomId":1,"customerEmail":"me@you.us","plannedCheckin":"2022-11-11T00:00:00","plannedCheckout":"2022-11-12T00:00:00"}]}
                    """.Trim()
                Expect.equal actual expected "should be true"

            testCase "union event booking added test deserialize - OK"  <| fun _ ->
                let booking = 
                    {
                        id = None
                        roomId = 1
                        customerEmail = "me@you.us"
                        plannedCheckin = DateTime.Parse("2022-11-11 00:00:00")
                        plannedCheckout = DateTime.Parse("2022-11-12 00:00:00")
                    }
                let expected = Event.BookingAdded booking
                let input =
                    """
                        {"Case":"BookingAdded","Fields":[{"id":null,"roomId":1,"customerEmail":"me@you.us","plannedCheckin":"2022-11-11T00:00:00","plannedCheckout":"2022-11-12T00:00:00"}]}
                    """.Trim()
                let (Ok result) = Event.Deserialize input
                Expect.equal result expected "should be true"

            testCase "union event test deserialize" <| fun _ ->
                let room = {
                    id = 1
                    description = None
                }
                let event = Event.RoomAdded room
                let input = 
                    """
                        {"Case":"RoomAdded","Fields":[{"id":1,"description":null}]}
                    """.Trim()
                let (Ok actual) = Event.Deserialize input 
                Expect.equal actual event "shoud be equal"

            testCase "union event roomAdded test deserialize  - error" <| fun _ ->
                let room = {
                    id = 1
                    description = None
                }
                let event = Event.RoomAdded room
                let input = 
                    """
                        {"CaseWWW":"RoomAdded","Fields":[{"id":1,"description":null}]}
                    """.Trim()
                let (Error actual) = Event.Deserialize input 
                Expect.isTrue true "true"
        ]
    [<Tests>]
    let serializeCommandsTests =
        testList "serializeCommandstests" [
            testCase "serialize AddRoom" <| fun _ ->
                let hotel = Hotel.GetEmpty()
                let room: Room = 
                    {
                        id = 1
                        description = None
                    }
                let addRoomCommand =
                    Command.AddRoom room
                let actual = addRoomCommand.Serialize()
                let expected = """{"Case":"AddRoom","Fields":[{"id":1,"description":null}]}"""
                Expect.equal actual expected "should be equal"
                
            // testCase "serialize AddBooking" <| fun _ ->
            //     let hotel = Hotel.GetEmpty()
            //     let room: Room = 
            //         {
            //             id = 1
            //             description = None
            //         }
            //     let addRoomCommand =
            //         Command.AddRoom room
            //     let actual = addRoomCommand.Serialize()
            //     let expected = """{"Case":"AddRoom","Fields":[{"id":1,"description":null}]}"""
            //     Expect.equal actual expected "should be equal"
        ]

    [<Tests>]
    let InterpretSerializedEventsTest =
        testList "serializeEventsTests" [
            testCase "interpret single addRoom event from serialized" <| fun _ ->
                let hotel = Hotel.GetEmpty()
                let room1 = {
                    id = 1
                    description = None
                }
                let sEvent = 
                    """
                        {"Case":"RoomAdded","Fields":[{"id":1,"description":null}]}
                    """

                let (Ok hotel') = hotel.SEvolve [sEvent]

                let expected = {
                    hotel with
                        rooms = [room1]
                        id = 1
                }
                Expect.equal hotel' expected "should be true" 

            testCase "add two addRoom - OK" <| fun _ ->
                let hotel = Hotel.GetEmpty()
                let room1 = {
                    id = 1
                    description = None
                }
                let room2 = {
                    id = 2
                    description = None
                }
                let sEvent = 
                    """
                        {"Case":"RoomAdded","Fields":[{"id":1,"description":null}]}
                    """
                let sEvent2 = 
                    """
                        {"Case":"RoomAdded","Fields":[{"id":2,"description":null}]}
                    """

                let (Ok hotel') = hotel.SEvolve [sEvent; sEvent2]

                let expected = {
                    hotel with
                        rooms = [room2; room1]
                        id = 2
                }

                Expect.equal hotel' expected "should be true" 

            testCase "add room with the same id twice - Error" <| fun _ ->
                let hotel = Hotel.GetEmpty()
                let room1 = {
                    id = 1
                    description = None
                }
                let room2 = {
                    id = 2
                    description = None
                }
                let sEvent = 
                    """
                        {"Case":"RoomAdded","Fields":[{"id":1,"description":null}]}
                    """
                let sEvent2 = 
                    """ 
                        {"Case":"RoomAdded","Fields":[{"id":1,"description":{"Case":"Some","Fields":["hot room"]}}]}
                    """
                let (Error result) =  hotel.SEvolve [sEvent; sEvent2]
                Expect.equal result "a room with number 1 already exists" "should be equal"

            testCase "add two rooms wrong json format - Error" <| fun _ ->
                let hotel = Hotel.GetEmpty()
                let room1 = {
                    id = 1
                    description = None
                }
                let room2 = {
                    id = 2
                    description = None
                }
                let sEvent = 
                    """ 
                        {"CaseWWWW":"RoomAdded","Fields":[{"id":1,"description":{"Case":"Some","Fields":["hot room"]}}]}
                    """
                let (Error result) =  hotel.SEvolve [sEvent]
                Expect.isTrue (result.Contains("Unexpected property 'CaseWWWW'")) "should be true"
        ]