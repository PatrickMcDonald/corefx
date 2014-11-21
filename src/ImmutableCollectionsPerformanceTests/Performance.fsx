#r "C:/Program Files/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.5/Facades/System.Runtime.dll"
#r "../../bin/Release/System.Collections.Immutable/System.Collections.Immutable.dll"

open System.Collections.Generic
open System.Collections.Immutable

let inline verifyListsEqual (expected: IList<_>) (actual: IList<_>) =
    if expected.Count <> actual.Count then
        failwithf "List lengths differ: expected %d, actual %d" expected.Count actual.Count

    for i in 0 .. expected.Count-1 do
        if not (EqualityComparer<_>.Default.Equals(expected.[i], actual.[i])) then
            failwithf "List differ at position %d: expected %A, actual %A" i expected.[i] actual.[i]

let benchmark label iterations f =
    let collectGarbage() =
        System.GC.Collect()
        System.GC.WaitForPendingFinalizers()
        System.GC.Collect()

    // Warmup
    f() |> ignore

    collectGarbage()

    let sw = System.Diagnostics.Stopwatch.StartNew()

    for i in 1 .. iterations do
        f() |> ignore

    collectGarbage()

    let elapsed = sw.Elapsed

    printfn "%s; iterations: %d; Time: %O" label iterations elapsed

    elapsed.TotalMilliseconds

let compare iterations optimized original =
    let t1 = benchmark "Optimized" iterations optimized
    let t2 = benchmark "Original" iterations original

    printfn "%f %f%%" (t2 / t1) ((t1 - t2) * 100.0 / t2)
    printfn ""

let createRandom() =
    let seed = int System.DateTime.Now.Ticks
    printfn "Using random seed %d" seed
    System.Random(seed)

let benchmarkFindAll() =
    printfn("Benchmarking FindAll:")
    let random = createRandom()

    let items = ImmutableList<int>.Empty.AddRange(Seq.init 100000 (fun _ -> random.Next(100)))
    let p = System.Predicate<int>(fun x -> x < 50)

    verifyListsEqual (items.FindAllOriginal(p)) (items.FindAll(p))

    compare 20 (fun () -> items.FindAll(p)) (fun () -> items.FindAllOriginal(p))

let benchmarkConvertAll() =
    printfn "Benchmarking ConvertAll:"
    let random = createRandom()

    let items = ImmutableList<int>.Empty.AddRange(Seq.init 100000 (fun _ -> random.Next(100)))
    let p = System.Func<int, bool>(fun x -> x < 50)

    verifyListsEqual (items.ConvertAllOriginal(p)) (items.ConvertAll(p))

    compare 20 (fun () -> items.ConvertAll(p)) (fun () -> items.ConvertAllOriginal(p))

    printfn ""

let benchmarkAddRange() =
    printfn "Benchmarking AddRange:"
    let starterList = ImmutableList.CreateRange(seq { 5 .. 104 })
    let expected = ImmutableList.CreateRange(seq { 5 .. 154 })
    let diff = [| 105 .. 154 |]
    let diffList = ImmutableList.CreateRange(diff)

    verifyListsEqual (starterList.AddRangeOriginal(diff)) (starterList.AddRange(diff))
    verifyListsEqual (starterList.AddRangeOriginal(diffList)) (starterList.AddRange(diffList))

    compare 100000 (fun () -> starterList.AddRange(diff)) (fun () -> starterList.AddRangeOriginal(diff))

    compare 100000 (fun () -> starterList.AddRange(diffList)) (fun () -> starterList.AddRangeOriginal(diffList))

    printfn ""

benchmarkFindAll()
benchmarkConvertAll()
benchmarkAddRange()

