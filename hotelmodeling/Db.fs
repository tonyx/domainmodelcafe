module hotelmodeling.Db
open FSharp.Data.Sql
open System
open System.Data
open System.Globalization
open System.Data.Common

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
        UseOptionTypes=Common.NullableColumnType.VALUE_OPTION>

type dbContext = Sql.dataContext
type SqlEvents = dbContext.``public.eventsEntity``