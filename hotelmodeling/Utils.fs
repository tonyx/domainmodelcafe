namespace hotelmodeling

open FSharp.Data
open Newtonsoft
open Newtonsoft.Json
open FSharpPlus
open FSharpPlus.Operators

open System.IO
open System.Text
open System

module Utils = 
    type CeErrorBuilder()  =
        member this.Bind(x, f) =
            match x with
            | Error x -> Error x
            | Ok x -> f x 

        member this.Return(x) =
            x |> Ok
