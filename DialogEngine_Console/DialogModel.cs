// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;
using System.Collections;
using System.Collections.Specialized;

namespace DialogModel
{
	/// <summary>
	/// 
	/// </summary>
	public class DSession
	{
		protected	int				m_iVMC;						// ID of virtual media channel
		public		DVariables		m_DVariables;			// Array of DVariable // FIX - change this to a NameObjectCollectionBase or NameValueCollection (for efficiency of searching.)
        public      DProperties     m_DProperties;

		//public		ExperienceLevel	m_ExperienceLevel;

		public enum eVars
		{
			// NOTE:  Most of these directly correspond to VoiceXML session variables, and may not be available in other environments
			connection_local_uri,
			connection_remote_uri,
			connection_remote_displayname,
			connection_protocol_name,
			connection_originator,
            speechbridge_session_id,
		};

		public DSession(int i_iVMC)
		{
			m_iVMC = i_iVMC;
			m_DVariables = new DVariables();

			// Add default variables
			m_DVariables.Add(new DVariable(eVars.connection_local_uri.ToString(), ""));
			m_DVariables.Add(new DVariable(eVars.connection_remote_uri.ToString(), ""));
			m_DVariables.Add(new DVariable(eVars.connection_remote_displayname.ToString(), ""));
			m_DVariables.Add(new DVariable(eVars.connection_protocol_name.ToString(), ""));
			m_DVariables.Add(new DVariable(eVars.connection_originator.ToString(), ""));
            m_DVariables.Add(new DVariable(eVars.speechbridge_session_id.ToString(), ""));

            m_DProperties = new DProperties();
		}

		public int VMC
		{
			get { return(m_iVMC); }
			//set { m_iVMC = value; }
		}

		public DVariable FindDVariable(string i_sName)
		{
			int			ii = 0, iNum = 0;
			DVariable	dvTmp = null;

			iNum = m_DVariables.Count;
			for(ii = 0; ii < iNum; ii++)	// Obviously only efficient for a small number of variables.
			{
				dvTmp = m_DVariables[ii];
				if(dvTmp.Name == i_sName)
				{
					return(dvTmp);
				}
			}

			return(null);
		}
	}

	/// <summary>
	/// Corresponding to the document/page loaded from the web server.
	/// </summary>
	public class DDocument
	{
		public		DSession		m_dsParentSession = null;
		public		DVariables		m_DVariables;			// Array of DVariable // FIX - change this to a NameObjectCollectionBase or NameValueCollection (for efficiency of searching.)
        public      DProperties     m_DProperties;
		public		DForms			m_DForms;				// Array of DForms.
		public		int				m_iActiveForm;			// Index of active form.
		public		string			m_sApplication;			// Name of application of which this document is a part.
		public		string			m_sLang = "en-US";		// Languange, defaulting to US english

		public DDocument(DSession i_dsParentSession)
		{
			m_dsParentSession = i_dsParentSession;
			m_DVariables = new  DVariables();
            m_DProperties = new DProperties();
			m_DForms = new DForms();
			m_iActiveForm = 0;
		}

		public DVariable FindDVariable(string i_sName)
		{
			int			ii = 0, iNum = 0;
			DVariable	dvTmp = null;

			iNum = m_DVariables.Count;
			for(ii = 0; ii < iNum; ii++)	// Obviously only efficient for a small number of variables.
			{
				dvTmp = m_DVariables[ii];
				if(dvTmp.Name == i_sName)
				{
					return(dvTmp);
				}
			}

			return(null);
		}
	}

	/// <summary>
	/// The "base" of a DForm or DField.
	/// Rationale - There are many times where Forms and Fields are used in similar circumstances
	/// where common members are passed in parameters (like DActions, DVariables, etc.) and duplicate
	/// code is written that complicates maintenance.  Instead, we can pass a DSubdocument and check
	/// the type when it is used.
	/// </summary>
	public interface ISubdocContext			// FIX - Terrible name
	{
		string			ID					{ get; set; }
		DVariables		Variables			{ get; set; }
		DActions		ActionsPre			{ get; set; }
	}

