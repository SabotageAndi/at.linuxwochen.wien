namespace www.linuxwochen.common

module entities = 
    open System
    open NodaTime
    open SQLite
    open common

    type Speaker2Entry() =

        [<PrimaryKey>]
        [<AutoIncrement>]
        member val Id : int = -1 with get,set
        member val SpeakerGuid : Guid = Guid.Empty with get, set
        member val EntryGuid : Guid = Guid.Empty with get, set
        member val ConferenceId : int = 0 with get, set

    type Speaker() =
        member val Id : int = -1 with get,set
        [<PrimaryKey>]
        member val Guid : Guid = Guid.Empty with get,set
        member val Name : string = "" with get,set
        member val ConferenceId : int = 0 with get, set
    
    type Entry() = 
        member val Id : int = -1 with get, set        
        [<PrimaryKeyAttribute>]
        member val Guid : Guid = Guid.Empty with get, set        
        member val Title : string = "" with get, set
        member val Subtitle : string = "" with get, set        
        [<IndexedAttribute>]
        member val Track : string = "" with get, set        

        member this.StartData
            with get() : byte array = NodaTypeSerializerDelegate.serialize(this.Start) 
            and set (value : byte array) = this.Start <- (NodaTypeSerializerDelegate.deserialize value typedefof<OffsetDateTime> :?> OffsetDateTime)

        
        [<Ignore>]    
        member val Start : OffsetDateTime = new OffsetDateTime() with get, set
        
        member this.DurationData
            with get() : byte array = NodaTypeSerializerDelegate.serialize(this.Duration) 
            and set (value : byte array) = this.Duration <- (NodaTypeSerializerDelegate.deserialize value typedefof<Duration> :?> Duration)

        [<Ignore>]
        member val Duration : Duration = Duration.Zero with get, set
        member val Type : string = "" with get, set
        member val Language : string = "" with get, set
        member val Abstract : string = "" with get, set
        member val Description : string = "" with get, set        
        [<IndexedAttribute>]
        member val ConferenceId : int = 0 with get, set      
        [<IndexedAttribute>]
        member val ConferenceDayGuid : Guid = Guid.Empty with get, set        
        [<IndexedAttribute>]
        member val RoomGuid : Guid = Guid.Empty with get, set
        [<Ignore>]
        member val Speaker : Speaker list = [] with get, set
    
    type Room() =         
        [<PrimaryKeyAttribute>]
        member val Guid : Guid = Guid.Empty with get, set        
        [<IndexedAttribute>]
        member val ConferenceId : int = 0 with get, set        
        [<IndexedAttribute>]
        member val ConferenceDayGuid : Guid = Guid.Empty with get, set        
        member val Name : string = "" with get, set
        [<IgnoreAttribute>]
        member val Entries : Entry list = [] with get, set
    
    type ConferenceDay() =         
        [<PrimaryKeyAttribute>]
        member val Guid : Guid = Guid.Empty with get, set        
        [<IndexedAttribute>]
        member val ConferenceId : int = 0 with get, set        
        member val Index : int = -1 with get, set

        member this.DayData
            with get() : byte array = NodaTypeSerializerDelegate.serialize(this.Day) 
            and set (value : byte array) = this.Day <- (NodaTypeSerializerDelegate.deserialize value typedefof<LocalDate> :?> LocalDate)

        [<Ignore>]
        member val Day : LocalDate = new LocalDate() with get, set        

        member this.StartTimeData
            with get() : byte array = NodaTypeSerializerDelegate.serialize(this.StartTime) 
            and set (value : byte array) = this.StartTime <- (NodaTypeSerializerDelegate.deserialize value typedefof<OffsetDateTime> :?> OffsetDateTime)

        [<Ignore>]
        member val StartTime : OffsetDateTime = new OffsetDateTime() with get, set


        member this.EndTimeData
            with get() : byte array = NodaTypeSerializerDelegate.serialize(this.EndTime) 
            and set (value : byte array) = this.EndTime <- (NodaTypeSerializerDelegate.deserialize value typedefof<OffsetDateTime> :?> OffsetDateTime)


        [<Ignore>]
        member val EndTime : OffsetDateTime = new OffsetDateTime() with get, set
        [<IgnoreAttribute>]
        member val Rooms : Room list = [] with get, set
    
    type ConferenceData() =         
        [<PrimaryKeyAttribute>]
        member val ConferenceId : int = 0 with get, set        
        [<IgnoreAttribute>]
        member val Days : ConferenceDay list = [] with get, set        
        member val Version : string = "" with get, set

        member this.LastSyncData
            with get() : byte array = NodaTypeSerializerDelegate.serialize(this.LastSync) 
            and set (value : byte array) = this.LastSync <- (NodaTypeSerializerDelegate.deserialize value typedefof<OffsetDateTime> :?> OffsetDateTime)


        [<Ignore>]
        member val LastSync : OffsetDateTime = new OffsetDateTime() with get, set

    type EntryFavorite() =        
        [<PrimaryKeyAttribute>]
        member val Guid : Guid = Guid.Empty with get, set
        [<IndexedAttribute>]
        member val ConferenceId : int = 0 with get, set
        member val EntryId : int = -1 with get, set

    [<AllowNullLiteral>]
    type Conference(id : int, name : string, dataUri : string, rawData : string) = 
        let id = id
        let name = name
        let dataUri = dataUri
        let rawData = rawData
        let mutable data : ConferenceData option = None
        member this.Name = name
        member this.Id = id
        member this.DataUri = dataUri
        member this.RawData = rawData
        
        member this.Data 
            with get () = data
            and set (v) = data <- v
