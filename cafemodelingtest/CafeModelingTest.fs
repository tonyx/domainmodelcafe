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
open FSharpPlus
open FSharpPlus.Data

module Shared =
    let spaghetti = { name = "spaghetti" }
    let table1 = { id = 1; orderItems = [] }
    let tableAdded t: Event =
        fun (x: Cafe) -> x.AddTable t
    let foodAdded f: Event =
        fun (x: Cafe) -> x.AddFood f

open Shared
open Utils

[<Tests>]
let domainObjectsTests =
    testList
        "domain objects tests"
        [ 
            testCase "add a table to empty cafe - OK"
                <| fun _ ->
                    let initCafe = Cafe.GetEmpty()
                    let nextCafe = initCafe.AddTable table1
                    let expected =
                        { 
                            tables = [ table1 ]
                            availableFoods = [] 
                        } 
                        |> Ok
                    Expect.equal nextCafe expected "should be equal"

            testCase "add already existing table - Error "
                <| fun _ ->
                    let initCafe = { Cafe.GetEmpty() with tables = [ table1 ] }
                    let (Error result) = initCafe.AddTable table1
                    Expect.equal result "table 1 already exists" "should be equal"

            testCase "add food to empty cafe - OK"
                <| fun _ ->
                    let initCafe = Cafe.GetEmpty()
                    let (Ok actual) = initCafe.AddFood spaghetti
                    let expected =
                        { 
                            tables = []
                            availableFoods = [ spaghetti ] 
                        }
                    Expect.equal actual expected "should be equal"

            testCase "can't add food that already exists - OK"
                <| fun _ ->
                    let initCafe = { Cafe.GetEmpty() with availableFoods = [ spaghetti ] }
                    let (Error result) = initCafe.AddFood spaghetti
                    Expect.equal result "there is already food named spaghetti" "should be equal" 
        ]

