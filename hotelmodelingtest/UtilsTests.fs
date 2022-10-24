namespace hotelmodeling

open Expecto
open System
open FSharp.Core
open FSharpPlus
open FSharpPlus.Data
open hotelmodeling.Utils

module UtilTests =
    let ceError =  CeErrorBuilder()
    [<Tests>]
    let ceTests =
        testList "ceTest" [
            testCase "ce test no binding to let!, no error" <| fun _ ->
                let anExpression =
                    ceError {
                        let first = "a"
                        return first
                    }
                let result = anExpression
                let (Ok res) = result
                Expect.equal res "a" "should be equal"

            testCase "ce test with a let binding" <| fun _ ->
                let anExpression =
                    ceError {
                        let! first = "a" |> Result.Ok
                        return first
                    }
                let result = anExpression
                let (Ok res) = result
                Expect.equal res "a" "should be equal"

            testCase "ce test with a normal binding and a let! binding. returns the second" <| fun _ ->
                let anExpression =
                    ceError {
                        let first = "a"
                        let! second = "b" |> Result.Ok
                        return second
                    }
                let result = anExpression
                let (Ok res) = result
                Expect.equal res "b" "should be equal"

            testCase "ce test with two let! bindings. Returns the first" <| fun _ ->
                let anExpression =
                    ceError {
                        let! first = "a" |> Result.Ok
                        let! second = "b" |> Result.Ok
                        return second
                    }
                let result = anExpression
                let (Ok res) = result
                Expect.equal res "b" "should be equal"

            testCase "ce test with thre let! bindings. Returns the third" <| fun _ ->
                let anExpression =
                    ceError {
                        let! first = "a" |> Result.Ok
                        let! second = "b" |> Result.Ok
                        let! third = "c" |> Result.Ok
                        return third
                    }
                let result = anExpression
                let (Ok res) = result
                Expect.equal res "c" "should be equal"

            testCase "ce test with three let! bindings. The first is error. Error is bound to the first" <| fun _ ->
                let anExpression =
                    ceError {
                        let! first = "a" |> Result.Error
                        let! second = "b" |> Result.Ok
                        let! third = "c" |> Result.Ok
                        let! fourth = "d" |> Result.Ok
                        return third
                    }
                let result = anExpression
                let (Error res) = result
                Expect.equal res "a" "should be equal"

            testCase "ce test with three let! bindings. the first two are errors. return the first" <| fun _ ->
                let anExpression =
                    ceError {
                        let! first = "a" |> Result.Error
                        let! second = "b" |> Result.Error
                        let! third = "c" |> Result.Error
                        let! fourth = "d" |> Result.Ok
                        return fourth
                    }
                let result = anExpression
                let (Error res) = result
                Expect.equal res "a" "should be equal"

            testCase "ce test with three let! bindings. the second is error" <| fun _ ->
                let anExpression =
                    ceError {
                        let! first = "a" |> Result.Ok
                        let! second = "b" |> Result.Error
                        let! third = "c" |> Result.Ok
                        return third
                    }
                let result = anExpression
                let (Error res) = result
                Expect.equal res "b" "should be equal"

            testCase "ce test with a let! and a normal assignment. return normal assignment - ok" <| fun _ ->
                let anExpression =
                    ceError {
                        let! first = "b" |> Result.Ok
                        let second = "a"
                        return second
                    }
                let result = anExpression |> Result.get
                Expect.equal result "a" "should be equal"

            testCase "ce test with two let! assignments. returns the second ok" <| fun _ ->
                let anExpression =
                    ceError {
                        let! first = "a" |> Result.Ok
                        let! second = "b" |> Result.Ok
                        return second
                    }
                let result = anExpression |> Result.get
                Expect.equal result "b" "should be equal"

            testCase "ce test with a let! and a normal assignment, return first assignment" <| fun _ ->
                let anExpression =
                    ceError {
                        let! first = "b" |> Result.Ok
                        let second = "a"
                        return first
                    }
                let result = anExpression |> Result.get
                Expect.equal result "b" "should be equal"
        ]
