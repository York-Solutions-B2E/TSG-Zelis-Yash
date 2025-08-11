namespace CommunicationLifecycle.Core.Enums;

public static class CommunicationStatus
{
    // Creation Phase
    public const string ReadyForRelease = "ReadyForRelease";
    public const string Released = "Released";
    
    // Production Phase
    public const string QueuedForPrinting = "QueuedForPrinting";
    public const string Printed = "Printed";
    public const string Inserted = "Inserted";
    public const string WarehouseReady = "WarehouseReady";
    
    // Logistics Phase
    public const string Shipped = "Shipped";
    public const string InTransit = "InTransit";
    public const string Delivered = "Delivered";
    public const string Returned = "Returned";
    
    // Additional Statuses
    public const string Failed = "Failed";
    public const string Cancelled = "Cancelled";
    public const string Expired = "Expired";
    public const string Archived = "Archived";
    
    public static readonly string[] All = 
    {
        ReadyForRelease, Released,
        QueuedForPrinting, Printed, Inserted, WarehouseReady,
        Shipped, InTransit, Delivered, Returned,
        Failed, Cancelled, Expired, Archived
    };
    
    public static readonly Dictionary<string, string> Descriptions = new()
    {
        { ReadyForRelease, "Communication is ready to be released" },
        { Released, "Communication has been released" },
        { QueuedForPrinting, "Communication is queued for printing" },
        { Printed, "Communication has been printed" },
        { Inserted, "Communication has been inserted into envelope/package" },
        { WarehouseReady, "Communication is ready at warehouse" },
        { Shipped, "Communication has been shipped" },
        { InTransit, "Communication is in transit" },
        { Delivered, "Communication has been delivered" },
        { Returned, "Communication was returned" },
        { Failed, "Communication processing failed" },
        { Cancelled, "Communication was cancelled" },
        { Expired, "Communication has expired" },
        { Archived, "Communication has been archived" }
    };
} 