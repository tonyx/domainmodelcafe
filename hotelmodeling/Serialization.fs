
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
                | _ as ex -> Error (ex.ToString())

    type Room with
        member this.Serialize() =
            JsonConvert.SerializeObject this
        static member Deserialize(x: string) =
            try
                let result = JsonConvert.DeserializeObject<Room> x
                result |> Ok
            with
                | _ as ex -> Error (ex.ToString())

    type Booking with
        member this.Serialize() =
            JsonConvert.SerializeObject this
        static member Deserialize(x: string) =
            try 
                let result = JsonConvert.DeserializeObject<Booking> x
                result |> Ok
            with
                | _ as ex -> Error (ex.ToString())

    type Event with
        member this.Serialize() =
            JsonConvert.SerializeObject this
        static member Deserialize(x: string) : Result<Event, string> =
            try
                let result = JsonConvert.DeserializeObject<Event> x
                result |> Ok
            with
                | _ as ex -> Error (ex.ToString())

    type Command with
        member this.Serialize() =
            JsonConvert.SerializeObject this
        static member Deserialize(x: string) : Result<Command, string> =
            try
                let result = JsonConvert.DeserializeObject<Command> x
                result |> Ok
            with
                | _ as ex -> Error (ex.ToString())

    let catchErrors f l =
        let (okList, errors) =
            l  
            |> List.map f 
            |> Result.partition
        if (errors.Length > 0) then
            Result.Error (errors.Head)
        else
            okList |> Result.Ok

    type State with 
        member this.SEvolve serEvents =
            match serEvents |> catchErrors Event.Deserialize
                with
                | Error x -> Error x
                | Ok x -> x |> this.Evolve
