// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;
using System.Collections;

namespace SBConfigStor
{
	public class Variables : CollectionBase
	{
		public class Variable
		{
			private string m_sName;
			private string m_sValue;

			public Variable()
			{
			}

			public Variable(string i_sName, string i_sValue)
			{
				Name = i_sName;
				Value = i_sValue;
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
		} // Variable

		public int Add(Variable i_Variable)
		{
			return List.Add(i_Variable);
		}

		public bool Contains(Variable i_Variable)
		{
			return List.Contains(i_Variable);
		}

		public int IndexOf(Variable i_Variable)
		{
			return List.IndexOf(i_Variable);
		}

		public void Insert(int i_iIndex, Variable i_Variable)
		{
			List.Insert(i_iIndex, i_Variable);
		}

		public void Remove(Variable i_Variable)
		{
			List.Remove(i_Variable);
		}

		public Variable this[int i_iIndex]
		{
			get { return (Variable)List[i_iIndex]; }
			set { List[i_iIndex] = value; }
		}

		public int Add(string i_sName, string i_sValue)
		{
			return List.Add(new Variable(i_sName, i_sValue));
		}

		public bool Contains(string i_sName)
		{
			foreach (Variable v in List)
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
				Variable v = (Variable)List[i];

				if (i_sName == v.Name)
				{
					index = i;
					break;
				}
			}

			return index;
		}

		public void Insert(int i_iIndex, string i_sName, string i_sValue)
		{
			List.Insert(i_iIndex, new Variable(i_sName, i_sValue));
		}

		public void Remove(string i_sName)
		{
			for (int i = 0; i < List.Count; ++i)
			{
				Variable v = (Variable)List[i];

				if (i_sName == v.Name)
				{
					List.Remove(v);
					break;
				}
			}
		}

		protected override void OnValidate(object i_oValue)
		{
			if (i_oValue.GetType() != typeof(SBConfigStor.Variables.Variable))
			{
				throw new ArgumentException("Value must be of type SBConfigStor.Variables.Variable", "i_Variable");
			}
		}
	} // Variables
}