	/// <summary>
	/// 
	/// </summary>
	public class DForm : ISubdocContext
	{
		private string			m_sID = "";					// Form ID.  Most often a URI.
		private	DActions		m_DActionsPre;				// Array of DAction-s to iterate through when form is "activated".  // FIX - Need to handle multiple WAVs/TTSs per prompt AND multiple prompts.
		private	DVariables		m_DVariables;				// Array of DVariables, used in addition to responses given by user.	// FIX - change this to a NameObjectCollectionBase or NameValueCollection (for efficiency of searching.)
        private DProperties     m_DProperties;

		public	DDocument		m_ddParentDocument = null;
		public	string			m_sName = "";					// Name of the form, often used in reference.
		//public	DForm			m_ParentForm;				// Reference to this form's predecessor.

		// FIX - The following DActions is not sufficient for VoiceXML, as its <block> elements can occur anywhere
		// in the form.  To properly handle that, DField and a new type should be derived from a base class, of
		// which an array in the form is iterated through.

		public	DFields			m_DFields;					// Array of DField-s that make up the 'fields' of the form.
		public	int				m_iActiveField;				// Index of active field.
		public	DResponse		m_ChosenResponse;			// DResponse of form completion (ie., 'ok' button, etc.)

		public DForm(DDocument i_ddParentDocument/*DForm i_Parent*/)
		{
			m_ddParentDocument = i_ddParentDocument;
			//m_ParentForm = i_Parent;
			m_DActionsPre = new DActions();
			m_DFields = new DFields();
			m_iActiveField = 0;
			m_DVariables = new DVariables();
            m_DProperties = new DProperties();
		}

		public DVariable FindDVariable(string i_sName)
		{
			int			ii = 0, iNum = 0;
			DVariable	dvTmp = null;

			iNum = m_DVariables.Count;
			for(ii = 0; ii < iNum; ii++)	// Obviously only efficient for a small number of variables.
			{
				dvTmp = m_DVariables[ii];
				if(dvTmp.Name == i_sName)
				{
					return(dvTmp);
				}
			}

			return(null);
		}

		public string ID
		{
			get
			{
				return (m_sID);
			}
			set
			{
				m_sID = value;
			}
		}

		public DVariables Variables
		{
			get
			{
				return(m_DVariables);
			}
			set
			{
				m_DVariables = value;
			}
		}

        public DProperties Properties
        {
            get { return m_DProperties; }
            set { m_DProperties = value; }
        }

		public DActions ActionsPre
		{
			get
			{
				return(m_DActionsPre);
			}
			set
			{
				m_DActionsPre = value;
			}
		}
	} // DForm

	public class DForms : CollectionBase
	{
		public int Add(DForm i_Elem)
		{
			return(List.Add(i_Elem));
		}

		public bool Contains(DForm i_Elem)
		{
			return(List.Contains(i_Elem));
		}

		public int IndexOf(DForm i_Elem)
		{
			return(List.IndexOf(i_Elem));
		}

		public void Insert(int i_iIndex, DForm i_Elem)
		{
			List.Insert(i_iIndex, i_Elem);
		}

		public void Remove(DForm i_Elem)
		{
			List.Remove(i_Elem);
		}

		public DForm this[int i_iIndex]
		{
			get {	return((DForm)List[i_iIndex]);	}
			set {	List[i_iIndex] = value;	}
		}

		protected override void OnValidate(object i_oValue)
		{
			Type t1, t2;

			t1 = i_oValue.GetType();
			//t2 = Type.GetType("DialogModel.DForm");
			t2 = typeof(DialogModel.DForm);

			if(t1 != t2)
			{
				throw new ArgumentException("Value must be of type DialogModel.DForm.", "i_oValue");
			}
		}
	} // DForms

	/// <summary>
	/// 
	/// Note - It would have been more readable to use 'DPrompt[]' and 'DResponse' rather than 'ArrayList'
	/// declarations, but the XML doc is parsed in one pass, so each node would be added as it is read.
	/// </summary>
	public class DField : ISubdocContext
	{
		private string			m_sID = "";					// Field ID.  ("name" in VXML.)
		private	DActions		m_DActionsPre = null;		// Array of DAction-s to iterate through when field is "activated".  // FIX - Need to handle multiple WAVs/TTSs per prompt AND multiple prompts.  // FIX - seperate prompts for different ExperienceLevel-s?
		private	DVariables		m_DVariables;				// Array of DVariable-s	// FIX - change this to a NameObjectCollectionBase or NameValueCollection (for efficiency of searching.)
        private DProperties     m_DProperties;

