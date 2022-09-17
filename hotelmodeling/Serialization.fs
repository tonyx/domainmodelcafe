
namespace hostelmodeling

open hotelmodeling.Domain
open hotelmodeling.MiscUtils
open FSharp.Data
open Newtonsoft
open Newtonsoft.Json
open FSharpPlus
open FSharpPlus.Operators

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
        static member Deserialize(x: string) : Result<UnionEvent, string> =
            try
                let result = JsonConvert.DeserializeObject<UnionEvent> x
                result |> Ok
            with
                | :? Newtonsoft.Json.JsonReaderException as ex -> Error (ex.ToString())

    let separateErrors events =
        let (okList, errors) =
            events  
            |> List.map UnionEvent.Deserialize 
            |> Result.partition
        if (errors.Length > 0) then
            Result.Error (errors.Head)
        else
            okList |> Result.Ok

    type State with 
        member this.SUEvolve serEvents =
            let sEvents =
                serEvents 
                |> separateErrors
            match sEvents with
            | Error x -> Error x
            | Ok x -> x |> this.UEvolve



