
namespace hostelmodeling

open Expecto
open hotelmodeling.Domain
open System
open FSharp.Core
open FSharpPlus.Data
open Newtonsoft
open Newtonsoft.Json
open hostelmodeling.Serialization

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

            testCase "deSerialize room" <| fun _ ->
                let input =
                    """
                        {"id":1,"description":null}
                    """.Trim()
                let room: Room = 
                    {
                        id = 1
                        description = None
                    }
                let deserialized = Room.Deserialize input 
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
                let deserialized = Room.Deserialize input
                Expect.equal deserialized room  "should be equal"

            testCase "serialize hotel state 1" <| fun _ ->
                let empty = State.GetEmpty()
                let actual = empty.Serialize()
                let expected = 
                    """
                        {"rooms":[],"bookings":[],"id":0}
                    """.Trim()
                Expect.equal actual expected "should be true"

            testCase "deserialize hotel state 1" <| fun _ ->
                let empty = State.GetEmpty()
                let input = 
                    """
                        {"rooms":[],"bookings":[],"id":0}
                    """.Trim()
                let actual = State.Deserialize input
                Expect.equal actual empty "should be true"

            testCase "serialize hotel state 2" <| fun _ ->
                let input: State = 
                    {
                        State.GetEmpty() with
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
                let expected: State = 
                    {
                        State.GetEmpty() with
                            rooms = [{
                                id = 1
                                description = None
                            }]
                    }
                let input = 
                    """
                        {"rooms":[{"id":1,"description":null}],"bookings":[],"id":0}
                    """.Trim()
                let actual = State.Deserialize input 
                Expect.equal expected actual "should be true"
                
            testCase "serialize hotel state 3" <| fun _ ->
                let input: State = 
                    {
                        State.GetEmpty() with
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
                let expected: State = 
                    {
                        State.GetEmpty() with
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
                let actual = State.Deserialize input
                Expect.equal actual expected "should be bla true"

            testCase "union event test serialize "  <| fun _ ->
                let room = {
                    id = 1
                    description = None
                }
                let input = UnionEvent.UAddRoom room
                let actual = input.Serialize() 
                let expected = 
                    """
                        {"Case":"UAddRoom","Fields":[{"id":1,"description":null}]}
                    """.Trim()
                Expect.equal actual expected "should be equal"

            testCase "union event test deserialize" <| fun _ ->
                let room = {
                    id = 1
                    description = None
                }
                let event = UnionEvent.UAddRoom room
                let input = 
                    """
                        {"Case":"UAddRoom","Fields":[{"id":1,"description":null}]}
                    """.Trim()
                let actual = UnionEvent.Deserialize input 
                Expect.equal actual event "shoud be equal"
        ]