		public enum eVars
		{
			utterance,
			inputmode,
			confidence,
		};

		public enum eInputModes
		{
			speech,
			dtmf,
			keyboard,		// Do kb & mouse have to be separate?
			mouse,
			pen,
			// ...
		};

		////////////////////////////////////////////////////////
		// These are set before prompting the user
		public	DForm			m_dfParentForm = null;
		//public	DField		m_Prev;						// Previous field in the form.
		//public	DField		m_Next;						// Next field in the form.
		public	int				m_iResponseTimeout;			// In seconds, how long to wait before reprompting.
		public	int				m_iFinalIteration;			// Number of times to reprompt before cancelling field.	// FIX - what about cancelling the form?
		public	int				m_iIterationCount;			// Current iteration count.
		public	DActions		m_DActionsPost = null;		// Array of DAction-s to iterate through after reco responses have been handled, but before moving on to the next field.
		public	DActions		m_Errors_NoMatch = null;	// When the spoken result doesn't match an option
		public	DActions		m_Errors_NoInput = null;	// When the spoken result is empty
		public	DResponses		m_Options = null;			// Array of possible DResponse-s (if grammar is used, results must match these.)
		public	DGrammar		m_DGrammar = null;			// External or inline grammar to use (optional.)
		public	string			m_sHelp = "";				// Help string for this stage of the dialog

		////////////////////////////////////////////////////////
		// These are set after getting user response
		public	DResponse		m_Response;					// What the user selected.

		public DField(DForm i_dfParentForm)
		{
			m_dfParentForm = i_dfParentForm;
			m_sID = "";
			m_iResponseTimeout = 15;
			m_iFinalIteration = 1;
			m_iIterationCount = 0;

			m_DActionsPre = new DActions();
			m_DActionsPost = new DActions();
			m_Errors_NoMatch = new DActions();
			m_Errors_NoInput = new DActions();
			m_Options = new DResponses();
			m_DVariables = new DVariables();
            m_DProperties = new DProperties();

			// Add in default field variables
			m_DVariables.Add(new DVariable(eVars.inputmode.ToString(), eInputModes.speech.ToString()));		// Default to speech
			m_DVariables.Add(new DVariable(eVars.utterance.ToString(), ""));
			m_DVariables.Add(new DVariable(eVars.confidence.ToString(), ""));
		}

		public DVariable FindDVariable(string i_sName)
		{
			int			ii = 0, iNum = 0;
			DVariable	dvTmp = null;

			iNum = m_DVariables.Count;
			for(ii = 0; ii < iNum; ii++)	// Obviously only efficient for a small number of variables.
			{
				dvTmp = m_DVariables[ii];
				if(dvTmp.Name == i_sName)
				{
					return(dvTmp);
				}
			}

			return(null);
		}

		public string ID
		{
			get
			{
				return (m_sID);
			}
			set
			{
				m_sID = value;
			}
		}

		public DVariables Variables
		{
			get
			{
				return (m_DVariables);
			}
			set
			{
				m_DVariables = value;
			}
		}

        public DProperties Properties
        {
            get { return m_DProperties; }
            set { m_DProperties = value; }
        }

		public DActions ActionsPre
		{
			get
			{
				return (m_DActionsPre);
			}
			set
			{
				m_DActionsPre = value;
			}
		}
	} // DField

	public class DFields : CollectionBase
	{
		public int Add(DField i_Elem)
		{
			return(List.Add(i_Elem));
		}

		public bool Contains(DField i_Elem)
		{
			return(List.Contains(i_Elem));
		}

		public int IndexOf(DField i_Elem)
		{
			return(List.IndexOf(i_Elem));
		}

		public void Insert(int i_iIndex, DField i_Elem)
		{
			List.Insert(i_iIndex, i_Elem);
		}

		public void Remove(DField i_Elem)
		{
			List.Remove(i_Elem);
		}

