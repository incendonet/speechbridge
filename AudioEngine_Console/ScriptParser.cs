using System;
using System.Net;
using System.IO;
using System.Text;
using System.Collections;
using System.Xml;

namespace ScriptParser
{
	/// <summary>
	/// XElement class.
	/// </summary>
	public class XElement : XAttribute
	{
		public ArrayList	m_alAttributes = new ArrayList();
		public ArrayList	m_alElements = new ArrayList();
		public XElement		m_xeParentElem;

		private int			m_iDepth = 0;	// Absolute (not relative) depth in the tree.

		public XElement()
		{
		}

		public XElement(XElement i_xeParentElem)
		{
			m_xeParentElem = i_xeParentElem;
		}

		public int Depth
		{
			get { return m_iDepth; }
			set { m_iDepth = value; }
		}

		public string GetAttributeVal(string i_sAttrName)
		{
			string		sRet = null;
			int			ii, iAttrs;
			bool		bFound = false;
			XAttribute	xaCurr = null;

			iAttrs = m_alAttributes.Count;
			for(ii = 0; ((ii < iAttrs) && (!bFound) ); ii++)
			{
				xaCurr = (XAttribute)m_alAttributes[ii];
				if(xaCurr.Name == i_sAttrName)
				{
					sRet = xaCurr.Value;
					bFound = true;
				}
			}

			return(sRet);
		}
	}

	/// <summary>
	/// XAttribute class.
	/// </summary>
	public class XAttribute
	{
		private string		m_sName;
		private string		m_sValue;

		public XAttribute()
		{
		}

		public string Name
		{
			get { return m_sName; }
			set { m_sName = value; }
		}

