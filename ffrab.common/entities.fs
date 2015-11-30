namespace ffrab.mobile.common

module entities = 
    open System
    open NodaTime
    open SQLite.Net
    open SQLite.Net.Attributes

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
        
        member val Start : OffsetDateTime = new OffsetDateTime() with get, set
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
        member val Day : LocalDate = new LocalDate() with get, set
        member val StartTime : OffsetDateTime = new OffsetDateTime() with get, set
        member val EndTime : OffsetDateTime = new OffsetDateTime() with get, set
        [<IgnoreAttribute>]
        member val Rooms : Room list = [] with get, set
    
    type ConferenceData() = 
        
        [<PrimaryKeyAttribute>]
        member val ConferenceId : int = 0 with get, set
        
        [<IgnoreAttribute>]
        member val Days : ConferenceDay list = [] with get, set
        
        member val Version : string = "" with get, set
