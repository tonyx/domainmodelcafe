namespace hotelmodeling

open hotelmodeling
open hotelmodeling.Domain
open hotelmodeling.CommandEvents
open hotelmodeling.HotelSerialization
open hotelmodeling.MiscUtils
open FSharp.Core
open hotelmodeling.App
open FSharp.Core.Result
open Microsoft.FSharp.Core.Result
open Microsoft.FSharp.Core
open FSharp
open System
open Expecto
open FSharpPlus
open FSharpPlus.Data
open Npgsql.FSharp

module DbTests =
    open Db

    let deleteAllevents () =
        let _ =
            TPConnectionString 
            |> Sql.connect
            |> Sql.query "DELETE from events"
            |> Sql.executeNonQuery
        ()

    
