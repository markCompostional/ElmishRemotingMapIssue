module Client

open Elmish
open Elmish.React
open Fable.React
open Fable.React.Props
open Fulma
open Thoth.Json

open Shared

// The model holds data that you want to keep track of while the application is running
// in this case, we are keeping track of a counter
// we mark it as optional, because initially it will not be available from the client
// the initial value will be requested from server
type Model =
    { Counter: Counter option
      Map: MyMapFails option
      Map2: MyMapWorks option
      SetMapSuccess: bool }

// The Msg type defines what events/actions can occur while the application is running
// the state of the application changes *only* in reaction to these events
type Msg =
    | Increment
    | Decrement
    | GetMap
    | GotMap of MyMapFails
    | SetMapCommand of MyMapFails
    | IsSetMapEvent of bool
    | GetMap2
    | GotMap2 of MyMapWorks
    | SetMapCommand2 of MyMapWorks
    | IsSetMapEvent2 of bool
    | InitialCountLoaded of Counter

module Server =

    open Shared
    open Fable.Remoting.Client

    /// A proxy you can use to talk to server directly
    let api : ICounterApi =
      Remoting.createApi()
      |> Remoting.withRouteBuilder Route.builder
      |> Remoting.buildProxy<ICounterApi>
let initialCounter = Server.api.initialCounter

// defines the initial state and initial command (= side-effect) of the application
let init () : Model * Cmd<Msg> =
    let initialModel = { Counter = None; Map = None; Map2 = None; SetMapSuccess = false }
    let loadCountCmd =
        Cmd.OfAsync.perform initialCounter () InitialCountLoaded
    initialModel, loadCountCmd

// The update function computes the next state of the application based on the current state and the incoming events/messages
// It can also run side-effects (encoded as commands) like calling the server via Http.
// these commands in turn, can dispatch messages to which the update function will react.
let update (msg : Msg) (currentModel : Model) : Model * Cmd<Msg> =
    match msg with
    | Increment ->
        let nextModel = { currentModel with Counter = currentModel.Counter |> Option.map (fun c -> { Value = c.Value + 1 }) }
        nextModel, Cmd.none
    | Decrement ->
        let nextModel = { currentModel with Counter = currentModel.Counter |> Option.map (fun c -> { Value = c.Value - 1 }) }
        nextModel, Cmd.none
    | GetMap ->
        let cmd = Cmd.OfAsync.perform Server.api.getMap () GotMap
        currentModel, cmd
    | GotMap newMap ->
        let nextModel = { currentModel with Map = Some newMap; SetMapSuccess = true }
        nextModel, Cmd.none
    | SetMapCommand newMap -> 
        let cmd = Cmd.OfAsync.perform Server.api.setMap newMap IsSetMapEvent
        { currentModel with SetMapSuccess = false }, cmd
    | IsSetMapEvent isSuccess ->
        let nextModel =
            if not isSuccess then { currentModel with Map = None } else currentModel

        { nextModel with SetMapSuccess = isSuccess } , Cmd.none
    | GetMap2 ->
        let cmd = Cmd.OfAsync.perform Server.api.getMap2 () GotMap2
        currentModel, cmd
    | GotMap2 newMap ->
        let nextModel = { currentModel with Map2 = Some newMap; SetMapSuccess = true }
        nextModel, Cmd.none
    | SetMapCommand2 newMap -> 
        let cmd = Cmd.OfAsync.perform Server.api.setMap2 MyMapWorks.value IsSetMapEvent2
        { currentModel with SetMapSuccess = false }, cmd
    | IsSetMapEvent2 isSuccess ->
        let nextModel =
            if not isSuccess then { currentModel with Map2 = None } else currentModel
        { nextModel with SetMapSuccess = isSuccess } , Cmd.none
    | InitialCountLoaded count ->
        let nextModel = { currentModel with Counter = Some count }
        nextModel, Cmd.none
    | _ -> currentModel, Cmd.none


let safeComponents =
    let components =
        span [ ]
           [ a [ Href "https://github.com/SAFE-Stack/SAFE-template" ]
               [ str "SAFE  "
                 str Version.template ]
             str ", "
             a [ Href "https://github.com/giraffe-fsharp/Giraffe" ] [ str "Giraffe" ]
             str ", "
             a [ Href "http://fable.io" ] [ str "Fable" ]
             str ", "
             a [ Href "https://elmish.github.io" ] [ str "Elmish" ]
             str ", "
             a [ Href "https://fulma.github.io/Fulma" ] [ str "Fulma" ]
             str ", "
             a [ Href "https://zaid-ajaj.github.io/Fable.Remoting/" ] [ str "Fable.Remoting" ]

           ]

    span [ ]
        [ str "Version "
          strong [ ] [ str Version.app ]
          str " powered by: "
          components ]

let showCounter = function
    | { Counter = Some counter } -> string counter.Value
    | { Counter = None   } -> "Loading..."

let showMap = function
| { Map = Some map } -> MyMapFails.count map |> string 
| { Map = None   } -> "Loading..."

let button txt onClick =
    Button.button
        [ Button.IsFullWidth
          Button.Color IsPrimary
          Button.OnClick onClick ]
        [ str txt ]

let view (model : Model) (dispatch : Msg -> unit) =
    div []
        [ Navbar.navbar [ Navbar.Color IsPrimary ]
            [ Navbar.Item.div [ ]
                [ Heading.h2 [ ]
                    [ str "SAFE Template" ] ] ]

          Container.container []
              [ Content.content [ Content.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Centered) ] ]
                    [ Heading.h3 [] [ str ("Press buttons to manipulate counter: " + showCounter model) ] ]
                Columns.columns [] [
                    Column.column [] [ button "-" (fun _ -> dispatch Decrement) ]
                    Column.column [] [ button "+" (fun _ -> dispatch Increment) ]
                ]
              ]
          Container.container []
            [ Content.content [ Content.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Centered) ] ]
                  [ Heading.h3 [] [ str ("model length: " + showMap model) ] ]
              Content.content [ Content.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Centered) ] ]
                  [ Heading.h3 [] [ str ("Success = " + (model.SetMapSuccess |> string)) ] ]
              Columns.columns [] [
                  Column.column [] [ button "getMap: Map<(int,int),int>" (fun _ -> dispatch GetMap) ]
                  Column.column [] [ button "setMap: Map<(int,int),int> -> bool" (fun _ -> dispatch (SetMapCommand (MyMapFails.value))) ]
                  Column.column [] [ button "getMap2: Map<int,int>" (fun _ -> dispatch GetMap2) ]
                  Column.column [] [ button "setMap2: Map<int,int> -> bool" (fun _ -> dispatch (SetMapCommand2 (MyMapWorks.value))) ]
              ]
            ]

          Footer.footer [ ]
                [ Content.content [ Content.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Centered) ] ]
                    [ safeComponents ] ] ]
        

#if DEBUG
open Elmish.Debug
open Elmish.HMR
#endif

Program.mkProgram init update view
#if DEBUG
|> Program.withConsoleTrace
#endif
|> Program.withReactBatched "elmish-app"
#if DEBUG
|> Program.withDebugger
#endif
|> Program.run
