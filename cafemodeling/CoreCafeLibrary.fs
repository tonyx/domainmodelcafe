
namespace cafemodeling

open FSharp.Core
open FSharpPlus
open FSharpPlus.Data

module modeling =
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

            member this.AddOrderItem (food: Food) =
                {
                    this
                        with 
                            orderItems = food::this.orderItems
                }

    type Cafe =
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
            member this.AddTable table: Result<Cafe, string> =
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

            member this.Evolve events =
                events |> NonEmptyList.toList
                |> List.fold 
                    (fun x f -> 
                        match x with
                        | Ok x1 -> f x1
                        | Error x -> Error x
                    ) (this |> Ok)

            member this.Interpret command =
                match command this with
                | Ok x -> 
                    match this.Evolve x with
                    | Ok _ -> Ok x
                    | Error e -> Error (sprintf "command error: %s" e)
                | Error x ->  Error (sprintf "command error: %s" x)

    type Event = Cafe -> Result<Cafe, string>
    type Command = Cafe -> Result<NonEmptyList<Event>, string>

module Utils =
    open modeling
    type CommandMaker =
        | AddTable of Table 
        | AddFood of Food

    let makeCommand commandMaker: Command =
        match commandMaker with
            | AddTable t ->
                fun _ -> 
                    [fun (x: Cafe) -> x.AddTable t] 
                    |> NonEmptyList.ofList 
                    |> Ok
            | AddFood f ->
                fun _ -> 
                    [fun (x: Cafe) -> x.AddFood f] 
                    |> NonEmptyList.ofList 
                    |> Ok
