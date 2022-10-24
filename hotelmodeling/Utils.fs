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
            | Error x1 -> Error x1
            | Ok x1 -> f x1 

        member this.MergeSources (x, y) =
            match x, y with
                | Error x1, _ -> Error x1
                | _, Error y1 -> Error y1
                | Ok x1, Ok y2 -> (x1, y2) |> Ok

        member this.Return(x) =
            x |> Ok
