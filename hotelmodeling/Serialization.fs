
namespace hotelmodeling

open hotelmodeling.Domain
open hotelmodeling.CommandEvents
open hotelmodeling.MiscUtils
open FSharp.Data
open Newtonsoft
open Newtonsoft.Json
open FSharpPlus
open FSharpPlus.Operators

open System.IO
open System.Text
open System

module HotelSerialization = 
    type Hotel with
        member this.Serialize() =
            JsonConvert.SerializeObject this
        static member Deserialize(x: string) =
            try 
                let result = JsonConvert.DeserializeObject<Hotel> x 
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

    type Hotel with 
        member this.Evolve serEvents =
            match serEvents |> catchErrors Event.Deserialize
                with
                | Error x -> Error x
                | Ok x -> x |> this.Evolve
