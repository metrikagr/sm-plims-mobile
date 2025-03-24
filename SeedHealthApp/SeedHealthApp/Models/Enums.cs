using System;
using System.Collections.Generic;
using System.Text;

namespace SeedHealthApp.Models
{
    public enum AssayMobileEnum
    {
        BlotterAndFreezePaperTest = 3,
        Elisa = 5,
        GerminationTest = 6,
        Pcr = 8
    }

    public enum StatusEnum
    {
        Pending = 41,
        InProcess = 42,
        Finished = 43
    }

    public enum ReadingDataEntry
    {
        Entry = 67,
        Positive = 68,
        Negative = 70,
        Buffer = 71,
    }
}
