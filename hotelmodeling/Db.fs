module hotelmodeling.Db
open FSharp.Data.Sql
open System
open System.Data
open System.Globalization
open System.Data.Common
open Npgsql
open NpgsqlTypes
open Npgsql.Util
open Npgsql.Internal
open Npgsql.FSharp
open CommonExtensionsAndTypesForNpgsqlFSharp
open Npgsql.Util
open Npgsql.PostgresTypes

[<Literal>]
let TPConnectionString = 
    "Server=127.0.0.1;"+ 
    "Database=dmhotel;" + 
    "User Id=hotel;"+ 
    "Password=1234;"

let [<Literal>] dbVendor = Common.DatabaseProviderTypes.POSTGRESQL
let [<Literal>] indivAmount = 1000 

let initState = "{\"rooms\":[],\"bookings\":[]}"

type Sql =
    SqlDataProvider< 
        dbVendor,
        TPConnectionString,
        "",        
        "",
        indivAmount,
        UseOptionTypes=Common.NullableColumnType.OPTION>

type DbContext = Sql.dataContext
type Events = DbContext.``public.eventsEntity``

let getContext() = Sql.GetDataContext(TPConnectionString)

let getAllEvents (ctx: DbContext ) =
    query {
        for event in ctx.Public.Events do
            sortBy event.Id
            select event
    } |> Seq.toList

let addEvent event (ctx: DbContext) =
    try 
        let _ = ctx.Public.Events.``Create(event, timestamp)``(event, System.DateTime.Now)
        ctx.SubmitUpdates()
        () |> Ok
    with
        | _ as ex -> (ex.ToString()) |> Error

let addEvents (events: List<string>) (ctx: DbContext) =
    try 
        let _ = 
            events 
            |> List.map
                (fun x -> (ctx.Public.Events.``Create(event, timestamp)``(x, System.DateTime.Now)))
        ctx.SubmitUpdates()
        () |> Ok
    with
        | _ as ex -> (ex.ToString()) |> Error

let getLastSnapshot (ctx: DbContext) =
    let lastSnapshot =
        query {
            for event in ctx.Public.Events do
                sortByDescending event.Id
                where event.Snapshot.IsSome
                select (event.Id, event.Snapshot.Value)
        } |> Seq.tryHead
    match lastSnapshot with
        Some x ->  x
        | _ ->  (0, initState)
    
let setSnapshot id snapshot (ctx: DbContext) =
    try
        let event = 
            query {
                for event in ctx.Public.Events do 
                    where (event.Id = id)
                    select event
            } |> Seq.tryHead
        match event with
            | Some x -> 
                x.Snapshot <- snapshot |> Some
                ctx.SubmitUpdates()
            | None -> ()
        () |> Ok
    with
        | _ as ex ->  (ex.ToString()) |> Error

let getEventsAfterId id (ctx: DbContext) =
    query {
        for event in ctx.Public.Events do
            where (event.Id > id)
            sortBy event.Id
            select (event.Id, event.Event)
    } |> Seq.toList