
namespace hostelmodeling

open hotelmodeling.Domain
open FSharp.Data
open Newtonsoft
open Newtonsoft.Json

open System.IO
open System.Text

module Serialization = 
    type State with
        member this.Serialize() =
            JsonConvert.SerializeObject this
        static member Deserialize(x: string) =
            JsonConvert.DeserializeObject<State> x
    type Room with
        member this.Serialize() =
            JsonConvert.SerializeObject this
        static member Deserialize(x: string) =
            JsonConvert.DeserializeObject<Room> x
    type Booking with
        member this.Serialize() =
            JsonConvert.SerializeObject this
        static member Deserialize(x: string) =
            JsonConvert.DeserializeObject<Booking> x
    type UnionEvent with
        member this.Serialize() =
            JsonConvert.SerializeObject this
        static member Deserialize(x: string) =
            JsonConvert.DeserializeObject<UnionEvent> x