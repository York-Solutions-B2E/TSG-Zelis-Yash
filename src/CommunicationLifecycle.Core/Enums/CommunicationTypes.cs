namespace CommunicationLifecycle.Core.Enums;

public static class CommunicationTypes
{
    public const string EOB = "EOB"; // Explanation of Benefits
    public const string EOP = "EOP"; // Explanation of Payment
    public const string ID_CARD = "ID_CARD"; // Member ID Card
    public const string WELCOME_PACKET = "WELCOME_PACKET";
    public const string CLAIM_STATEMENT = "CLAIM_STATEMENT";
    public const string PROVIDER_STATEMENT = "PROVIDER_STATEMENT";
    
    public static readonly string[] All = 
    {
        EOB, EOP, ID_CARD, WELCOME_PACKET, CLAIM_STATEMENT, PROVIDER_STATEMENT
    };
    
    public static readonly Dictionary<string, string> DisplayNames = new()
    {
        { EOB, "Explanation of Benefits" },
        { EOP, "Explanation of Payment" },
        { ID_CARD, "Member ID Card" },
        { WELCOME_PACKET, "Welcome Packet" },
        { CLAIM_STATEMENT, "Claim Statement" },
        { PROVIDER_STATEMENT, "Provider Statement" }
    };
} 