		public string Value
		{
			get { return m_sValue; }
			set { m_sValue = value; }
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public class XmlParser
	{
		/// <summary>
		/// Parses the XML into a more easily accessible node tree.
		/// </summary>
		/// <param name="i_sXml">The XML string</param>
		/// <param name="io_xeRootElem">The root node of the tree.</param>
		public static bool ParseXml(string i_sXml, ref XElement io_xeRootElem)
		{
			bool			bRet = false;
			int				iCurrDepth = 0;
			XElement		xeCurrElem, xePrevElem;

			try
			{
				// Create the XmlNamespaceManager.
				NameTable nt = new NameTable();
				XmlNamespaceManager nsmgr = new XmlNamespaceManager(nt);

				// Create the XmlParserContext.
				XmlParserContext context = new XmlParserContext(null, nsmgr, null, XmlSpace.None);

				// Create the reader.
				XmlTextReader reader = new XmlTextReader(i_sXml, XmlNodeType.Element, context);

				// Set up the node tree
				xePrevElem = xeCurrElem = io_xeRootElem;
				io_xeRootElem.Name = "ROOTNAME - IGNORE";

				// Parse the XML and display each node.
				while (reader.Read())
				{
					switch (reader.NodeType)
					{
						case XmlNodeType.XmlDeclaration :
						{
							// Ignore for now.
						}
						break;
						case XmlNodeType.Element :
						{
							xeCurrElem = new XElement(xePrevElem);

							if(reader.Depth > iCurrDepth)
							{
								// Add the element
								((XElement)(xePrevElem.m_alElements[xePrevElem.m_alElements.Count - 1])).m_alElements.Add(xeCurrElem);
								// Go down a level
								xePrevElem = (XElement)xePrevElem.m_alElements[xePrevElem.m_alElements.Count - 1];
								// Assign the proper parent node
								xeCurrElem.m_xeParentElem = xePrevElem;
								//iCurrDepth++;		// For coding consistency with elseif below, not needed though.
							}
							else if(reader.Depth < iCurrDepth)
							{
								// Go back up
								while(iCurrDepth > reader.Depth)
								{
									xePrevElem = xePrevElem.m_xeParentElem;
									iCurrDepth--;
								}
								// Add the element
								xePrevElem.m_alElements.Add(xeCurrElem);
							}
							else
							{
								// Add the element
								xePrevElem.m_alElements.Add(xeCurrElem);
							}

							xeCurrElem.Name = reader.Name;

							iCurrDepth = xeCurrElem.Depth = reader.Depth;

							//Console.Write("{0} {1},{2}  ", reader.Depth, reader.LineNumber, reader.LinePosition);
							//Console.WriteLine("<{0}>", reader.Name);

							if(reader.HasAttributes)
							{
								XAttribute	xeTmpAttr;

								for(int ii = 0; ii < reader.AttributeCount; ii++)
								{
									reader.MoveToAttribute(ii);

									xeTmpAttr = new XAttribute();
									xeTmpAttr.Name = reader.Name;
									xeTmpAttr.Value = reader.Value;
									xeCurrElem.m_alAttributes.Add(xeTmpAttr);

									//Console.WriteLine("    \"{0}\" = \"{1}\"", reader.Name, reader.Value);
								}
							}
						}
						break;
						case XmlNodeType.Text :
						{
							//Console.Write("{0} {1},{2}  ", reader.Depth, reader.LineNumber, reader.LinePosition);
							//Console.WriteLine("  {0}", reader.Value);
						
							xeCurrElem.Value = reader.Value;
						}
						break;
						case XmlNodeType.EndElement :
						{
							//Console.Write("{0} {1},{2}  ", reader.Depth, reader.LineNumber, reader.LinePosition);
							//Console.WriteLine("</{0}>", reader.Name);
						}
						break;
						case XmlNodeType.Whitespace :
						case XmlNodeType.Comment :
						{
							// It's safe/desired to ignore whitespace and comments.
						}
						break;
						default :
						{
							Console.Error.WriteLine("Warning: ScriptParser.XmlParser read unexpected XmlNodeType '{0}'.", reader.NodeType.ToString());
						}
						break;
					}       
				}           

				// Close the reader.
				reader.Close();

				//Console.WriteLine("\n\r");
			}
			catch(Exception e)
			{
				bRet = false;
				Console.Error.WriteLine("ERROR: ScriptParser.XmlParser caught exception '{0}'.", e.ToString());
			}

			return(bRet);
		}

		/// <summary>
		/// Retrieves the document from the requested URL.  (We're assuming that it is XML.)
		/// </summary>
		/// <param name="i_sURL">URL of the requested document.</param>
		/// <param name="o_sXml">String containing the document.</param>
		/// <returns></returns>
		public static bool GetXmlString(string i_sURL, out string o_sXml)
		{
			WebRequest		wrReq;
			WebResponse		wrResp;
			Uri				uriPage = new Uri(i_sURL);
			StringBuilder	sbString = new StringBuilder("");
			String			sTmp;
			
			o_sXml = "";
			wrReq = WebRequest.Create(uriPage);
			wrResp = wrReq.GetResponse();
			StreamReader srXml = new StreamReader(wrResp.GetResponseStream(), Encoding.UTF8);

			while(srXml.Peek() >= 0)
			{
				sTmp = srXml.ReadLine();
				if(sTmp.Length > 0)
				{
					sbString.Append(sTmp);
				}
			}

			o_sXml = sbString.ToString();

			return(true);		// NEEDS EXCEPTION HANDING!!!
		}

		/// <summary>
		/// Displays the node tree in a text console window.
		/// </summary>
		/// <param name="i_xeRootNode">The root node of the tree.</param>
		public static void DisplayNodeTree(XElement i_xeRootNode)
		{
			int ii, iNumElems, iNumAttrs;

			for(ii = 0; ii < i_xeRootNode.Depth; ii++)
			{
				Console.Write("\t");
			}

			if(i_xeRootNode.Name != null)
			{
				Console.Write("<{0}", i_xeRootNode.Name);
			}

			iNumAttrs = i_xeRootNode.m_alAttributes.Count;
			for(ii = 0; ii < iNumAttrs; ii++)
			{
				Console.Write(" {0}='{1}'", ((XAttribute)(i_xeRootNode.m_alAttributes[ii])).Name, ((XAttribute)(i_xeRootNode.m_alAttributes[ii])).Value);
			}

			if(i_xeRootNode.Value != null)
			{
				Console.WriteLine(">{0}</{1}>", i_xeRootNode.Value, i_xeRootNode.Name);
			}
			else
			{
				Console.WriteLine(">");
			}

			iNumElems = i_xeRootNode.m_alElements.Count;
			for(ii = 0; ii < iNumElems; ii++)
			{
				DisplayNodeTree((XElement)i_xeRootNode.m_alElements[ii]);
			}
		}
	}
}
