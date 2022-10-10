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
let [<Literal>] resPath = "" 
let [<Literal>] indivAmount = 1000 
let [<Literal>] useOptTypes  = false

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

let addEvent cont (ctx: DbContext) =
    try 
        let event = ctx.Public.Events.``Create(event, timestamp)``(cont, System.DateTime.Now)
        ctx.SubmitUpdates()
        event |> Result.Ok
    with
        | _ as ex -> Error (ex.ToString())

let addEventWithSnapshot cont snapshot (ctx: DbContext) =
    try 
        let event = ctx.Public.Events.``Create(event, timestamp)``(cont, System.DateTime.Now)
        event.Snapshot <- snapshot |> Some
        ctx.SubmitUpdates()
        event |> Result.Ok
    with
        | _ as ex -> Error (ex.ToString())
        
let getLatestSnapshotWithId (ctx: DbContext)  =
    query {
        for event in ctx.Public.Events do
            sortByDescending event.Id
            where event.Snapshot.IsSome
            select (event.Id, event.Snapshot.Value)
    } |> Seq.tryHead

// let getCurrentState (ctx: DbContext) =
//     let (_, s) = getLatestSnapshotWithId ctx
//     match s with

let getEventsAfterId id (ctx: DbContext) =
    query {
        for event in ctx.Public.Events do
            where (event.Id > id)
            sortBy event.Id
            select (event.Id, event.Event)
    } |> Seq.toList