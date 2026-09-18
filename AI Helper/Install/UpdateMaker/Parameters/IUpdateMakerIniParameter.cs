namespace AIHelper.Install.UpdateMaker.Parameters
{
    /// <summary>
    /// Parameter of the update maker which is parsed from the ini file
    /// </summary>
    public interface IUpdateMakerIniParameter
    {
        /// <summary>
        /// Parameter name
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Priority in parse order, bigger priority earlier Parse will be executed
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// Value of the parameter from the ini
        /// </summary>
        string IniValue { get; set; }

        /// <summary>
        /// Parse file or dir path
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        string Parse(string filePath);
    }
}
