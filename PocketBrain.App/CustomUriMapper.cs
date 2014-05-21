using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace PocketBrain.App
{
    /// <summary>
    /// The custom URI mapper.
    /// </summary>
    public class CustomUriMapper : UriMapperBase
    {
        /// <summary>
        /// Maps the given URI.
        /// </summary>
        /// <param name="uri">The URI to map.</param>
        /// <returns>THe mapped URI.</returns>
        public override Uri MapUri(Uri uri)
        {
            string tempUri = HttpUtility.UrlDecode(uri.ToString());

            // URI association launch for pocketbrain
            // /Protocol/
            if (tempUri.Contains("/Protocol"))
            {
                int newNoteIndexPosition = tempUri.IndexOf(string.Format("newNote?"));

                if (newNoteIndexPosition == -1)
                {
                    return new Uri(string.Format("/MainPage.xaml"), UriKind.Relative);
                }

                newNoteIndexPosition += "newNote?".Length;
                string parameters = tempUri.Substring(newNoteIndexPosition);

                return new Uri(string.Format("/NotePage.xaml?{0}", parameters), UriKind.Relative);
            }

 

            // Otherwise perform normal launch.
            return uri;
        }
    }
}
