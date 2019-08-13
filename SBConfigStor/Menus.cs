// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;
using System.Collections;

namespace SBConfigStor
{
	public class Menus : CollectionBase
	{
		public class Menu
		{
			private const string m_csDefaultStringValue = "";

			private int m_iMenuId = -1;
			private string m_sMenuName = m_csDefaultStringValue;
			private string m_sLanguageCode = m_csDefaultStringValue;
			private string m_sInclude = m_csDefaultStringValue;
			private string m_sGrammarUrl = m_csDefaultStringValue;
			private Variables m_Variables;
			private int m_iMinConfScore = 50;
			private int m_iHighConfScore = 80;
			private bool m_bEnabled = true;
            private bool m_bDtmfCanBeSpoken = false;
            private bool m_bConfirmationEnabled = false;

			public Menu()
			{
				m_Variables = new Variables();
			}

			public int MenuId
			{
				get { return m_iMenuId; }
				set { m_iMenuId = value; }
			}

			public string MenuName
			{
				get { return m_sMenuName; }
				set { m_sMenuName = (null == value) ? m_csDefaultStringValue : value; }
			}

			public string LanguageCode
			{
				get { return m_sLanguageCode; }
				set { m_sLanguageCode = value ?? m_csDefaultStringValue; }
			}

			public string Include
			{
				get { return m_sInclude; }
				set { m_sInclude = (null == value) ? m_csDefaultStringValue : value; }
			}

			public string GrammarUrl
			{
				get { return m_sGrammarUrl; }
				set { m_sGrammarUrl = (null == value) ? m_csDefaultStringValue : value; }
			}

			public Variables Variables
			{
				get { return m_Variables; }
				set { m_Variables = value; }
			}

			public int MinConfScore
			{
				get { return m_iMinConfScore; }
				set { m_iMinConfScore = value; }
			}

			public int HighConfScore
			{
				get { return m_iHighConfScore; }
				set { m_iHighConfScore = value; }
			}

			public bool Enabled
			{
				get { return m_bEnabled; }
				set { m_bEnabled = value; }
			}

			public bool DtmfCanBeSpoken
			{
				get { return m_bDtmfCanBeSpoken; }
				set { m_bDtmfCanBeSpoken = value; }
			}

            public bool ConfirmationEnabled
            {
                get { return m_bConfirmationEnabled; }
                set { m_bConfirmationEnabled = value; }
            }
		} // Menu

		public int Add(Menu i_Menu)
		{
			return List.Add(i_Menu);
		}

		public bool Contains(Menu i_Menu)
		{
			return List.Contains(i_Menu);
		}

		public int IndexOf(Menu i_Menu)
		{
			return List.IndexOf(i_Menu);
		}

		public void Insert(int i_iIndex, Menu i_Menu)
		{
			List.Insert(i_iIndex, i_Menu);
		}

		public void Remove(Menu i_Menu)
		{
			List.Remove(i_Menu);
		}

		public Menu this[int i_iIndex]
		{
			get { return (Menu)List[i_iIndex]; }
			set { List[i_iIndex] = value; }
		}

		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Note: We're assuming that the DB is enforcing unique names
		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public Menu GetMenuByName(string i_sMenuName)
		{
			int			ii = 0;
			Menu		oMenu = null;

			try
			{
				for(ii = 0; ( (ii < this.Count) && (oMenu == null) ); ii++)
				{
					if(((Menu)(List[ii])).MenuName == i_sMenuName)
					{
						oMenu = (Menu)(List[ii]);
					}
				}
			}
			catch
			{
				oMenu = null;
			}

			return(oMenu);
		}

		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public Menu GetMenuByID(int i_iMenuID)
		{
			int			ii = 0;
			Menu		oMenu = null;

			try
			{
				for(ii = 0; ( (ii < this.Count) && (oMenu == null) ); ii++)
				{
					if(((Menu)(List[ii])).MenuId == i_iMenuID)
					{
						oMenu = (Menu)(List[ii]);
					}
				}
			}
			catch
			{
				oMenu = null;
			}

			return(oMenu);
		}

		protected override void OnValidate(object i_oValue)
		{
			if (i_oValue.GetType() != typeof(SBConfigStor.Menus.Menu))
			{
				throw new ArgumentException("Value must be of type SBConfigStor.Menus.Menu", "i_Menu");
			}
		}
	} // Menus
}
