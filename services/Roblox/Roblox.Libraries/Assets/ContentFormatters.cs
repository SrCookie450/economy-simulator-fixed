using System.Xml;

namespace Roblox.Libraries.Assets
{
    public static class ContentFormatters
    {
        private static Dictionary<string, string> templateCache { get; } = new();
        private static System.Threading.Mutex templateCacheMux { get; } = new();
        private static string GetTemplate(string templateName)
        {
            templateCacheMux.WaitOne();
            var templateExists = templateCache.ContainsKey(templateName);
            if (templateExists)
            {
                var templateXml = templateCache[templateName];
                templateCacheMux.ReleaseMutex();
                return templateXml;
            }
            templateCacheMux.ReleaseMutex();
            // get from disk
            var templatesLocation = Path.Join(Configuration.XmlTemplatesDirectory, "./" + templateName + ".xml");
            var templateFile = File.ReadAllText(templatesLocation);
            templateCacheMux.WaitOne();
            templateCache[templateName] = templateFile;
            templateCacheMux.ReleaseMutex();
            return templateFile;
        }

        private static string AddTemplateParameter(string templateXml, string paramName, string value)
        {
            templateXml = templateXml.Replace("{" + paramName + "}", value);
            return templateXml;
        }

        private static string GetTemplateOneParam(string templateName, string paramName, string paramValue)
        {
            return AddTemplateParameter(GetTemplate(templateName), paramName, paramValue);
        }
        
        public static string GetTeeShirt(long contentId)
        {
            return GetTemplateOneParam("teeshirt", "assetId", contentId.ToString());
        }
        
        public static string GetShirt(long contentId)
        {
            return GetTemplateOneParam("shirt", "assetId", contentId.ToString());
        }
        
        public static string GetPants(long contentId)
        {
            return GetTemplateOneParam("pants", "assetId", contentId.ToString());
        }
    }
}