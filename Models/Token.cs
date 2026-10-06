using System;

namespace TokenApp.Models
{
    public enum TokenType
    {
        Yellow = 0,
        Red = 1,
        Emergency = 2,
        Other = 3
    }

    public enum TokenStatus
    {
        Given = 0,
        SanctumEntry = 1,
        VipEscort = 2,
        Pending = 3
    }

    [Flags]
    public enum TokenActions
    {
        None = 0,
        Reprint = 1,
        Details = 2,
        Escort = 4,
        Export = 8,
        Print = 16
    }
}
