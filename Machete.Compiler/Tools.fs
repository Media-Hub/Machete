﻿namespace Machete.Compiler


//module Tools =
//
//    module TreeTraverser = 
//
//        type NodeTest<'a, 'b> = 'a -> option<'b>
//
//        type TreeTraverser() =
//            member x.Bind (f:NodeTest<'a, 'b>, g:unit -> NodeTest<'b, 'c>) (v:'a) = 
//                match f v with
//                | Some v -> g () v
//                | None -> None               
//            member x.Delay (f:unit -> NodeTest<'a, 'b>) = f()
//            member x.Return v1 v2 = Some v1
//            member x.ReturnFrom (f:NodeTest<'a, 'b>) v = f v
//        
//        let traverse = TreeTraverser()
//
//    open System
//    open System.Collections.Generic
//    open System.Diagnostics
//    open LazyList 
//    
//    [<DebuggerStepThrough>]
//    type ParseException (message) =
//        inherit Exception (message)
//
//    [<Struct;DebuggerStepThrough>]
//    type State<'a, 'b> (input:LazyList<'a>, data:'b) =
//        member this.Input = input
//        member this.Data = data
//
//    type Result<'a, 'b, 'c> =
//    | Success of 'c * State<'a, 'b>
//    | Failure of list<string> * State<'a, 'b>
//    | Empty of State<'a, 'b>
//    | End of State<'a, 'b>
//
//
//    type Parser<'a, 'b, 'c> = 
//        State<'a, 'b> -> Result<'a, 'b, 'c>
//
//    let zero<'a, 'b, 'c> (state:State<'a, 'b>) =
//        Result<'a, 'b, 'c>.Empty (state)
//
//    let item<'a, 'b> (state:State<'a, 'b>) : Result<'a, 'b, 'a> = 
//        match state.Input with
//        | Cons (head, tail) ->
//            Success(head, State (tail, state.Data))
//        | Nil ->
//            End (state)    
//
//    let result<'a, 'b, 'c> (value:'c) (state:State<'a, 'b>) =
//        Success (value, state)
//    
//    let (>>=) (parser:Parser<'a, 'b, 'c>) (next:'c -> Parser<'a, 'b, 'd>) (state:State<'a, 'b>) =
//        let result = parser state
//        match result with
//        | Success (value, state) ->
//            next value state
//        | Failure (messages, state) ->
//            Result<'a, 'b, 'd>.Failure (messages, state)
//        | Empty (state) ->
//            Result<'a, 'b, 'd>.Empty (state)
//        | End (state) ->
//            Result<'a, 'b, 'd>.End (state)  
//
//    let (<|>) (left:Parser<'a, 'b, 'c>) (right:Parser<'a, 'b, 'c>) (state:State<'a, 'b>) =
//        let result = left state
//        match result with
//        | Empty (_) -> right state
//        | Failure (messages, _) -> right state
//        | _ -> result 
//
//    let run p i d =
//        p (State(i, d)) 
//
//    type ParseMonad() =        
//        member this.Bind (parser:Parser<'a, 'b, 'c>, next:'c -> Parser<'a, 'b, 'd>) (state:State<'a, 'b>) = 
//            (parser >>= next) state     
//        member this.Combine (left:Parser<'a, 'b, 'c>, right:Parser<'a, 'b, 'c>) (state:State<'a, 'b>) = 
//            (left <|> right) state     
//        member this.Delay (delayer:unit -> Parser<'a, 'b, 'c>) (state:State<'a, 'b>) = 
//            delayer () state
//        member this.Return (value:'c) (state:State<'a, 'b>) = 
//            result value state
//        member this.ReturnFrom (parser:Parser<'a, 'b, 'c>) (state:State<'a, 'b>) = 
//            parser state 
//        member this.Zero () (state:State<'a, 'b>) = 
//            zero state
//    
//    let parse = ParseMonad()
//
//    let fail (message:string) (state:State<'a, 'b>) =
//        raise (ParseException(message))
//
//    let (|>>) (parser:Parser<'a, 'b, 'c>) (f:'c -> 'd) = parse {
//        let! v = parser
//        return f v   
//    }
//
//    let satisfy (predicate:'a -> bool) = parse {
//        let! value = item
//        if predicate value then
//            return value 
//    }
//
//    let maybe (parser:Parser<'a, 'b, 'c>) (state:State<'a, 'b>) = 
//        ((parser |>> Some) <|> result None) state 
//
//    let choice (ps:seq<Parser<'a, 'b, 'c>>) (state:State<'a, 'b>) =
//        (ps |> Seq.reduce (<|>)) state
//
//
//    let between left right parser =
//        parse {
//            let! _ = left
//            let! v = parser
//            let! _ = right
//            return v
//        }
//
//    let skip p = parse {
//        let! v = p
//        return ()
//    }
//
//    let many (parser:Parser<'a, 'b, 'c>) (state:State<'a, 'b>) = 
//        let rec many result (state:State<'a, 'b>) =
//            let r = parser state
//            match r with
//            | Success (value, state) -> many (value::result) state
//            | _ -> Success(result, state)     
//        many [] state
//
//    let many1 parser = parse {
//        let! r = many parser
//        if not r.IsEmpty then
//            return r
//    }
//
//    let manyFold parser start (f:_ -> _ -> _) = parse {
//        let! r = many parser
//        return r |> List.rev |> List.fold f start
//    }
//
//    let many1Fold parser start (f:_ -> _ -> _) = parse {
//        let! r = many1 parser
//        return r |> List.fold f start
//    } 
//    
//    let manyWithSepFold (par:Parser<'a, 'b, 'c>) (sep:Parser<'a, 'b, 'd>) (f:'c * 'd * 'c -> 'c) (d:'c) (sepStart:'d) (state:State<'a, 'b>) =
//        let firstResult = par state
//        match firstResult with
//        | Success (firstValue, firstState) ->
//            let rec run (result:list<'d * 'c>) state =
//                let sepResult = sep state
//                match sepResult with                
//                | Success (sepValue, sepState) -> 
//                    let nextResult = par sepState
//                    match nextResult with
//                    | Success (nextValue, nextState) ->
//                        run ((sepValue, nextValue)::result) nextState
//                    | _ -> failwith ""
//                | _ -> result, state
//            let result, state = run [] firstState
//            let first = f (d, sepStart, firstValue)
//            Success (result |> Seq.fold (fun x (y, z) -> f (x, y, z)) first, state) 
//        | r -> r
//
//    let manySepFold (par:Parser<'a, 'b, 'c>) (sep:Parser<'a, 'b, 'd>) (f:'c * 'c -> 'c) (d:'c) (state:State<'a, 'b>) =
//        let firstResult = par state
//        match firstResult with
//        | Success (firstValue, firstState) ->
//            let rec run (result:list<'c>) state =
//                let sepResult = sep state
//                match sepResult with                
//                | Success (sepValue, sepState) -> 
//                    let nextResult = par sepState
//                    match nextResult with
//                    | Success (nextValue, nextState) ->
//                        run (nextValue::result) nextState
//                    | _ -> failwith ""
//                | _ -> result, state
//            let result, state = run [] firstState
//            let first = f (d, firstValue)
//            Success (result |> List.rev |> List.fold (fun x y -> f (x, y)) first, state) 
//        | r -> r
//
//    let manySep (par:Parser<'a, 'b, 'c>) (sep:Parser<'a, 'b, 'd>) (state:State<'a, 'b>) =
//        let firstResult = par state
//        match firstResult with
//        | Success (firstValue, firstState) ->
//            let rec run (result:list<'c>) state =
//                let sepResult = sep state
//                match sepResult with                
//                | Success (sepValue, sepState) -> 
//                    let nextResult = par sepState
//                    match nextResult with
//                    | Success (nextValue, nextState) ->
//                        run (nextValue::result) nextState
//                    | _ -> failwith ""
//                | _ -> result, state
//            let result, state = run [] firstState
//            Success (firstValue::(result |> List.rev), state) 
//        | _ -> Empty(state)
//        
//    let isNotFollowedBy p (state:State<'a,'b>) : Result<'a, 'b, unit> =
//        (parse {
//            let! v = maybe p
//            if v.IsNone then
//                return ()
//        }) state
//
//    let pipe2 (p1:Parser<'a, 'b, 'c>) (p2:Parser<'a, 'b, 'd>) (f:'c -> 'd -> 'e) = 
//        parse {
//            let! v1 = p1
//            let! v2 = p2
//            return f v1 v2
//        }
//
//    let pipe3 (p1:Parser<'a, 'b, 'c>) (p2:Parser<'a, 'b, 'd>) (p3:Parser<'a, 'b, 'e>) (f:'c -> 'd -> 'e -> 'f) = 
//        parse {
//            let! v1 = p1
//            let! v2 = p2
//            let! v3 = p3
//            return f v1 v2 v3
//        }
//
//    let pipe4 (p1:Parser<'a, 'b, 'c>) (p2:Parser<'a, 'b, 'd>) (p3:Parser<'a, 'b, 'e>) (p4:Parser<'a, 'b, 'f>) (f:'c -> 'd -> 'e -> 'f -> 'g) = 
//        parse {
//            let! v1 = p1
//            let! v2 = p2
//            let! v3 = p3
//            let! v4 = p4
//            return f v1 v2 v3 v4
//        }
//
//    let pipe5 (p1:Parser<'a, 'b, 'c>) (p2:Parser<'a, 'b, 'd>) (p3:Parser<'a, 'b, 'e>) (p4:Parser<'a, 'b, 'f>) (p5:Parser<'a, 'b, 'g>) (f:'c -> 'd -> 'e -> 'f -> 'g -> 'h) = 
//        parse {
//            let! v1 = p1
//            let! v2 = p2
//            let! v3 = p3
//            let! v4 = p4
//            let! v5 = p5
//            return f v1 v2 v3 v4 v5
//        }
//
//    let tuple2<'a, 'b, 'c, 'd, 'e> (p1:Parser<'a, 'b, 'c>) (p2:Parser<'a, 'b, 'd>) (f:'c * 'd -> 'e) = 
//        parse {
//            let! v1 = p1
//            let! v2 = p2
//            return f (v1, v2)
//        }
//
//    let tuple3 (p1:Parser<'a, 'b, 'c>) (p2:Parser<'a, 'b, 'd>) (p3:Parser<'a, 'b, 'e>) (f:'c * 'd * 'e -> 'f) = 
//        parse {
//            let! v1 = p1
//            let! v2 = p2
//            let! v3 = p3
//            return f (v1, v2, v3)
//        }
//
//    let tuple4 (p1:Parser<'a, 'b, 'c>) (p2:Parser<'a, 'b, 'd>) (p3:Parser<'a, 'b, 'e>) (p4:Parser<'a, 'b, 'f>) (f:'c * 'd * 'e * 'f -> 'g) = 
//        parse {
//            let! v1 = p1
//            let! v2 = p2
//            let! v3 = p3
//            let! v4 = p4
//            return f (v1, v2, v3, v4)
//        }
//
//    let tuple5 (p1:Parser<'a, 'b, 'c>) (p2:Parser<'a, 'b, 'd>) (p3:Parser<'a, 'b, 'e>) (p4:Parser<'a, 'b, 'f>) (p5:Parser<'a, 'b, 'g>) (f:'c * 'd * 'e * 'f * 'g -> 'h) = 
//        parse {
//            let! v1 = p1
//            let! v2 = p2
//            let! v3 = p3
//            let! v4 = p4
//            let! v5 = p5
//            return f (v1, v2, v3, v4, v5)
//        }
//
//    let createParserRef<'a, 'b, 'c> () =
//        let dummyParser = fun state -> failwith "a parser was not initialized"
//        let r = ref dummyParser
//        (fun state -> !r state), r : Parser<'a, 'b, 'c> * Parser<'a, 'b, 'c> ref

