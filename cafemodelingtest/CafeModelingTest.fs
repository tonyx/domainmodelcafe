module Tests

open cafemodeling
open cafemodeling.modeling
open FSharp.Core
open FSharp.Core.Result
open Microsoft.FSharp.Core.Result
open Microsoft.FSharp.Core
open FSharp
open System
open Expecto

module Shared =
    let spaghetti = { name = "spaghetti" }

    let table1 = { id = 1; orderItems = [] }

open Shared
open Utils

[<Tests>]
let domainObjectsTests =
    testList
        "domain objects tests"
        [ 
            testCase "add a table to empty world - OK"
                <| fun _ ->
                    let initWorld = World.GetEmpty()
                    let nextWorld = initWorld.AddTable table1

                    let expected =
                        { 
                            tables = [ table1 ]
                            availableFoods = [] 
                        } 
                        |> Ok

                    Expect.equal nextWorld expected "should be equal"

            testCase "add already existing table - Error "
                <| fun _ ->
                    let initWorld = { World.GetEmpty() with tables = [ table1 ] }
                    let result = initWorld.AddTable table1
                    let (Error result) = initWorld.AddTable table1
                    Expect.equal result "table 1 already exists" "should be equal"

            testCase "add food to empty world - OK"
                <| fun _ ->
                    let initWorld = World.GetEmpty()

                    let (Ok actual) = initWorld.AddFood spaghetti
                    let expected =
                        { 
                            tables = []
                            availableFoods = [ spaghetti ] 
                        }

                    Expect.equal actual expected "should be equal"

            testCase "can't add food that already exists - OK"
                <| fun _ ->
                    let initWorld = { World.GetEmpty() with availableFoods = [ spaghetti ] }

                    let (Error result) = initWorld.AddFood spaghetti

                    Expect.equal result "there is already food named spaghetti" "should be equal" 
        ]

[<Tests>]
let domainEntitiesTests =
    testList
        "entity comparisons"
        [ 
            testCase "foods with the same name are not the same food"
            <| fun _ ->
                let food1 = { name = "bread" }
                let food2 = { name = "bread" }
                Expect.equal food1 food2 "should be equal"

            testCase "foods with different names are different foods"
            <| fun _ ->
                let food1 = { name = "bread" }
                let food2 = { name = "macaroni" }
                Expect.notEqual food1 food2 "should be equal"

            testCase "tables with same id are the same table"
            <| fun _ ->
                let table1b =
                    { 
                        id = 1
                        orderItems = [ { name = "macaroni" } ] 
                    }
                Expect.equal table1 table1b "should be equal"

            testCase "tables with different ids are different table"
            <| fun _ ->
                let table1a =
                    { 
                        id = 1
                        orderItems = [ { name = "macaroni" } ] 
                    }

                let table1b =
                    { 
                        id = 2
                        orderItems = [ { name = "macaroni" } ] 
                    }
                Expect.notEqual table1a table1b "should be equal"

            testCase "add dish to orderitems - Ok"
            <| fun _ ->
                let result = table1.AddOrderItem spaghetti
                Expect.equal (result.orderItems.Length) 1 "should be equal"
                Expect.equal (result.orderItems) [ spaghetti ] "should be equal"

            testCase "add food to table orderitems - Ok"
            <| fun _ ->
                let world =
                    { 
                        tables = [ table1 ]
                        availableFoods = [ spaghetti ] 
                    }
                let result = table1.AddOrderItem spaghetti
                Expect.equal (result.orderItems.Length) 1 "should be equal"
                Expect.equal (result.orderItems) [ spaghetti ] "should be equal"

            testCase "add food to orderitems - Ok"
            <| fun _ ->
                let world =
                    { 
                        tables = [ table1 ]
                        availableFoods = [ spaghetti ] 
                    }

                let expectedTable1 = { id = 1; orderItems = [ spaghetti ] }

                let expectedWorld =
                    { 
                        tables = [ expectedTable1 ]
                        availableFoods = [ spaghetti ] 
                    }

                let (Ok result) = world.AddOrderItem 1 spaghetti
                Expect.equal result expectedWorld "should be equal"

            testCase "add unexisting dish to orderitem - Error"
            <| fun _ ->
                let world =
                    { 
                        tables = [ table1 ]
                        availableFoods = [ spaghetti ] 
                    }

                let steack = { name = "steack" }
                let (Error result) = world.AddOrderItem 1 steack
                Expect.equal "unavailable food steack" result "should be equal"

            testCase "add dish to unexisting table - Error"
            <| fun _ ->
                let world =
                    { 
                        tables = [ table1 ]
                        availableFoods = [ spaghetti ] 
                    }
                let (Error result) = world.AddOrderItem 2 spaghetti
                Expect.equal result "unexisting table 2" "should be equal"

            testCase "add alredy existing food - Error"
            <| fun _ ->
                let world =
                    { 
                        tables = []
                        availableFoods = [ spaghetti ] }

                let (Error result) = world.AddFood spaghetti
                Expect.equal "there is already food named spaghetti" result "should be equal"

            testCase "add table - Ok"
            <| fun _ ->
                let world = World.GetEmpty()
                let table = { id = 1; orderItems = [] }
                let (Ok result) = world.AddTable table
                Expect.isTrue true "true" ]

