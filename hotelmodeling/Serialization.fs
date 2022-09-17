
namespace hostelmodeling

open hotelmodeling.Domain
open hotelmodeling.MiscUtils
open FSharp.Data
open Newtonsoft
open Newtonsoft.Json

open System.IO
open System.Text

module DomainSerialization = 
    type State with
        member this.Serialize() =
            JsonConvert.SerializeObject this
        static member Deserialize(x: string) =
            try 
                let result = JsonConvert.DeserializeObject<State> x 
                result |> Ok
            with
                | :? Newtonsoft.Json.JsonReaderException as ex -> Error (ex.ToString())

    type Room with
        member this.Serialize() =
            JsonConvert.SerializeObject this
        static member Deserialize(x: string) =
            try
                let result = JsonConvert.DeserializeObject<Room> x
                result |> Ok
            with
                | :? Newtonsoft.Json.JsonReaderException as ex -> Error (ex.ToString())

    type Booking with
        member this.Serialize() =
            JsonConvert.SerializeObject this
        static member Deserialize(x: string) =
            try 
                let result = JsonConvert.DeserializeObject<Booking> x
                result |> Ok
            with
                | :? Newtonsoft.Json.JsonReaderException as ex -> Error (ex.ToString())

    type UnionEvent with
        member this.Serialize() =
            JsonConvert.SerializeObject this
        static member Deserialize(x: string) =
            try
                let result = JsonConvert.DeserializeObject<UnionEvent> x
                result |> Ok
            with
                | :? Newtonsoft.Json.JsonReaderException as ex -> Error (ex.ToString())

    type State with 
        member this.SUEvolve events =
            events 
            |> List.map UnionEvent.Deserialize
            |> List.map OkValue 
            |> this.UEvolve