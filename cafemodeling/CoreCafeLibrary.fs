
namespace cafemodeling

open FSharp.Core
open FSharpPlus
open FSharpPlus.Data

module rec modeling =
    type Food = 
        {
            name: string
        }

    [<CustomEquality; NoComparison>]
    type Table =
        {
            id: int
            orderItems: List<Food>
        }
        with 
            override this.Equals(obj) =
                match obj with
                    | :? Table as t -> this.id = t.id
                    | _ -> false
            override this.GetHashCode() =
                this.id

            member this.AddOrderItem (dish: Food) =
                {
                    this
                        with 
                            orderItems = dish::this.orderItems
                }

    type Event = World -> Result<World, string>
    type Command = World -> Result<NonEmptyList<Event>, string>
    type World =
        {
            tables: List<Table>
            availableFoods: List<Food>
        }
        with 
            static member GetEmpty() =
                {
                    tables = []
                    availableFoods = []
                }
            member this.AddTable table: Result<World, string> =
                if ((this.tables) |> List.contains table) then
                    sprintf "table %d already exists" table.id |> Error
                else
                    {
                        this with
                            tables = table::this.tables
                    } 
                    |> Ok
            member this.AddFood food =
                if ((this.availableFoods) |> List.contains food) then
                    sprintf "there is already food named %s" food.name 
                    |> Error
                else
                    {
                        this with
                            availableFoods = food::this.availableFoods
                    } 
                    |> Ok
                    
            member this.AddOrderItem tableId food =       
                let tableLookup = 
                    this.tables 
                    |> List.tryFind (fun x -> x.id = tableId)
                let foodLookup = 
                    this.availableFoods 
                    |> List.tryFind (fun x -> x = food)

                match tableLookup, foodLookup with
                    | None, None -> Error (sprintf "unavailable food %s; unexisting table %d" (food.name) tableId)
                    | _, None -> Error (sprintf "unavailable food %s" (food.name))
                    | None, _ -> Error (sprintf "unexisting table %d" (tableId))
                    | Some table, _ ->
                        {
                            this
                                with
                                    tables = 
                                        table.AddOrderItem food::
                                            (
                                                this.tables 
                                                |> List.filter (fun x -> x <> table)
                                            )
                        } |> Ok

            member this.ProcessEvents (events: NonEmptyList<Event>) =
                events |> NonEmptyList.toList
                |> List.fold 
                    (fun x f -> 
                        match x with
                        | Ok x1 -> f x1
                        | Error x -> Error x
                    ) (this |> Ok)

            member this.ProcessCommand (command: Command) =
                let res = 
                    match command this with
                    | Ok x -> 
                        match this.ProcessEvents x with
                        | Ok _ -> Ok x
                        | Error e -> Error (sprintf "command error: %s" e)
                    | Error x ->  Error (sprintf "command error: %s" x)
                res

module Utils =
    open modeling
    type CommandMaker =
        | AddTable of Table 
        | AddFood of Food

    let makeCommand commandMaker: Command =
        match commandMaker with
            | AddTable t ->
                let addTable: Event=
                    let tableAdded t =
                        fun (x: World) -> x.AddTable t
                    tableAdded t
                fun _ -> [addTable] |> NonEmptyList.ofList |> Ok

            | AddFood f ->
                let addFood: Event=
                    let foodAdded f =
                        fun (x: World) -> x.AddFood f
                    foodAdded f
                fun _ -> [addFood] |> NonEmptyList.ofList |> Ok