		public DField this[int i_iIndex]
		{
			get {	return((DField)List[i_iIndex]);	}
			set {	List[i_iIndex] = value;	}
		}

		protected override void OnValidate(object i_oValue)
		{
			Type t1, t2;

			t1 = i_oValue.GetType();
			//t2 = Type.GetType("DialogModel.DField");
			t2 = typeof(DialogModel.DField);

			if(t1 != t2)
			{
				throw new ArgumentException("Value must be of type DialogModel.DField.", "i_oValue");
			}
		}
	} // DFields

	/// <summary>
	/// A response can be a spoken response to a prompt, a button click, a string entered into an edit field, etc.
	/// </summary>
	public class DResponse
	{
		public	string			m_sValue = "";					// Symantic value
		//public	DActions		m_DActions = null;				// DAction(s) to be performed sequentially.  Optional.
		// Conditions?

		public DResponse()
		{
			//m_DActions = new DActions();
		}
	} // DResponse

	public class DResponses : CollectionBase
	{
		public int Add(DResponse i_Elem)
		{
			return(List.Add(i_Elem));
		}

		public bool Contains(DResponse i_Elem)
		{
			return(List.Contains(i_Elem));
		}

		public int IndexOf(DResponse i_Elem)
		{
			return(List.IndexOf(i_Elem));
		}

		public void Insert(int i_iIndex, DResponse i_Elem)
		{
			List.Insert(i_iIndex, i_Elem);
		}

		public void Remove(DResponse i_Elem)
		{
			List.Remove(i_Elem);
		}

		public DResponse this[int i_iIndex]
		{
			get {	return((DResponse)List[i_iIndex]);	}
			set {	List[i_iIndex] = value;	}
		}

		protected override void OnValidate(object i_oValue)
		{
			Type t1, t2;

			t1 = i_oValue.GetType();
			//t2 = Type.GetType("DialogModel.DResponse");
			t2 = typeof(DialogModel.DResponse);

			if(t1 != t2)
			{
				throw new ArgumentException("Value must be of type DialogModel.DResponse.", "i_oValue");
			}
		}
	} // DResponses

	/// <summary>
	/// 
	/// </summary>
	public class DAction
	{
		public enum DActionType
		{
			eUnknown,
			eScript,
			eSubmitResponses,
			eLink,
			eLinkExpr,
			eCondition,
			eCC_HangupImmediate,
			eCC_HangupAfterPrompts,
			eCC_TransferSession,
			//eCC_...
			ePlayPrompt,
			eNextElement,
			eLog,
		}

		public DActionType		m_Type = DActionType.eUnknown;
		public object			m_oValue = null;					// Could be a DPrompt, DField, string(URL), ...  The type is defined by mType.

		public DAction()
		{
		}
	} // DAction

	public class DActions : CollectionBase
	{
		public int Add(DAction i_Elem)
		{
			return(List.Add(i_Elem));
		}

		public bool Contains(DAction i_Elem)
		{
			return(List.Contains(i_Elem));
		}

		public int IndexOf(DAction i_Elem)
		{
			return(List.IndexOf(i_Elem));
		}

		public void Insert(int i_iIndex, DAction i_Elem)
		{
			List.Insert(i_iIndex, i_Elem);
		}

		public void Remove(DAction i_Elem)
		{
			List.Remove(i_Elem);
		}

		public DAction this[int i_iIndex]
		{
			get {	return((DAction)List[i_iIndex]);	}
			set {	List[i_iIndex] = value;	}
		}

		protected override void OnValidate(object i_oValue)
		{
			Type t1, t2;

			t1 = i_oValue.GetType();
			//t2 = Type.GetType("DialogModel.DAction");
			t2 = typeof(DialogModel.DAction);

			if(t1 != t2)
			{
				throw new ArgumentException("Value must be of type DialogModel.DAction.", "i_oValue");
			}
		}
	} // DActions

	/// <summary>
	/// 
	/// </summary>
	public class DCondition
	{
		public string		m_sStatement = "";		// Boolean statement to evaluate.  If the string is empty, this is the 'default' case.
		public DActions		m_Actions;				// Actions to perform if Statement is true.

		public DCondition()
		{
			m_Actions = new DActions();
		}
	}