//module Tools1 =
//
//    module TreeTraverser = 
//
//        type NodeTest<'a, 'b> = 'a -> option<'b>
//
//        type TreeTraverser() =
//            member x.Bind (f:NodeTest<'a, 'b>, g:unit -> NodeTest<'b, 'c>) (v:'a) = 
//                match f v with
//                | Some v -> g () v
//                | None -> None               
//            member x.Delay (f:unit -> NodeTest<'a, 'b>) = f()
//            member x.Return v1 v2 = Some v1
//            member x.ReturnFrom (f:NodeTest<'a, 'b>) v = f v
//        
//        let traverse = TreeTraverser()
//
//    open System
//    open System.Collections.Generic
//    open System.Diagnostics
//    open LazyList 
//
//    [<Struct;DebuggerStepThrough>]
//    type State<'a, 'b> (input:LazyList<'a>, data:'b) =
//        member this.Input = input
//        member this.Data = data
//
//    type Result<'a, 'b, 'c> =
//    | Success of 'c * State<'a, 'b>
//    | Failure of list<string> * State<'a, 'b>
//        
//    let isSuccess r =
//        match r with
//        | Success (v, s) -> true
//        | _ -> false
//    
//    type Parser<'a, 'b, 'c> = 
//        State<'a, 'b> -> seq<Result<'a, 'b, 'c>>
//
//    let inline zero<'a, 'b, 'c> (state:State<'a, 'b>) =
//        Seq.empty<Result<'a, 'b, 'c>>
//
//    let inline item<'a, 'b> (state:State<'a, 'b>) = seq { 
//        match state.Input with
//        | Cons (head, tail) ->
//            yield Success(head, State (tail, state.Data))
//        | Nil -> ()
//    } 
//
//    let inline result<'a, 'b, 'c> (value:'c) (state:State<'a, 'b>) = seq  {
//        yield Success (value, state)
//    }
//    
//    let run p i d =
//        p (State(i, d)) 
//
////    let (>>=) (m:Parser<'a, 'b, 'c>) (f:'c -> Parser<'a, 'b, 'd>) (state:State<'a, 'b>) =
////        let rec run errors = seq {
////            for r in m state do
////                match r with
////                | Success (v, s) ->
////                    yield! f v s
////                | Failure (ms, s) ->
////                    yield! run (errors @ ms)
////        }
////        run []
//
//    let inline (>>=) (m:Parser<'a, 'b, 'c>) (f:'c -> Parser<'a, 'b, 'd>) (state:State<'a, 'b>) =
//        if not (LazyList.isEmpty state.Input) then
//            let mr = m state |> Seq.tryFind isSuccess 
//            match mr with
//            | Some (Success(mv, mState)) ->
//                f mv mState   
//            | _ -> Seq.empty
//        else
//            Seq.empty
////    let (<|>) (l:Parser<'a, 'b, 'c>) (r:Parser<'a, 'b, 'c>) (state:State<'a, 'b>) =  
////        let rec run p = seq {
////            if not (LazyList.isEmpty state.Input) then
////                for result in p state do
////                    match result with
////                    | Success (_, _) ->
////                        yield result
////                    | Failure (_, _) -> ()
////        }
////        Seq.append (run l) (run r)
//
//    let (<|>) (l:Parser<'a, 'b, 'c>) (r:Parser<'a, 'b, 'c>) (state:State<'a, 'b>) =
//        if state.Input |> LazyList.isEmpty then Seq.empty else
//        let rStart = l state |> Seq.tryFind isSuccess
//        match rStart with
//        | Some (Success(vStart, sStart)) ->
//            [|rStart.Value|]:>seq<Result<'a, 'b, 'c>>   
//        | _ ->
//            let right = r state |> Seq.tryFind isSuccess
//            match right with
//            | Some (Success(rval, sStart)) ->
//                [|right.Value|]:>seq<Result<'a, 'b, 'c>> 
//            | _ -> Seq.empty
//
//    type ParseMonad() =        
//        member this.Bind (f:Parser<'a, 'b, 'c>, g:'c -> Parser<'a, 'b, 'd>) : Parser<'a, 'b, 'd> = f >>= g     
//        member this.Combine (f, g) = f <|> g      
//        member this.Delay (f:unit -> Parser<'a, 'b, 'c>) (state:State<'a, 'b>) = f () state
//        member this.Return value state = [Success (value, state)] :> seq<Result<'a, 'b, 'c>>
//        member this.ReturnFrom p = p 
//        member this.Zero () state = Seq.empty<Result<'a, 'b, 'c>>
//    
//    let parse = ParseMonad()
//
//    
//
//    
//    let inline (|>>) (parser:Parser<'a, 'b, 'c>) (f:'c -> 'd) = parse {
//        let! v = parser
//        return f v   
//    }
//
//    let inline satisfy predicate = parse {
//        let! value = item
//        if predicate value then
//            return value 
//    }
//
//    let inline maybe parser = parse {
//        return! parser |>> Some <|> result None 
//    }
//
//    let inline choice (ps:seq<Parser<'a, 'b, 'c>>) (state:State<'a, 'b>) =
//        seq {
//            for p in ps do
//                yield! p state    
//        }
//
//    let inline between left right parser =
//        parse {
//            let! _ = left
//            let! v = parser
//            let! _ = right
//            return v
//        }
//
//    let inline skip p = parse {
//        let! v = p
//        return ()
//    }
//
//    let inline many (parser:Parser<'a, 'b, 'c>) (state:State<'a, 'b>) = 
//        let rec many result (state:State<'a, 'b>) =
//            let v = parser state |> Seq.tryFind isSuccess
//            match v with
//            | Some (Success (v, s)) -> many (v::result) s
//            | None -> [Success(result, state)] :> seq<Result<'a, 'b, 'c list>>       
//        many [] state
////    let many parser = 
////        let rec many result = parse {
////            let! v = parser
////            let result = v::result
////            return! many result
////            return result    
////        }
////        many []
//
////    let many (parser:Parser<'a, 'b, 'c>) (state:State<'a, 'b>) : seq<Result<'a, 'b, list<'c>>> = 
////        let rec many (result:list<'c>) (state:State<'a, 'b>) : seq<Result<'a, 'b, list<'c>>> = seq {
////            for x in parser state do
////                match x with
////                | Success (v, s) ->
////                    yield! many (v::result) s
////                | Failure (ms, s) ->
////                    yield Success (result, s)
////        }
////        seq {       
////            for r in many [] state do
////                match r with
////                | Success (v, s) ->
////                    yield r
////                | Failure (ms, s) -> ()                
////        } |> Seq.toList |> fun s -> if s.IsEmpty then [Success([], state)]|>List.toSeq else [s.Head]|>List.toSeq
//
//    let inline many1 parser = parse {
//        let! r = many parser
//        if not r.IsEmpty then
//            return r
//    }
//
//    let inline manyFold parser start (f:_ -> _ -> _) = parse {
//        let! r = many parser
//        return r |> List.rev |> List.fold f start
//    }
//
//    let inline many1Fold parser start (f:_ -> _ -> _) = parse {
//        let! r = many1 parser
//        return r |> List.fold f start
//    } 
//
//    
//    let inline manyWithSepFold (par:Parser<'a, 'b, 'c>) (sep:Parser<'a, 'b, 'd>) (f:'c * 'd * 'c -> 'c) (d:'c) (sepStart:'d) (state:State<'a, 'b>) =
//        let run = ref true
//        let result = List<'d * 'c>()
//        let rStart = par state |> Seq.tryFind isSuccess
//        match rStart with
//        | Some (Success(vStart, sStart)) ->
//            let currentState = ref sStart
//            while !run do
//                let r = sep !currentState |> Seq.tryFind isSuccess
//                match r with
//                | Some (Success(sepVal, stepState)) ->
//                    let r = par stepState |> Seq.tryFind isSuccess
//                    match r with
//                    | Some (Success(parVal, parState)) ->
//                        result.Add(sepVal, parVal) 
//                        currentState := parState
//                    | None -> run := false                    
//                | None -> run := false
//            let first = f (d, sepStart, vStart)
//            let r = [Success (result |> Seq.fold (fun x (y, z) -> f (x, y, z)) first, !currentState)] 
//            r |> List.toSeq
//        | _ -> Seq.empty
//
//    let inline manySepFold (par:Parser<'a, 'b, 'c>) (sep:Parser<'a, 'b, 'd>) (f:'c * 'c -> 'c) (d:'c) (state:State<'a, 'b>) =
//        let run = ref true
//        let result = List<'c>()
//        let rStart = par state |> Seq.tryFind isSuccess
//        match rStart with
//        | Some (Success(vStart, sStart)) ->
//            let currentState = ref sStart
//            while !run do
//                let r = sep !currentState |> Seq.tryFind isSuccess
//                match r with
//                | Some (Success(sepVal, stepState)) ->
//                    let r = par stepState |> Seq.tryFind isSuccess
//                    match r with
//                    | Some (Success(parVal, parState)) ->
//                        result.Add(parVal) 
//                        currentState := parState
//                    | None -> run := false                    
//                | None -> run := false
//            let first = f (d, vStart)
//            let r = [Success (result |> Seq.fold (fun x y -> f (x, y)) first, !currentState)] 
//            r |> List.toSeq
//        | _ -> Seq.empty
//
//    let inline manySep (par:Parser<'a, 'b, 'c>) (sep:Parser<'a, 'b, 'd>) (state:State<'a, 'b>) =
//        let run = ref true
//        let result = List<'c>()
//        let rStart = par state |> Seq.tryFind isSuccess
//        match rStart with
//        | Some (Success(vStart, sStart)) ->
//            let currentState = ref sStart
//            while !run do
//                let r = sep !currentState |> Seq.tryFind isSuccess
//                match r with
//                | Some (Success(sepVal, stepState)) ->
//                    let r = par stepState |> Seq.tryFind isSuccess
//                    match r with
//                    | Some (Success(parVal, parState)) ->
//                        result.Add(parVal) 
//                        currentState := parState
//                    | None -> run := false                    
//                | None -> run := false
//            [Success (result |> Seq.toList, !currentState)] :> seq<Result<'a, 'b, 'c list>>
//        | _ -> Seq.empty
//        
//    let inline isNotFollowedBy p =
//        parse {
//            let! v = maybe p
//            match v with
//            | Some _ -> ()
//            | None -> return ()
//        }
//
//    let pipe2 (p1:Parser<'a, 'b, 'c>) (p2:Parser<'a, 'b, 'd>) (f:'c -> 'd -> 'e) = 
//        parse {
//            let! v1 = p1
//            let! v2 = p2
//            return f v1 v2
//        }
//
//    let pipe3 (p1:Parser<'a, 'b, 'c>) (p2:Parser<'a, 'b, 'd>) (p3:Parser<'a, 'b, 'e>) (f:'c -> 'd -> 'e -> 'f) = 
//        parse {
//            let! v1 = p1
//            let! v2 = p2
//            let! v3 = p3
//            return f v1 v2 v3
//        }
//
//    let pipe4 (p1:Parser<'a, 'b, 'c>) (p2:Parser<'a, 'b, 'd>) (p3:Parser<'a, 'b, 'e>) (p4:Parser<'a, 'b, 'f>) (f:'c -> 'd -> 'e -> 'f -> 'g) = 
//        parse {
//            let! v1 = p1
//            let! v2 = p2
//            let! v3 = p3
//            let! v4 = p4
//            return f v1 v2 v3 v4
//        }
//
//    let pipe5 (p1:Parser<'a, 'b, 'c>) (p2:Parser<'a, 'b, 'd>) (p3:Parser<'a, 'b, 'e>) (p4:Parser<'a, 'b, 'f>) (p5:Parser<'a, 'b, 'g>) (f:'c -> 'd -> 'e -> 'f -> 'g -> 'h) = 
//        parse {
//            let! v1 = p1
//            let! v2 = p2
//            let! v3 = p3
//            let! v4 = p4
//            let! v5 = p5
//            return f v1 v2 v3 v4 v5
//        }
//
//    let inline tuple2<'a, 'b, 'c, 'd, 'e> (p1:Parser<'a, 'b, 'c>) (p2:Parser<'a, 'b, 'd>) (f:'c * 'd -> 'e) = 
//        parse {
//            let! v1 = p1
//            let! v2 = p2
//            return f (v1, v2)
//        }
//
//    let tuple3 (p1:Parser<'a, 'b, 'c>) (p2:Parser<'a, 'b, 'd>) (p3:Parser<'a, 'b, 'e>) (f:'c * 'd * 'e -> 'f) = 
//        parse {
//            let! v1 = p1
//            let! v2 = p2
//            let! v3 = p3
//            return f (v1, v2, v3)
//        }
//
//    let tuple4 (p1:Parser<'a, 'b, 'c>) (p2:Parser<'a, 'b, 'd>) (p3:Parser<'a, 'b, 'e>) (p4:Parser<'a, 'b, 'f>) (f:'c * 'd * 'e * 'f -> 'g) = 
//        parse {
//            let! v1 = p1
//            let! v2 = p2
//            let! v3 = p3
//            let! v4 = p4
//            return f (v1, v2, v3, v4)
//        }
//
//    let tuple5 (p1:Parser<'a, 'b, 'c>) (p2:Parser<'a, 'b, 'd>) (p3:Parser<'a, 'b, 'e>) (p4:Parser<'a, 'b, 'f>) (p5:Parser<'a, 'b, 'g>) (f:'c * 'd * 'e * 'f * 'g -> 'h) = 
//        parse {
//            let! v1 = p1
//            let! v2 = p2
//            let! v3 = p3
//            let! v4 = p4
//            let! v5 = p5
//            return f (v1, v2, v3, v4, v5)
//        }
//
//    let createParserRef<'a, 'b, 'c> () =
//        let dummyParser = fun state -> failwith "a parser was not initialized"
//        let r = ref dummyParser
//        (fun state -> !r state), r : Parser<'a, 'b, 'c> * Parser<'a, 'b, 'c> ref