[<Tests>]
let EventsTests =
    testList
        "domain events tests"
        [ 
            testCase "add table event - Ok"
            <| fun _ ->
                let world = World.GetEmpty()

                let tableAdded t =
                    let res = fun (x: World) -> x.AddTable t
                    res

                let event = tableAdded table1
                let (Ok newWorld) = event world

                let expected =
                    { 
                        tables = [ table1 ]
                        availableFoods = [] 
                    }
                Expect.equal newWorld expected "should be equal"

            testCase "add food event - Ok"
            <| fun _ ->
                let world = World.GetEmpty()

                let foodAdded f =
                    let res = fun (x: World) -> x.AddFood f
                    res

                let event = foodAdded spaghetti
                let (Ok newWorld) = event world
                let expected: World = { world with availableFoods = [ spaghetti ] }
                Expect.equal newWorld expected "should be equal"

            testCase "add different tables, process event list - OK"
            <| fun _ ->
                let world = World.GetEmpty()
                let table2 = { id = 2; orderItems = [] }

                let tableAdded t =
                    let res = fun (x: World) -> x.AddTable t
                    res

                let table2Added = table2 |> tableAdded
                let table1Added = table1 |> tableAdded
                let events = [ table2Added; table1Added ]
                let newWorld = events |> world.ProcessEvents
                let (Ok newWorldVal) = newWorld
                let expected = { world with tables = [ table1; table2 ] }
                Expect.equal newWorldVal expected "should be equal"

            testCase "add table and food - Ok"
            <| fun _ ->
                let world = World.GetEmpty()

                let tableAdded t =
                    let res = fun (x: World) -> x.AddTable t
                    res

                let foodAdded (f: Food) =
                    let res = fun (x: World) -> x.AddFood f
                    res

                let table1Added = table1 |> tableAdded
                let spaghettiAdded = spaghetti |> foodAdded
                let events = [ table1Added; spaghettiAdded ]
                let newWorld = events |> world.ProcessEvents
                let (Ok newWorldVal) = newWorld

                let expected =
                    { 
                        tables = [ table1 ]
                        availableFoods = [ spaghetti ] 
                    }
                Expect.equal newWorldVal expected "should be equal"

            testCase "add already existing table   - Error"
            <| fun _ ->
                let world = { World.GetEmpty() with tables = [ table1 ] }

                let tableAdded t =
                    let res = fun (x: World) -> x.AddTable t
                    res

                let table1Added = table1 |> tableAdded
                let events = [ table1Added ]
                let newWorld = events |> world.ProcessEvents
                let (Error error) = newWorld
                Expect.equal error "table 1 already exists" "should be equal"

            testCase "add already existing table - Error"
            <| fun _ ->
                let world =
                    { 
                        tables = [ table1 ]
                        availableFoods = [] 
                    }

                let tableAdded t =
                    let res = fun (x: World) -> x.AddTable t
                    res

                let foodAdded f =
                    let res = fun (x: World) -> x.AddFood f
                    res

                let table1Added: Event = tableAdded table1
                let food1Added: Event = foodAdded spaghetti
                let events = [ table1Added; food1Added ]
                let newWorld = events |> world.ProcessEvents
                let (Error res) = newWorld
                Expect.equal res "table 1 already exists" "should be equal"

            testCase "add already existing food - Error"
            <| fun _ ->
                let world = { World.GetEmpty() with availableFoods = [ spaghetti ] }

                let tableAdded t =
                    let res = fun (x: World) -> x.AddTable t
                    res

                let foodAdded f =
                    let res = fun (x: World) -> x.AddFood f
                    res

                let table1Added: Event = tableAdded table1
                let spaghettiAdded: Event = foodAdded spaghetti
                let events = [ table1Added; spaghettiAdded ]
                let newWorld = events |> world.ProcessEvents
                let (Error res) = newWorld
                Expect.equal res "there is already food named spaghetti" "should be equal"

            testCase "add orderitem  - Ok"
            <| fun _ ->
                let world =
                    { 
                        tables = [ table1 ]
                        availableFoods = [ spaghetti ] 
                    }

                let orderItemAdded tableId food =
                    let res = fun (w: World) -> w.AddOrderItem tableId food
                    res

                let orderItemAddedSpaghettiTable1: Event = orderItemAdded 1 spaghetti

                let events = [ orderItemAddedSpaghettiTable1 ]
                let newWorld = events |> world.ProcessEvents

                let expectedTable = { table1 with orderItems = [ spaghetti ] }

                let expectedWorld =
                    { 
                        tables = [ expectedTable ]
                        availableFoods = [ spaghetti ] 
                    }

                let (Ok res) = newWorld
                Expect.equal expectedWorld res "should be equal" ]

