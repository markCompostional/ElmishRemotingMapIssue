namespace Shared

type Counter = { Value : int }

module Route =
    /// Defines how routes are generated on server and mapped from client
    let builder typeName methodName =
        sprintf "/api/%s/%s" typeName methodName

// this succeeds
type MyMapWorks = Map<int,int>

module MyMapWorks =
    let value = Map.ofList [1,1]
    let isEmpty (x: MyMapWorks) = x.IsEmpty
    let count (x: MyMapWorks) = x.Count

// this fails with SetMap
type MyMapFails = Map<int * int,int>

module MyMapFails =
    let value = Map.ofList [(1,1),1]
    let isEmpty (x: MyMapFails) = x.IsEmpty
    let count (x: MyMapFails) = x.Count

/// A type that specifies the communication protocol between client and server
/// to learn more, read the docs at https://zaid-ajaj.github.io/Fable.Remoting/src/basics.html
type ICounterApi =
    { initialCounter : unit -> Async<Counter>
      getMap : unit -> Async<MyMapFails>
      setMap : Map<int * int,int> -> Async<bool> // this one fails
      getMap2 : unit -> Async<MyMapWorks>
      setMap2 : MyMapWorks -> Async<bool> }

