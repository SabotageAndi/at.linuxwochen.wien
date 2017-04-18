namespace www.linuxwochen.common

 module Database = 
    open entities
    open common
    open System.Collections.Generic
    open System.Linq
            
    let tables = [typeof<Entry>; typeof<Room>; typeof<ConferenceDay>; typeof<ConferenceData>; typeof<Speaker>; typeof<Speaker2Entry>; typeof<EntryFavorite>]

    let getSQLConnection() =
        CurrentState.SQLConnection

    let getTable<'T when 'T : (new : 'T )>() =
        getSQLConnection().Table<'T>()

    let createTable tableType =
        getSQLConnection().CreateTable(tableType, SQLite.CreateFlags.None) |> ignore

    let dropTable<'T when 'T : (new : 'T )>() =
        getSQLConnection().DropTable<'T>() |> ignore

    let createSchema() =
        tables
        |> List.iter createTable
                           
    let dropSchema() =
        getSQLConnection().DropTable<Entry>() |> ignore
        getSQLConnection().DropTable<Room>() |> ignore
        getSQLConnection().DropTable<ConferenceDay>() |> ignore
        getSQLConnection().DropTable<ConferenceData>() |> ignore
        getSQLConnection().DropTable<Speaker>() |> ignore
        getSQLConnection().DropTable<Speaker2Entry>() |> ignore
        getSQLConnection().DropTable<EntryFavorite>() |> ignore
        

    let writeDbEntry dbEntry =
        getSQLConnection().Insert dbEntry |> ignore

    let updateDbEntry dbEntry =
        getSQLConnection().Update dbEntry |> ignore

    let query<'T when 'T : (new : 'T )>(queryString, arg1) =
        let args = [arg1].ToArray()
        getSQLConnection().Query<'T>(queryString, args)

    let query2<'T when 'T : (new : 'T )>(queryString, arg1, arg2) =
        let args = [arg1;arg2].ToArray()
        getSQLConnection().Query<'T>(queryString, args)

    let filter<'T when 'T : (new : 'T )> predicate =
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