[<Tests>]
let CommandsTests =
    testList
        "command tests"
        [ 
            testCase "add table command returns tableAdded event - Ok"
            <| fun _ ->
                let world = World.GetEmpty()
                let addTable1Command: Command =
                    let tableAdded t =
                        let res = fun (x: World) -> x.AddTable t
                        res

                    let table1Added: Event = tableAdded table1
                    fun x -> table1Added |> Ok

                let (Ok event) = addTable1Command |> world.ProcessCommand
                let (Ok newWorld) = world.ProcessEvents [ event ]
                let expected: World = { world with tables = [ table1 ] }
                Expect.equal newWorld expected "should be equal"

            testCase "add table command returns tableAdded event 2 - Ok"
            <| fun _ ->
                let world = World.GetEmpty()
                let addTable1Command: Command =
                    let tableAdded t =
                        let res = fun (x: World) -> x.AddTable t
                        res

                    let table1Added: Event = tableAdded table1
                    fun x -> table1Added |> Ok

                let (Ok event) = Utils.makeCommand (AddTable table1) |> world.ProcessCommand

                let (Ok newWorld) = world.ProcessEvents [ event ]
                let expected: World = { world with tables = [ table1 ] }
                Expect.equal newWorld expected "should be equal"

            testCase "add table command returns tableAdded event 3 - Ok"
            <| fun _ ->
                let world = World.GetEmpty()
                let addTable1Command: Command =
                    let tableAdded t =
                        let res = fun (x: World) -> x.AddTable t
                        res

                    let table1Added: Event = tableAdded table1
                    fun x -> table1Added |> Ok

                let command: Command = Utils.makeCommand (AddTable table1)
                let (Ok event) = command |> world.ProcessCommand

                let (Ok newWorld) = world.ProcessEvents [ event ]
                let expected: World = { world with tables = [ table1 ] }
                Expect.equal newWorld expected "should be equal"

            testCase "add table command returns Error if event can't add table - Error"
            <| fun _ ->
                let world = 
                    {
                        World.GetEmpty()
                            with tables = [table1]
                    }

                let command: Command = Utils.makeCommand (AddTable table1)
                let (Error error) = command |> world.ProcessCommand
                Expect.equal "command error: table 1 already exists" error "should be equal"

            testCase "add food command returns Error if event can't add the food - Error"
            <| fun _ ->
                let world = 
                    {
                        World.GetEmpty()
                            with availableFoods = [spaghetti]
                    }

                let command: Command = Utils.makeCommand (AddFood spaghetti)
                let (Error error) = command |> world.ProcessCommand
                Expect.equal "command error: there is already food named spaghetti" error "should be equal"
        ]