[<Tests>]
let domainEntitiesTests =
    testList
        "entity comparisons"
        [ 
            testCase "foods with the same name are the same food"
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
                Expect.notEqual table1a table1b "should be not equal"

            testCase "add food to orderitems - Ok"
            <| fun _ ->
                let result = table1.AddOrderItem spaghetti
                Expect.equal (result.orderItems.Length) 1 "should be equal"
                Expect.equal (result.orderItems) [ spaghetti ] "should be equal"

            testCase "add orderitem - Ok"
            <| fun _ ->
                let result = table1.AddOrderItem spaghetti
                Expect.equal (result.orderItems.Length) 1 "should be equal"
                Expect.equal (result.orderItems) [ spaghetti ] "should be equal"

            testCase "add orderitem 2 - Ok"
            <| fun _ ->
                let cafe =
                    { 
                        tables = [ table1 ]
                        availableFoods = [ spaghetti ] 
                    }
                let expectedTable1 = { id = 1; orderItems = [ spaghetti ] }
                let expectedCafe =
                    { 
                        tables = [ expectedTable1 ]
                        availableFoods = [ spaghetti ] 
                    }
                let (Ok result) = cafe.AddOrderItem 1 spaghetti
                Expect.equal result expectedCafe "should be equal"

            testCase "add unexisting food to orderitem - Error"
            <| fun _ ->
                let cafe =
                    { 
                        tables = [ table1 ]
                        availableFoods = [ spaghetti ] 
                    }
                let steack = { name = "steack" }
                let (Error result) = cafe.AddOrderItem 1 steack
                Expect.equal "unavailable food steack" result "should be equal"

            testCase "add food to unexisting table - Error"
            <| fun _ ->
                let cafe =
                    { 
                        tables = [ table1 ]
                        availableFoods = [ spaghetti ] 
                    }
                let (Error result) = cafe.AddOrderItem 2 spaghetti
                Expect.equal result "unexisting table 2" "should be equal"

            testCase "add already existing food - Error"
            <| fun _ ->
                let cafe =
                    { 
                        tables = []
                        availableFoods = [ spaghetti ] }

                let (Error result) = cafe.AddFood spaghetti
                Expect.equal "there is already food named spaghetti" result "should be equal"

            testCase "add table - Ok"
            <| fun _ ->
                let cafe = Cafe.GetEmpty()
                let table = { id = 1; orderItems = [] }
                let (Ok cafe') = cafe.AddTable table
                Expect.equal (cafe'.tables.Length) 1 "should be equal"
                Expect.isTrue true "true" ]

[<Tests>]
let EventsTests =
    testList
        "domain events tests"
        [ 
            testCase "add table event - Ok"
            <| fun _ ->
                let cafe = Cafe.GetEmpty()
                let event = tableAdded table1
                let (Ok newCafe) = cafe |> event
                let expected =
                    { 
                        tables = [ table1 ]
                        availableFoods = [] 
                    }
                Expect.equal newCafe expected "should be equal"

            testCase "add food event - Ok"
            <| fun _ ->
                let cafe = Cafe.GetEmpty()
                let event = foodAdded spaghetti
                let (Ok cafe') = cafe |> event
                let expected: Cafe = { cafe with availableFoods = [ spaghetti ] }
                Expect.equal cafe' expected "should be equal"

            testCase "add different tables, process event list - OK"
            <| fun _ ->
                let cafe = Cafe.GetEmpty()
                let table2 = { id = 2; orderItems = [] }
                let table2Added = table2 |> tableAdded
                let table1Added = table1 |> tableAdded
                let events = [ table2Added; table1Added ] |> NonEmptyList.ofList
                let (Ok cafe') = events |> cafe.ProcessEvents
                let expected = { cafe with tables = [ table1; table2 ] }
                Expect.equal cafe' expected "should be equal"

            testCase "add table and food - Ok"
            <| fun _ ->
                let cafe = Cafe.GetEmpty()
                let table1Added = table1 |> tableAdded
                let spaghettiAdded = spaghetti |> foodAdded
                let events = [ table1Added; spaghettiAdded ] |> NonEmptyList.ofList
                let (Ok cafe') = events |> cafe.ProcessEvents

                let expected =
                    { 
                        tables = [ table1 ]
                        availableFoods = [ spaghetti ] 
                    }
                Expect.equal cafe' expected "should be equal"

            testCase "add already existing table - Error"
            <| fun _ ->
                let cafe = { Cafe.GetEmpty() with tables = [ table1 ] }
                let table1Added = table1 |> tableAdded
                let events = [ table1Added ] |> NonEmptyList.ofList
                let (Error error) = events |> cafe.ProcessEvents
                Expect.equal error "table 1 already exists" "should be equal"

            testCase "add already existing table 2 - Error"
            <| fun _ ->
                let cafe =
                    { 
                        tables = [ table1 ]
                        availableFoods = [] 
                    }
                let table1Added: Event = tableAdded table1
                let food1Added: Event = foodAdded spaghetti
                let events = [ table1Added; food1Added ] |> NonEmptyList.ofList
                let (Error error) = events |> cafe.ProcessEvents
                Expect.equal error "table 1 already exists" "should be equal"

            testCase "add already existing food - Error"
            <| fun _ ->
                let cafe = { Cafe.GetEmpty() with availableFoods = [ spaghetti ] }
                let table1Added: Event = tableAdded table1
                let spaghettiAdded: Event = foodAdded spaghetti
                let events = [ table1Added; spaghettiAdded ] |> NonEmptyList.ofList
                let (Error error) = events |> cafe.ProcessEvents
                Expect.equal error "there is already food named spaghetti" "should be equal"

            testCase "add orderitem  - Ok"
            <| fun _ ->
                let cafe =
                    { 
                        tables = [ table1 ]
                        availableFoods = [ spaghetti ] 
                    }
                let orderItemAdded tableId food =
                    fun (w: Cafe) -> w.AddOrderItem tableId food
                let orderItemAddedTable1: Event = orderItemAdded 1 spaghetti
                let events = [ orderItemAddedTable1 ] |> NonEmptyList.ofList
                let (Ok cafe') = events |> cafe.ProcessEvents
                let expectedTable = { table1 with orderItems = [ spaghetti ] }
                let expectedCafe =
                    { 
                        tables = [ expectedTable ]
                        availableFoods = [ spaghetti ] 
                    }
                Expect.equal expectedCafe cafe' "should be equal" ]

[<Tests>]
let CommandsTests =
    testList
        "command tests"
        [ 
            testCase "add table command returns tableAdded event - Ok"
            <| fun _ ->
                let cafe = Cafe.GetEmpty()
                let addTable1Command: Command =
                    let table1Added: Event = tableAdded table1
                    fun _ -> ([table1Added] |> NonEmptyList.ofList) |> Ok

                let (Ok events) = addTable1Command |> cafe.ProcessCommand
                let (Ok cafe') = cafe.ProcessEvents events
                let expected: Cafe = { cafe with tables = [ table1 ] }
                Expect.equal cafe' expected "should be equal"

            testCase "add table command returns tableAdded event 2 - Ok"
            <| fun _ ->
                let cafe = Cafe.GetEmpty()
                let (Ok events) = Utils.makeCommand (AddTable table1) |> cafe.ProcessCommand
                let (Ok newCafe) = cafe.ProcessEvents events
                let expected: Cafe = { cafe with tables = [ table1 ] }
                Expect.equal newCafe expected "should be equal"

            testCase "add food command returns foodAdded event 2 - Ok"
            <| fun _ ->
                let cafe = Cafe.GetEmpty()
                let (Ok events) = Utils.makeCommand (AddFood spaghetti) |> cafe.ProcessCommand
                let (Ok cafe') = cafe.ProcessEvents events
                let expected: Cafe = { cafe with availableFoods = [ spaghetti ] }
                Expect.equal cafe' expected "should be equal"

            testCase "add table command returns tableAdded event 3 - Ok"
            <| fun _ ->
                let cafe = Cafe.GetEmpty()
                let command: Command = Utils.makeCommand (AddTable table1)
                let (Ok events) = command |> cafe.ProcessCommand
                let (Ok cafe') = cafe.ProcessEvents events
                let expected: Cafe = {cafe with tables = [ table1 ] }
                Expect.equal cafe' expected "should be equal"

            testCase "add table command returns Error if event can't add table - Error"
            <| fun _ ->
                let cafe = 
                    {
                        Cafe.GetEmpty()
                            with tables = [table1]
                    }
                let command: Command = Utils.makeCommand (AddTable table1)
                let (Error error) = command |> cafe.ProcessCommand
                Expect.equal "command error: table 1 already exists" error "should be equal"

            testCase "add food command returns Error if event can't add the food - Error"
            <| fun _ ->
                let cafe = 
                    {
                        Cafe.GetEmpty()
                            with availableFoods = [spaghetti]
                    }
                let command: Command = Utils.makeCommand (AddFood spaghetti)
                let (Error error) = command |> cafe.ProcessCommand
                Expect.equal "command error: there is already food named spaghetti" error "should be equal"
        ]
