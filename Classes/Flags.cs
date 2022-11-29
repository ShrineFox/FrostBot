using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostBot
{
    public class Flags
    {
        public static string GetBitConv(string input, bool royalInput, bool royalOutput, bool useSection)
        {
            // If Input is a number, try to parse according to settings
            if (Int32.TryParse(input, out int result))
            {
                int flagId = result;
                if (royalInput && !royalOutput)
                {
                    flagId = ConvertToVanilla(result);
                }
                else if (!royalInput && royalOutput)
                {
                    flagId = ConvertToRoyal(result);
                }
                    
                if (flagId != -1)
                {
                    if (!useSection)
                        return flagId.ToString();
                    return GetFlowscriptCode(flagId, royalOutput);
                }
                return "**Error**: Could not convert flag. Result either overflowed to next section after conversion, " +
                    "or flag val exceeded max val in source array.";
            }
            else
            {
                return "**Error**: Input is not a number.";
            }
        }

        private static string GetFlowscriptCode(int flagId, bool royalBits)
        {
            var section = -1;
            var section_flag = 0;

            int[] bitsArray = sVanillaBits;
            if (royalBits)
                bitsArray = sRoyalBits;

            for (var i = 1; i < bitsArray.Length; i++)
            {
                if (flagId < bitsArray[i])
                {
                    section = i - 1;
                    section_flag = flagId - bitsArray[i - 1];
                    break;
                }
            }

            if (section >= 0)
            {
                var result = bitsArray[section] + section_flag;

                if (result <= bitsArray[section + 1])
                {
                    string text = $"Flag.Section{section} + {section_flag}";
                    if (royalBits)
                        text = text.Replace("Flag", "FlagR");
                    return text;
                }
                else
                {
                    return "**Error**: Could not print section. Overflowed to next section after conversion.";
                }
            }
            else
            {
                return "**Error**: Could not print section. Flag val exceeded max val in source array.";
            }
        }

        public static int[] sVanillaBits = { 0, 2048, 4096, 8192, 8448, 8704, 8960 };
        public static int[] sRoyalBits = { 0, 3072, 6144, 11264, 11776, 12288, 12800 };

        private static int ConvertToRoyal(int flag)
        {
            var section = -1;
            var section_flag = 0;

            // convert
            for (var i = 1; i < sVanillaBits.Length; i++)
            {
                if (flag < sVanillaBits[i])
                {
                    section = i - 1;
                    section_flag = flag - sVanillaBits[i - 1];
                    break;
                }
            }

            // flag val exceeded max val in source array
            if (section < 0)
            {
                return -1;
            }
            else
            {
                var result = sRoyalBits[section] + section_flag;

                // overflowed to next section after conversion
                if (result > sRoyalBits[section + 1])
                {
                    return -1;
                }

                return result;
            }
        }

        public static int ConvertToVanilla(int flag)
        {
            var section = -1;
            var section_flag = 0;

            // convert
            for (var i = 1; i < sRoyalBits.Length; i++)
            {
                if (flag < sRoyalBits[i])
                {
                    section = i - 1;
                    section_flag = flag - sRoyalBits[i - 1];
                    break;
                }
            }

            // flag val exceeded max val in source array
            if (section < 0)
            {
                return -1;
            }
            else
            {
                var result = sVanillaBits[section] + section_flag;

                // overflowed to next section after conversion
                if (result > sVanillaBits[section + 1])
                {
                    return -1;
                }

                return result;
            }
        }
    }
}
