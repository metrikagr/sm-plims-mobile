using System;
using System.Collections.Generic;
using System.Text;

namespace SeedHealthApp.Helpers
{
    public static class Utils
    {
        public static string ConvertToSampleTypeIndicator(int sampleTypeId)
        {
            switch (sampleTypeId)
            {
                case 29:
                    return "I";
                case 30:
                    return "G";
                case 31:
                    return "S";
                default:
                    throw new Exception("Invalid sample type id");
            }
        }
        public static string ConvertPositionToPositionTitle(int iRow, int iCol)
        {
            int ACharCode = 65;
            string positionTitle = $"{(char)(ACharCode + iRow)}{iCol + 1}";
            return positionTitle;
        }

        public static string FormatSampleNameAsTitle(string sampleName, int sampleNamePart = 0)
        {
            try
            {
                var dotIndex = sampleName.IndexOf('.');
                switch (sampleNamePart)
                {
                    case 0:
                        var requestCode = sampleName.Substring(0, dotIndex);
                        var sampleCode = sampleName.Substring(dotIndex + 1, sampleName.Length - dotIndex - 1);
                        return requestCode + Environment.NewLine + sampleCode;
                    case 1:
                        return sampleName.Substring(0, dotIndex); ;
                    case 2:
                        return sampleName.Substring(dotIndex + 1, sampleName.Length - dotIndex - 1);
                    default:
                        throw new Exception("Sample name part not supported");
                }
            }
            catch
            {
                return sampleName;
            }
        }
    }
}
