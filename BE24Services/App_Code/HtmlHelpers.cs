using System.IO;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace BE24Services.App_Code
{
    public static class HtmlHelpers
    {
        public static HtmlString RenderInclude(this IHtmlHelper helper, string path)
        {
            // check the cache
            string output = File.ReadAllText(path);
            return new HtmlString(output);
        }

        public static HtmlString IncludeFiles(this IHtmlHelper helper, string path, string mask)
        {
            var directoryInfo = new DirectoryInfo(path);
            var files = directoryInfo.GetFiles(mask);
            var output = string.Empty;
            foreach (FileInfo file in files)
            {
                output = output  + File.ReadAllText(path + file.Name);
            }
            return new HtmlString(output);
        }

        public static HtmlString IncludeScript(this IHtmlHelper helper, string path, string jsPath, string mask)
        {
            var directoryInfo = new DirectoryInfo(path);
            var files = directoryInfo.GetFiles(mask);
            var output = string.Empty;
            foreach (FileInfo file in files)
            {
                output = output + "<script src=\"" + jsPath + file.Name + "\"></script>";
            }
            return new HtmlString(output);
        }

    }
}