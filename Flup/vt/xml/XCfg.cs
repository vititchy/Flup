using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using vt.common;
using System.Xml.Linq;
using System.Collections;

namespace vt.xml
{
	/// <summary>
	/// xpath tester: http://www.xpathtester.com/test
	/// http://zvon.org/xxl/XPathTutorial/General/examples.html
	/// </summary>
	class XCfg
	{
		private readonly string DEFAULT_EXT = ".cfg.xml"; 
		private string _cfgFileName; 

		private XDocument _xDoc;

		private XPathDocument _xPathDocument;
		//private XPathNavigator _xPathNavigator;

		public XCfg() : this(string.Empty)
		{
		}

		public XCfg(string cfgFileName)
		{
			GetFileName(cfgFileName);
			LoadXml();
		}


		private void GetFileName(string cfgFileName)
		{
			if (!string.IsNullOrEmpty(cfgFileName))
			{
				// budu hledat v zaslanym souboru
				_cfgFileName = cfgFileName;
			}
			else
			{
				// jinak jmeno cfg souboru dle exe - jen odriznu .vshost.exe
				string exeFileName = AppDomain.CurrentDomain.FriendlyName;
				if (exeFileName.EndsWith(".vshost.exe"))
				{
					exeFileName = exeFileName.Substring(0, exeFileName.Length - ".vshost.exe".Length);
				}
				if (exeFileName.EndsWith(".exe"))
				{
					exeFileName = exeFileName.Substring(0, exeFileName.Length - ".exe".Length);
				}
				_cfgFileName = VtStrings.AddSlash(AppDomain.CurrentDomain.BaseDirectory) + exeFileName + DEFAULT_EXT;
			}
		}

		private void LoadXml()
		{
// pada ?
			_xPathDocument = new XPathDocument(_cfgFileName);
			//_xPathNavigator = _xPathDocument.CreateNavigator();

			_xDoc = XDocument.Load(_cfgFileName);

		}



		//public IEnumerable<string> SelectStrings(string xPathExpression)
		//{
		//    var result = new List<string>();

		//    XPathNodeIterator iterator = _xPathNavigator.Select(xPathExpression);
		//    foreach (var i in iterator)
		//    {
		//        var xpathNavigator = i as XPathNavigator;
		//        if (xpathNavigator != null)
		//        {

		//            result.Add(xpathNavigator.Value);
		//        }
		//    }

		//    return result;
		//}


		//public string SelectString(string xPathExpression)
		//{
		//    XPathNavigator xPathNavigator = _xPathNavigator.SelectSingleNode(xPathExpression);

		//    if (xPathNavigator != null)
		//    {
		//        return xPathNavigator.Value;
		//    }

		//    return null;
		//}










		private List<Object> SelectObjects(string xPathQuery)
		{
			if ((_xDoc != null) && !string.IsNullOrEmpty(xPathQuery))
			{
				var x = _xDoc.XPathEvaluate(xPathQuery);

				// http://msdn.microsoft.com/en-us/library/bb341675(v=vs.110).aspx
				// An object that can contain a bool, a double, a string, or an IEnumerable<T>. 

				if (x is bool)
				{
				}
				else if (x is double)
				{
				}
				else if (x is string)
				{
				}
				else if (x is IEnumerable<Object>)
				{
					var xpathResult = ((IEnumerable)x);
					var xObjectsResult = xpathResult.Cast<Object>().ToList();
					return xObjectsResult;
				}
				//else if (x is string)
				//{

				//    var a = new List<string>() { x as string };
				//    var b = new xo List<string>() { x as string };

				//    return a;
				//}
			}
			return null;
		}














