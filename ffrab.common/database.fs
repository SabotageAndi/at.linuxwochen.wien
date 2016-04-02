namespace ffrab.mobile.common

 module Database = 
    open entities
    open common
    open System.Collections.Generic
    open System.Linq
            
    let tables = [typeof<Entry>; typeof<Room>; typeof<ConferenceDay>; typeof<ConferenceData>; typeof<Speaker>; typeof<Speaker2Entry>; typeof<EntryFavorite>]

    let getSQLConnection() =
        CurrentState.SQLConnection

    let getTable<'T when 'T : not struct>() =
        getSQLConnection().Table<'T>()

    let createTable tableType =
        getSQLConnection().CreateTable(tableType, SQLite.Net.Interop.CreateFlags.None) |> ignore

    let dropTable tableType =
        getSQLConnection().DropTable(tableType) |> ignore

    let createSchema() =
        tables
        |> List.iter createTable
                           
    let dropSchema() =
        tables
        |> List.iter dropTable

    let writeDbEntry dbEntry =
        getSQLConnection().Insert dbEntry |> ignore

    let updateDbEntry dbEntry =
        getSQLConnection().Update dbEntry |> ignore

    let query<'T when 'T : not struct>(queryString, arg1) =
        let args = [arg1].ToArray()
        getSQLConnection().Query<'T>(queryString, args)

    let query2<'T when 'T : not struct>(queryString, arg1, arg2) =
        let args = [arg1;arg2].ToArray()
        getSQLConnection().Query<'T>(queryString, args)

    let filter<'T when 'T : not struct> predicate =
        let linq = predicate |> toLinq
        getTable<'T>().Where(linq)

    let any<'T> (collection : IEnumerable<'T>) = 
        collection.Any()

    let delete<'T>(collection : IEnumerable<'T>) = 
        collection 
        |> Seq.iter (fun i -> getSQLConnection().Delete(i) |> ignore)

    let tryFirst<'T>(collection : IEnumerable<'T>) =
        collection
        |> Seq.tryHead