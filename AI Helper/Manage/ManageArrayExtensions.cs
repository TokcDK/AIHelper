namespace AIHelper.Manage
{
    static class ManageArrayExtensions
    {
        /// <summary>
        /// Trim each element of the array
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        internal static string[] TrimEachValue(this string[] array)
        {
            for (int i = 0; i < array.Length; i++) array[i] = array[i].Trim();

            return array;
        }
    }
}