	public class DConditions : CollectionBase
	{
		public int Add(DCondition i_Elem)
		{
			return(List.Add(i_Elem));
		}

		public bool Contains(DCondition i_Elem)
		{
			return(List.Contains(i_Elem));
		}

		public int IndexOf(DCondition i_Elem)
		{
			return(List.IndexOf(i_Elem));
		}

		public void Insert(int i_iIndex, DCondition i_Elem)
		{
			List.Insert(i_iIndex, i_Elem);
		}

		public void Remove(DCondition i_Elem)
		{
			List.Remove(i_Elem);
		}

		public DCondition this[int i_iIndex]
		{
			get {	return((DCondition)List[i_iIndex]);	}
			set {	List[i_iIndex] = value;	}
		}

		protected override void OnValidate(object i_oValue)
		{
			Type t1, t2;

			t1 = i_oValue.GetType();
			//t2 = Type.GetType("DialogModel.DCondition");
			t2 = typeof(DialogModel.DCondition);

			if(t1 != t2)
			{
				throw new ArgumentException("Value must be of type DialogModel.DCondition.", "i_oValue");
			}
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public class DPrompt
	{
		public ISMessaging.Audio.ISMPlayPrompts.PromptType	m_Type = ISMessaging.Audio.ISMPlayPrompts.PromptType.eUnknown;
		public object		m_oValue = null;
		public string		m_sText = "";		// Text of prompt.  (Can be TTSed if WAV file isn't found, for example.)
		public bool			m_bBargeinEnabled = true;

		public DPrompt()
		{
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public class DVariable
	{
		public enum eType
		{
			USCInt,
			USCString,
			USCObject,
		};

		private	eType			m_eType = eType.USCString;
		private string			m_sName = "";
		private string			m_sValue = "";
		private System.Int32	m_iValue = 0;
		private object			m_oValue = null;

		public DVariable()
		{
		}

		public DVariable(string i_sName, string i_sValue)
		{
			m_sName = i_sName;
			m_sValue = i_sValue;
		}

		public eType Type
		{
			get
			{
				return(m_eType);
			}
			set
			{
				m_eType = value;
			}
		}

		public string Name
		{
			get
			{
				return(m_sName);
			}
			set
			{
				m_sName = value;
			}
		}
		public string SValue
		{
			get
			{
				if( (m_eType != eType.USCString) && (m_eType != eType.USCInt) )
				{
					throw new InvalidCastException(String.Format("Value is not a string or an integer (name = '{0}').", Name));
				}
				else
				{
					return(m_sValue);
				}
			}
			set
			{
				m_sValue = value;
				m_eType = eType.USCString;
			}
		}
		public System.Int32 IValue
		{
			get
			{
				if(m_eType != eType.USCInt)
				{
					throw new InvalidCastException(String.Format("Value is not an integer (name = '{0}').", Name));
				}
				else
				{
					return(m_iValue);
				}
			}
			set
			{
				m_iValue = value;
				m_sValue = value.ToString();
				m_eType = eType.USCInt;
			}
		}
		public object OValue
		{
			get
			{
				if(m_eType != eType.USCObject)
				{
					throw new InvalidCastException(String.Format("Value is not an object (name = '{0}').", Name));
				}
				else
				{
					return(m_oValue);
				}
			}
			set
			{
				m_oValue = value;
				m_eType = eType.USCObject;
			}
		}
	} // DVariable

	public class DVariables : CollectionBase
	{
		public int Add(DVariable i_Elem)
		{
			return(List.Add(i_Elem));
		}

		public bool Contains(DVariable i_Elem)
		{
			return(List.Contains(i_Elem));
		}

		public int IndexOf(DVariable i_Elem)
		{
			return(List.IndexOf(i_Elem));
		}

		public void Insert(int i_iIndex, DVariable i_Elem)
		{
			List.Insert(i_iIndex, i_Elem);
		}

		public void Remove(DVariable i_Elem)
		{
			List.Remove(i_Elem);
		}

		public DVariable this[int i_iIndex]
		{
			get {	return((DVariable)List[i_iIndex]);	}
			set {	List[i_iIndex] = value;	}
		}

		public DVariable this[string i_sName]
		{
			get
			{
				DVariable	dvRet = null, dvTmp = null;
				int			ii = 0;

				for(ii = 0; ( (ii < Count) && (dvRet == null) ); ii++)
				{
					dvTmp = (DVariable)(List[ii]);
					if(dvTmp.Name == i_sName)
					{
						dvRet = dvTmp;
					}
				}

				return(dvRet);
			}
		}

		protected override void OnValidate(object i_oValue)
		{
			Type t1, t2;

			t1 = i_oValue.GetType();
			//t2 = Type.GetType("DialogModel.DVariable");
			t2 = typeof(DialogModel.DVariable);

			if(t1 != t2)
			{
				throw new ArgumentException("Value must be of type DialogModel.DVariable.", "i_oValue");
			}
		}
	} // DVariables

	/// <summary>
	/// 
	/// </summary>
	public class DScript
	{
		private string				m_sSrc = "";
		private string				m_sLanguage = "";
		private string				m_sType = "";
		private StringCollection	m_scCode = null;

		public DScript()
		{
			m_scCode = new StringCollection();
		}

		public string Src
		{
			get
			{
				return(m_sSrc);
			}
			set
			{
				m_sSrc = value;
			}
		}

		public string Language
		{
			get
			{
				return(m_sLanguage);
			}
			set
			{
				m_sLanguage = value;
			}
		}

		public string Type
		{
			get
			{
				return(m_sType);
			}
			set
			{
				m_sType = value;
			}
		}

		public StringCollection Code
		{
			get
			{
				return(m_scCode);
			}
			set
			{
				m_scCode = value;
			}
		}
	} // DScript

	public class DGrammar
	{
		public enum eLocation
		{
			Uri,
			Inline,
		};

		public enum eType
		{
			Unknown,
			ABNF,
			GrXML,
		};

		private eType		m_eType = eType.Unknown;
		private eLocation	m_eLocation = eLocation.Uri;
		private string		m_sGrammar = "";

		public eType Type
		{
			get
			{
				return(m_eType);
			}
			set
			{
				m_eType = value;
			}
		}

		public eLocation Location
		{
			get
			{
				return(m_eLocation);
			}
			set
			{
				m_eLocation = value;
			}
		}

		public string Grammar
		{
			get
			{
				return(m_sGrammar);
			}
			set
			{
				m_sGrammar = value;
			}
		}

	} // DGrammar


    public class DProperty
    {
        private string m_sName = "";
        private string m_sValue = "";

        public DProperty()
        {
        }

        public DProperty(string i_sName, string i_sValue)
        {
            m_sName = i_sName;
            m_sValue = i_sValue;
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
    } // DProperty

    public class DProperties : CollectionBase
    {
        public int Add(DProperty i_Elem)
        {
            return (List.Add(i_Elem));
        }

        public bool Contains(DProperty i_Elem)
        {
            return (List.Contains(i_Elem));
        }

        public int IndexOf(DProperty i_Elem)
        {
            return (List.IndexOf(i_Elem));
        }

        public void Insert(int i_iIndex, DProperty i_Elem)
        {
            List.Insert(i_iIndex, i_Elem);
        }

        public void Remove(DProperty i_Elem)
        {
            List.Remove(i_Elem);
        }

        public DProperty this[int i_iIndex]
        {
            get { return ((DProperty)List[i_iIndex]); }
            set { List[i_iIndex] = value; }
        }

        public DProperty this[string i_sName]
        {
            get
            {
                DProperty dpRet = null;

                foreach (Object entry in List)
                {
                    if (((DProperty)entry).Name == i_sName)
                    {
                        dpRet = (DProperty)entry;
                        break;
                    }
                }

                return dpRet;
            }

            set
            {
                foreach (Object entry in List)
                {
                    if (((DProperty)entry).Name == i_sName)
                    {
                        ((DProperty)entry).Value = value.Value;
                        break;
                    }
                }
            }
        }

        protected override void OnValidate(object i_oValue)
        {
            Type t1, t2;

            t1 = i_oValue.GetType();
            t2 = typeof(DialogModel.DProperty);

            if (t1 != t2)
            {
                throw new ArgumentException("Value must be of type DialogModel.DProperty.", "i_oValue");
            }
        }
    } // DProperties
}
