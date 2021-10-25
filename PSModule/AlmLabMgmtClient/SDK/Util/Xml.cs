using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace PSModule.AlmLabMgmtClient.SDK.Util
{
    public static class Xml
    {
        private const string FIELDS = "Fields";
        private const string FIELD = "Field";
        private const string NAME = "Name";
        private const string VALUE = "Value";
        private const string ENTITY = "Entity";
        private const string TOTAL_RESULTS = "TotalResults";

        public static string GetAttributeValue(string xml, string attrName)
        {
            try
            {
                var doc = XDocument.Parse(xml);
                if (doc?.Root?.Element(FIELDS)?.HasElements == true)
                {
                    var fields = doc.Root.Element(FIELDS).Elements(FIELD);
                    foreach (XElement field in fields)
                    {
                        if (field.Attribute(NAME)?.Value == attrName)
                            return field.Element(VALUE)?.Value;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
            return string.Empty;
        }
        public static IList<IDictionary<string, string>> ToEntities(string xml)
        {
            var list = new List<IDictionary<string, string>>();
            try
            {
                var doc = XDocument.Parse(xml);
                var entities = doc.Root.Elements(ENTITY);
                foreach (var entity in entities)
                {
                    var newEntity = new Dictionary<string, string>();
                    var fields = entity.Element(FIELDS).Elements(FIELD);
                    foreach (var field in fields)
                        if (field.HasAttributes)
                            newEntity.Add(field.Attributes().First().Value, field.Element(VALUE)?.Value);
                    list.Add(newEntity);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }

            return list;
        }

        internal static bool HasResults(string xml)
        {
            bool ok = false;
            try
            {
                var doc = XDocument.Parse(xml);
                if (doc.Root.HasAttributes && int.TryParse(doc.Root.Attribute(TOTAL_RESULTS).Value, out int res))
                { 
                    ok = res > 0;
                }
                else if (doc.Root.HasElements && doc.Root.Elements(ENTITY).Any())
                {
                    ok = true;
                }
                return ok;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }
    }
}
