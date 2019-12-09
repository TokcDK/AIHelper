namespace AI_Helper.Manage
{
    class ManageStrings
    {
        /// <summary>
        /// Check if string A contains string B (if length of A > length of A with replaced B by "")<br></br><br></br>
        /// Speed check tests: https://cc.davelozinski.com/c-sharp/fastest-way-to-check-if-a-string-occurs-within-a-string
        /// </summary>
        /// <param name="StringAWhereSearch"></param>
        /// <param name="StringBToSearch"></param>
        /// <returns></returns>
        public static bool IsStringAContainsStringB(string StringAWhereSearch, string StringBToSearch)
        {
            int StringAInWhichSearchLength = StringAWhereSearch.Length;
            if (StringAInWhichSearchLength > 0 && StringBToSearch.Length > 0)//safe check for empty values
            {
                //if string A contains string B then string A with replaced stringB by empty will be
                return StringAInWhichSearchLength > StringAWhereSearch.Replace(StringBToSearch, string.Empty).Length;
            }
            return false;

        }

        public static bool IsStringContainsAnyExclusion(string InputString, string[] exclusions)
        {
            if (InputString.Length > 0 && exclusions != null)
            {
                int exclusionsLength = exclusions.Length;
                for (int i = 0; i < exclusionsLength; i++)
                {
                    if (IsStringAContainsStringB(InputString, exclusions[i]))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool IsStringAequalsStringB(string StringA, string StringB, bool ignoreCase = false)
        {
            if (ignoreCase)
            {
                return string.Compare(StringA, StringB, ignoreCase) == 0;
            }
            else
            {
                return string.CompareOrdinal(StringA, StringB) == 0;
            }
        }

        public static string AddStringBToAIfValid(string StringA, string StringB)
        {
            //добавление имени автора в начало имени папки
            if (StringA.StartsWith("[AI][") || (StringA.StartsWith("[") && !StringA.StartsWith("[AI]")) || ManageStrings.IsStringAContainsStringB(StringA, StringB))
            {
            }
            else if (StringB.Length > 0)
            {
                //проверка на любые невалидные для имени папки символы
                if (ManageFilesFolders.ContainsAnyInvalidCharacters(StringB))
                {
                }
                else
                {
                    StringA = "[" + StringB + "]" + StringA;
                }
            }

            return StringA;
        }

        public static bool IsStringAContainsAnyStringFromStringArray(string StringA, string[] StringArray, bool IgnoreCase = false)
        {
            foreach (var StringB in StringArray)
            {
                if (IgnoreCase)
                {
                    if (string.Compare(StringA, StringB, true)==0)
                    {
                        return true;
                    }
                }
                else
                {
                    if (IsStringAContainsStringB(StringA, StringB))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
