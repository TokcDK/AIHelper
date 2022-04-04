using AI_Helper.ViewModel;
using NGettext;
using System.IO;

//
// Usage:
//		T._("Hello, World!"); // GetString
//		T._n("You have {0} apple.", "You have {0} apples.", count, count); // GetPluralString
//		T._p("Context", "Hello, World!"); // GetParticularString
//		T._pn("Context", "You have {0} apple.", "You have {0} apples.", count, count); // GetParticularPluralString
//
//  https://habr.com/ru/post/432786/
//  https://github.com/VitaliiTsilnyk/NGettext
namespace Localization
{
    public static class T
    {
        static Catalog GetCatalog()
        {
            //_Catalog = new Catalog("en", localesDir, new CultureInfo("en-EN"));
            //_Catalog = new Catalog("ru", localesDir, new CultureInfo("ru-RU"));
            //_Catalog = new Catalog("helper", localesDir);
            return new Catalog("helper", Path.Combine(MainViewModel.AppDir, "RES", "locale")); ;
        }

        //private static readonly ICatalog _Catalog = new Catalog("Example", "./locale");
        private static readonly ICatalog Catalog = GetCatalog();


#pragma warning disable IDE1006 // Стили именования
#pragma warning disable CA1707 // Identifiers should not contain underscores
        public static string _(string text) => Catalog.GetString(text);

        public static string _(string text, params object[] args) => Catalog.GetString(text, args);
        public static string _n(string text, string pluralText, long n) => Catalog.GetPluralString(text, pluralText, n);


        public static string _n(string text, string pluralText, long n, params object[] args) => Catalog.GetPluralString(text, pluralText, n, args);

        public static string _p(string context, string text) => Catalog.GetParticularString(context, text);

        public static string _p(string context, string text, params object[] args) => Catalog.GetParticularString(context, text, args);

        public static string _pn(string context, string text, string pluralText, long n) => Catalog.GetParticularPluralString(context, text, pluralText, n);

        public static string _pn(string context, string text, string pluralText, long n, params object[] args) => Catalog.GetParticularPluralString(context, text, pluralText, n, args);
#pragma warning restore CA1707 // Identifiers should not contain underscores
#pragma warning restore IDE1006 // Стили именования
    }
}