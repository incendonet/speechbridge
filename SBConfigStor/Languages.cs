// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;
using System.Collections;

namespace SBConfigStor
{
	public class Languages : CollectionBase
	{
		public class Language
		{
			private string m_sName;
			private string m_sCode;

			public Language()
			{
			}

			public Language(string i_sName, string i_sCode)
			{
				Name = i_sName;
				Code = i_sCode;
			}

			public string Name
			{
				get { return m_sName; }
				set { m_sName = value; }
			}

			public string Code
			{
				get { return m_sCode; }
				set { m_sCode = value; }
			}
		} // Language

		public int Add(Language i_Language)
		{
			return List.Add(i_Language);
		}

		public bool Contains(Language i_Language)
		{
			return List.Contains(i_Language);
		}

		public int IndexOf(Language i_Language)
		{
			return List.IndexOf(i_Language);
		}

		public void Insert(int i_iIndex, Language i_Language)
		{
			List.Insert(i_iIndex, i_Language);
		}

		public void Remove(Language i_Language)
		{
			List.Remove(i_Language);
		}

		public Language this[int i_iIndex]
		{
			get { return (Language)List[i_iIndex]; }
			set { List[i_iIndex] = value; }
		}

		public int Add(string i_sName, string i_sCode)
		{
			return List.Add(new Language(i_sName, i_sCode));
		}

		public bool Contains(string i_sName)
		{
			foreach (Language v in List)
			{
				if (i_sName == v.Name)
				{
					return true;
				}
			}

			return false;
		}

		public int IndexOf(string i_sName)
		{
			int index = -1;

			for (int i = 0; i < List.Count; ++i)
			{
				Language v = (Language)List[i];

				if (i_sName == v.Name)
				{
					index = i;
					break;
				}
			}

			return index;
		}

		public void Insert(int i_iIndex, string i_sName, string i_sCode)
		{
			List.Insert(i_iIndex, new Language(i_sName, i_sCode));
		}

		public void Remove(string i_sName)
		{
			for (int i = 0; i < List.Count; ++i)
			{
				Language v = (Language)List[i];

				if (i_sName == v.Name)
				{
					List.Remove(v);
					break;
				}
			}
		}

		protected override void OnValidate(object i_oValue)
		{
			if (i_oValue.GetType() != typeof(SBConfigStor.Languages.Language))
			{
				throw new ArgumentException("Value must be of type SBConfigStor.Languages.Language", "i_Language");
			}
		}
	} // Languages
}
