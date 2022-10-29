
namespace hotelmodeling

open Expecto
open hotelmodeling.Domain
open hotelmodeling.CommandEvents
open hotelmodeling.HotelSerialization
open System
open FSharp.Core
open FSharpPlus
open FSharpPlus.Data

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
                let result = Room.Deserialize input
                Expect.isOk result "should be ok"
                let deserialized = result |> Result.get
                Expect.equal deserialized room  "should be equal"

            testCase "serialize room2 - OK" <| fun _ ->
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

            testCase "deserialize room2 - OK" <| fun _ ->
                let room: Room = 
                    {
                        id = 42 
                        description = "nice view" |> Some
                    }
                let input = 
                    """
                        {"id":42,"description":{"Case":"Some","Fields":["nice view"]}}
                    """.Trim()
                let result = Room.Deserialize input
                Expect.isOk result "should be ok"
                let deserialized = result |> Result.get
                Expect.equal deserialized room  "should be equal"

            testCase "serialize hotel state 1" <| fun _ ->
                let empty = Hotel.GetEmpty()
                let actual = empty.Serialize()
                let expected = 
                    """
                        {"rooms":[],"bookings":[]}
                    """.Trim()
                Expect.equal actual expected "should be true"

            testCase "deserialize hotel state 1 - Ok" <| fun _ ->
                let empty = Hotel.GetEmpty()
                let input = 
                    """
                        {"rooms":[],"bookings":[]}
                    """.Trim()
                let result = input |> Hotel.Deserialize
                Expect.isOk result "should be ok"
                let actual = result |> Result.get
                Expect.equal actual empty "should be equal"

            testCase "deserialize hotel state 1 - Error" <| fun _ ->
                let input = 
                    """
                        {"rooms":[],"bookings":[],"id":0 asdfasdflQQ
                    """.Trim()
                let result = Hotel.Deserialize input
                Expect.isError result "should be error"

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
                        {"rooms":[{"id":1,"description":null}],"bookings":[]}
                    """.Trim()
                Expect.equal expected actual "should be equal"

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
                let result =  Hotel.Deserialize input 
                Expect.isOk result "should be ok"
                let (Ok actual) = result
                Expect.equal expected actual "should be equal"
                
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
                        {"rooms":[{"id":111,"description":null},{"id":666,"description":{"Case":"Some","Fields":["hot room"]}}],"bookings":[]}
                    """.Trim()
                Expect.equal actual expected  "should be equal"

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
                let result =  Hotel.Deserialize input
                Expect.isOk result "should be ok"
                let (Ok actual) = result
                Expect.equal actual expected "should be equal"

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
                let result = Event.Deserialize input
                Expect.isOk result "should be ok"
                let actual = result |> Result.get
                Expect.equal actual expected "should be true"

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
                let result = Event.Deserialize input 
                Expect.isOk result "should be ok"
                let actual = result |> Result.get
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
                
            testCase "serialize AddBooking" <| fun _ ->
                let booking: Booking =
                    {
                        id = Guid.Parse("f1885f74-aba3-4de3-bd75-cc228078f74c") |> Some
                        roomId = 1
                        customerEmail = "email@you.me"
                        plannedCheckin = DateTime.Parse("2022-11-11 00:00:00")
                        plannedCheckout = DateTime.Parse("2022-11-12 00:00:00")
                    }
                let addBooking =
                    Command.AddBooking booking
                let actual = addBooking.Serialize()
                let expected = 
                    """
                        {"Case":"AddBooking","Fields":[{"id":{"Case":"Some","Fields":["f1885f74-aba3-4de3-bd75-cc228078f74c"]},"roomId":1,"customerEmail":"email@you.me","plannedCheckin":"2022-11-11T00:00:00","plannedCheckout":"2022-11-12T00:00:00"}]}
                    """.Trim()
                Expect.equal actual expected "should be true"
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

                let result =  hotel.Evolve [sEvent]
                Expect.isOk result "should be ok"
                let hotel' = result |> Result.get

                let expected = {
                    hotel with
                        rooms = [room1]
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
                let result = hotel.Evolve [sEvent; sEvent2]
                Expect.isOk result "should be ok"
                let hotel' = result |> Result.get

                let expected = {
                    hotel with
                        rooms = [room2; room1]
                }

                Expect.equal hotel' expected "should be true" 

            testCase "add room with the same id twice - Error" <| fun _ ->
                let hotel = Hotel.GetEmpty()
                let sEvent = 
                    """
                        {"Case":"RoomAdded","Fields":[{"id":1,"description":null}]}
                    """
                let sEvent2 = 
                    """ 
                        {"Case":"RoomAdded","Fields":[{"id":1,"description":{"Case":"Some","Fields":["hot room"]}}]}
                    """
                let result = hotel.Evolve [sEvent; sEvent2]
                Expect.isError result "should be error"
                let (Error error) =  result
                Expect.equal error "a room with number 1 already exists" "should be equal"

            testCase "add two rooms wrong json format - Error" <| fun _ ->
                let hotel = Hotel.GetEmpty()
                let sEvent = 
                    """ 
                        {"CaseWWWW":"RoomAdded","Fields":[{"id":1,"description":{"Case":"Some","Fields":["hot room"]}}]}
                    """
                let result =  hotel.Evolve [sEvent]
                Expect.isError result "should be error"
                let (Error error) =  hotel.Evolve [sEvent]
                Expect.isTrue (error.Contains("Unexpected property 'CaseWWWW'")) "should be true"
        ]
    [<Tests>]
    let CommandSequenceToEvents =
        testList "commandSequenceToEventSequence" [
            testCase "when the list contains a singl command that can be executed returning ok then the corresponding ok event is retured" <| fun _ ->
                let hotel = Hotel.GetEmpty()
                let room: Room = 
                    {
                        id = 1
                        description = None
                    }
                let addRoom =
                    Command.AddRoom room
                let commands = [addRoom]
                let result =  Command.Executes commands hotel
                Expect.isOk result "should be ok"
                let (Ok actual) = result
                let expected = [(Event.RoomAdded room)]
                Expect.equal expected actual "should be equal"
        
            testCase "add existing room command - KO" <| fun _ ->
                let room: Room = 
                    {
                        id = 1
                        description = None
                    }
                let hotel =
                    {
                        Hotel.GetEmpty() with
                            rooms = [room]
                    }
                let addRoom =
                    Command.AddRoom room
                let commands = [addRoom]
                let result = Command.Executes commands hotel
                Expect.isError result "should be ok"
                let (Error actual) = result
                Expect.equal actual "a room with number 1 already exists" "should be true"

            testCase "add existing room, two commands, error from second command - KO" <| fun _ ->
                let room1: Room = 
                    {
                        id = 1
                        description = None
                    }
                let room2: Room = 
                    {
                        id = 2
                        description = None
                    }
                let hotel =
                    {
                        Hotel.GetEmpty() with
                            rooms = [room2]
                    }
                let addRoom1 =
                    Command.AddRoom room1
                let addRoom2 =
                    Command.AddRoom room2
                let commands = [addRoom1; addRoom2]
                let result =  Command.Executes commands hotel
                Expect.isError result "should be error"
                let (Error actual) = result
                Expect.equal actual "a room with number 2 already exists" "should be true"
        ]
