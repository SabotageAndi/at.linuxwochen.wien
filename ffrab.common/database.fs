namespace ffrab.mobile.common

 module Database = 
    open entities
    open common
    open Microsoft.FSharp.Linq
            
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

    let query<'T when 'T : not struct>(query, arg1) =
        let args = [arg1]
        getSQLConnection().Query<'T>(query, args)

    let filter<'T when 'T : not struct> predicate =
        let linq = predicate |> toLinq
        getTable<'T>().Where(linq)