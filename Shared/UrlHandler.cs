using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared;
public class UrlHandler
{
    public static string GetFileNameFromUrl(string url)
    {
        Uri uri = new Uri(url);
        return (uri.LocalPath).ToString();
    }

}
