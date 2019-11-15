# Description of issue

Api consists of these functions (initialCounter part of safe stack)

    type ICounterApi =
        { initialCounter : unit -> Async<Counter>
          getMap : unit -> Async<Map<int * int,int>>    // works
          setMap : Map<int * int,int> -> Async<bool>    // this one fails
          getMap2 : unit -> Async<Map<int,int>>         // works
          setMap2 : Map<int,int> -> Async<bool> }       // works

Server reports when invoking setMap

    info: Microsoft.AspNetCore.Hosting.Internal.WebHost[1]
          Request starting HTTP/1.1 POST http://localhost:8085/api/ICounterApi/setMap application/json; charset=UTF-8 13
    System.InvalidCastException: Cannot cast Newtonsoft.Json.Linq.JArray to Newtonsoft.Json.Linq.JToken.
       at Newtonsoft.Json.Linq.Extensions.Convert[T,U](T token)
       at Fable.Remoting.Server.DynamicRecord.createArgsFromJson$cont@132-1(RecordFunctionInfo func, FSharpOption`1 logger, Type input, JToken parsedJson, Unit unitVar)
       at Fable.Remoting.Server.DynamicRecord.tryCreateArgsFromJson(RecordFunctionInfo func, String inputJson, FSharpOption`1 logger)

client reports "500 Internal Server Error"
and payload =

    [[[[1,1],1]]]

and errors here...

    let send (req: HttpRequest) =
       Async.FromContinuations <| fun (resolve, _, _) -> 
           let xhr = XMLHttpRequest.Create()
            
           match req.HttpMethod with 
           | HttpMethod.GET -> xhr.``open``("GET", req.Url)
           | HttpMethod.POST -> xhr.``open``("POST", req.Url)
                
           // set the headers, must be after opening the request
           for (key, value) in req.Headers do 
               xhr.setRequestHeader(key, value)

           xhr.onreadystatechange <- fun _ ->
               match xhr.readyState with
               | ReadyState.Done -> resolve { StatusCode = unbox xhr.status; ResponseBody = xhr.responseText }
               | otherwise -> ignore() 
         
           match req.RequestBody with 
           | Empty -> xhr.send()
           | Json content -> xhr.send(content) !!!!!!!!!!!!!!!!!!!!!!!!!!!!1!!!!!!! fails here
           | Binary content -> xhr.send(content)

# How to reproduce

Run up the app, you get 4 buttons, one corresponding to each call on the API, and a success failure flag.
all calls "succeed", except "setMap: Map<(int,int),int> -> bool"

# SAFE Template

This template can be used to generate a full-stack web application using the [SAFE Stack](https://safe-stack.github.io/). It was created using the dotnet [SAFE Template](https://safe-stack.github.io/docs/template-overview/). If you want to learn more about the template why not start with the [quick start](https://safe-stack.github.io/docs/quickstart/) guide?

## Install pre-requisites

You'll need to install the following pre-requisites in order to build SAFE applications

* The [.NET Core SDK](https://www.microsoft.com/net/download)
* [FAKE 5](https://fake.build/) installed as a [global tool](https://fake.build/fake-gettingstarted.html#Install-FAKE)
* The [Yarn](https://yarnpkg.com/lang/en/docs/install/) package manager (you an also use `npm` but the usage of `yarn` is encouraged).
* [Node LTS](https://nodejs.org/en/download/) installed for the front end components.
* If you're running on OSX or Linux, you'll also need to install [Mono](https://www.mono-project.com/docs/getting-started/install/).

## Work with the application

To concurrently run the server and the client components in watch mode use the following command:

```bash
fake build -t Run
```


## SAFE Stack Documentation

You will find more documentation about the used F# components at the following places:

* [Giraffe](https://github.com/giraffe-fsharp/Giraffe/blob/master/DOCUMENTATION.md)
* [Fable](https://fable.io/docs/)
* [Elmish](https://elmish.github.io/elmish/)
* [Fable.Remoting](https://zaid-ajaj.github.io/Fable.Remoting/)
* [Fulma](https://fulma.github.io/Fulma/)

If you want to know more about the full Azure Stack and all of it's components (including Azure) visit the official [SAFE documentation](https://safe-stack.github.io/docs/).

## Troubleshooting

* **fake not found** - If you fail to execute `fake` from command line after installing it as a global tool, you might need to add it to your `PATH` manually: (e.g. `export PATH="$HOME/.dotnet/tools:$PATH"` on unix) - [related GitHub issue](https://github.com/dotnet/cli/issues/9321)
