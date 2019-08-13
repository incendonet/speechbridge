// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;

using Incendonet.Utilities.LogClient;

namespace XmlDocParser
{
	/// <summary>
	/// XElement class.
	/// </summary>
	public class XElement : XAttribute
	{
		public List<XAttribute>	m_alAttributes = new List<XAttribute>();
		public List<XElement>	m_alElements = new List<XElement>();
		public XElement		m_xeParentElem;

		private int			m_iDepth = 0;	// Absolute (not relative) depth in the tree.

		public XElement() : this("", 0)
		{
		}

		public XElement(string i_sSourceFileName, int i_iApproximateLineNumber)
		{
			SourceFileName = i_sSourceFileName;
			ApproximateLineNumber = i_iApproximateLineNumber;
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
				xaCurr = m_alAttributes[ii];
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
		private int m_iApproximateLineNumber;
		private string m_sSourceFileName;

		public XAttribute() : this("", 0)
		{
		}

		public XAttribute(string i_sSourceFileName, int i_iApproximateLineNumber)
		{
			SourceFileName = i_sSourceFileName;
			ApproximateLineNumber = i_iApproximateLineNumber;
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

		public int ApproximateLineNumber
		{
			get { return m_iApproximateLineNumber; }
			protected set { m_iApproximateLineNumber = value; }
		}

		public string SourceFileName
		{
			get { return m_sSourceFileName; }
			protected set { m_sSourceFileName = value; }
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
		public static bool ParseXml(ILegacyLogger i_Logger, string i_sXml, ref XElement io_xeRootElem)
		{
			return ParseXml(i_Logger, "", i_sXml, ref io_xeRootElem);
		}

		/// <summary>
		/// Parses the XML into a more easily accessible node tree.
		/// </summary>
		/// <param name="i_sFileName">The file from which XML was obtained (used for error reporting only).</param>
		/// <param name="i_sXml">The XML string</param>
		/// <param name="io_xeRootElem">The root node of the tree.</param>
		public static bool ParseXml(ILegacyLogger i_Logger, string i_sFileName, string i_sXml, ref XElement io_xeRootElem)
		{
			bool			bRet = true;
			int				iCurrDepth = 0;
			XElement		xeCurrElem, xeParentElem;

			try
			{
				// Create the XmlNamespaceManager.
				NameTable nt = new NameTable();
				XmlNamespaceManager nsmgr = new XmlNamespaceManager(nt);

				// Create the XmlParserContext.
				XmlParserContext context = new XmlParserContext(null, nsmgr, null, XmlSpace.None);

				// Create the reader.
				using (XmlTextReader reader = new XmlTextReader(i_sXml, XmlNodeType.Element, context))
				{

					// Set up the node tree
					xeParentElem = xeCurrElem = io_xeRootElem;
					io_xeRootElem.Name = "ROOTNAME - IGNORE";

					int lineNumber = 1;

					// Parse the XML and display each node.
					while (reader.Read())
					{
						++lineNumber;

						switch (reader.NodeType)
						{
							case XmlNodeType.XmlDeclaration:
								{
									// Ignore for now.
								}
								break;
							case XmlNodeType.Element:
								{
									xeCurrElem = new XElement(i_sFileName, lineNumber);

									if (reader.Depth > iCurrDepth)
									{
										// Add the element
										xeParentElem.m_alElements[xeParentElem.m_alElements.Count - 1].m_alElements.Add(xeCurrElem);
										// Go down a level
										xeParentElem = xeParentElem.m_alElements[xeParentElem.m_alElements.Count - 1];
									}
									else if (reader.Depth < iCurrDepth)
									{
										// Go back up
										while (iCurrDepth >= reader.Depth)
										{
											xeParentElem = xeParentElem.m_xeParentElem;
											iCurrDepth = xeParentElem.Depth;
										}

										// Add the element
										xeParentElem.m_alElements.Add(xeCurrElem);
									}
									else
									{
										// Add the element
										xeParentElem.m_alElements.Add(xeCurrElem);
									}

									xeCurrElem.m_xeParentElem = xeParentElem;
									xeCurrElem.Name = reader.Name;

									iCurrDepth = xeCurrElem.Depth = reader.Depth;

									if (reader.HasAttributes)
									{
										XAttribute xeTmpAttr;

										for (int ii = 0; ii < reader.AttributeCount; ii++)
										{
											reader.MoveToAttribute(ii);

											xeTmpAttr = new XAttribute(i_sFileName, lineNumber);
											xeTmpAttr.Name = reader.Name;
											xeTmpAttr.Value = reader.Value;
											xeCurrElem.m_alAttributes.Add(xeTmpAttr);
										}
									}
								}
								break;
							case XmlNodeType.Text:
								{
									xeCurrElem.Value = reader.Value;
								}
								break;
							case XmlNodeType.EndElement:
								{
									--lineNumber;
								}
								break;
							case XmlNodeType.Whitespace:
								{
									// It's safe/desired to ignore whitespace and comments.
									--lineNumber;
								}
								break;
							case XmlNodeType.Comment:
								{
									// It's safe/desired to ignore whitespace and comments.
								}
								break;
							case XmlNodeType.CDATA:
								{
									xeCurrElem.Value = reader.Value;
								}
								break;
							default:
								{
									i_Logger.Log(Level.Warning, "XmlDocParser.XmlParser read unexpected XmlNodeType: " + reader.NodeType.ToString());
								}
								break;
						}
					}
				}
			}
			catch(Exception e)
			{
				bRet = false;
				i_Logger.Log(Level.Exception, "XmlDocParser.XmlParser caught exception: " + e.ToString());
			}

			return(bRet);
		}

		/// <summary>
		/// Retrieves the document from the requested URL.  (We're assuming that it is XML.)
		/// </summary>
		/// <param name="i_sURL">URL of the requested document.</param>
		/// <param name="o_sXml">String containing the document.</param>
		/// <returns></returns>
		public static bool GetXmlString(ILegacyLogger i_Logger, string i_sURL, out string o_sXml)
		{
			bool bRet = true;
			
			o_sXml = "";	// Had to be moved before try{} to shut compiler up.

			try
			{
				WebRequest wrReq = WebRequest.Create(new Uri(i_sURL));
				WebResponse wrResp = wrReq.GetResponse();

				using (StreamReader srXml = new StreamReader(wrResp.GetResponseStream(), Encoding.UTF8))
				{
					o_sXml = srXml.ReadToEnd();
				}

				i_Logger.Log(Level.Info, String.Format("XmlDocParser.GetXmlString({0}) read {1} chars.", i_sURL, o_sXml.Length));
			}
			catch(Exception e)
			{
				bRet = false;
				i_Logger.Log(Level.Exception, String.Format("ERROR: XmlDocParser.GetXmlString({0}) caught exception '{1}'.", i_sURL, e.ToString()));
			}

			return(bRet);
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
				Console.Write(" {0}='{1}'", i_xeRootNode.m_alAttributes[ii].Name, i_xeRootNode.m_alAttributes[ii].Value);
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
				DisplayNodeTree(i_xeRootNode.m_alElements[ii]);
			}
		}
	}
}