		private List<XObject> SelectXObjects(string xPathQuery)
		{
			if ((_xDoc != null) && !string.IsNullOrEmpty(xPathQuery))
			{
				var x = _xDoc.XPathEvaluate(xPathQuery);

				// http://msdn.microsoft.com/en-us/library/bb341675(v=vs.110).aspx
				// An object that can contain a bool, a double, a string, or an IEnumerable<T>. 


// ROZDELIT ? NA bool, double ... a IENUMARATOR ???

				//if (x is IEnumerable<XObject>)
				{
					var xpathResult = ((IEnumerable)x);
					var xObjectsResult = xpathResult.Cast<XObject>().ToList();
					return xObjectsResult;
				}
				//else if (x is string)
				//{

				//    var a = new List<string>() { x as string };
				//    var b = new xo List<string>() { x as string };

				//    return a;
				//}
			}
			return null;
		}



		private XObject SelectXObject(string xPathQuery)
		{
			var xObjects = SelectXObjects(xPathQuery);
			if ((xObjects != null) && xObjects.Any())
			{
				return xObjects.FirstOrDefault();
			}
			return null;
		}	
		



		//public List<T> XPathSelect<T>(string xPathQuery) where T :XObject
		//{
		//    var objects = SelectXObjects(xPathQuery);

		//    if (objects != null)
		//    {
		//        var xAtt = objects.Where(p => p.GetType() == typeof(T)).Cast<T>().ToList();
		//        return xAtt;
		//    }

		//    return null;
		//}

		//public T XPathSelectFirst(string xPathQuery) //where T : XObject
		//{
		//    var r = XPathSelect<T>(xPathQuery);
		//    if ((r != null) && (r.Count >= 1))
		//    {
		//        return r.FirstOrDefault();
		//    }

		//    return null;

		//}

		public string GetString(string xPathQuery)
		{
			var xObject = SelectXObject(xPathQuery);
			return GetStringValue(xObject);
		}



		public List<string> GetStrings(string xPathQuery)
		{
			var result = new List<string>();
			var xObjects = SelectXObjects(xPathQuery);
			foreach (var x in xObjects)
			{
				result.Add(GetStringValue(x));
			}
			return result;
		}




		private string GetStringValue(XObject xObject)
		{
			if (xObject != null)
			{
				if (xObject is XAttribute)
				{
					return (xObject as XAttribute).Value;
				}
				else if (xObject is XElement)
				{
					return (xObject as XElement).Value;

					//if ((xElement != null) && (xElement.FirstNode != null))
					//{
					//    var firstNode = xElement.FirstNode;
					//    if (firstNode is XText)
					//    {
					//        var nodeValue = (firstNode as XText).Value;
					//        return nodeValue;
					//    }
					//}
				}
			}
			return null;
		}








		public IEnumerable Test()
		{
			try
			{

				//string xmlFilePath = "exampleWithFruits.xml";
				string xPathQuery = "//database/server//@color";
				xPathQuery = "//database/server";

				var doc = XDocument.Load(_cfgFileName);
				IEnumerable att = (IEnumerable)doc.XPathEvaluate(xPathQuery);
				//string[] matchingValues = att.Cast<XAttribute>().Select(x => x.Value).ToArray();




				xPathQuery = "//database/@key";
				var a = doc.XPathEvaluate(xPathQuery);

				var b = ((IEnumerable)doc.XPathEvaluate(xPathQuery));

				var c = b.Cast<object>().ToList();


				xPathQuery = "/jkr/database";
				var b2 = ((IEnumerable)doc.XPathEvaluate(xPathQuery));

				var c2 = b2.Cast<object>().ToList();





				var x1 = GetString("/jkr/database");
				var x2 = GetString("/jkr/bevo/@name");
				var x3 = GetStrings("/jkr/bevo/@name");





				foreach (XAttribute aaaa in b)
				{
					var x = aaaa.Name + aaaa.Value;
				}

				List<string> myList = new List<string>();
				IEnumerable<string> myEnumerable = myList;
				List<string> listAgain = myEnumerable.ToList();

				return att;
			}
			catch(Exception ex)
			{

			}

			return null;
		}

	}
}
