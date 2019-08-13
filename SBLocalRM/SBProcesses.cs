// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;
using System.Collections;
using System.Diagnostics;

namespace SBLocalRM
{
	/// <summary>
	/// 
	/// </summary>
	public class SBProcesses : CollectionBase
	{
		public int Add(SBProcess i_Elem)
		{
			return(List.Add(i_Elem));
		}

		public bool Contains(SBProcess i_Elem)
		{
			return(List.Contains(i_Elem));
		}

		public int IndexOf(SBProcess i_Elem)
		{
			return(List.IndexOf(i_Elem));
		}

		public void Insert(int i_iIndex, SBProcess i_Elem)
		{
			List.Insert(i_iIndex, i_Elem);
		}

		public void Remove(SBProcess i_Elem)
		{
			List.Remove(i_Elem);
		}

//		public new void RemoveAt(int i_iIndex)
//		{
//			List.RemoveAt(i_iIndex);
//		}

		public SBProcess this[int i_iIndex]
		{
			get {	return((SBProcess)List[i_iIndex]);	}
			set {	List[i_iIndex] = value;	}
		}

		protected override void OnValidate(object i_oValue)
		{
			Type t1, t2;

			t1 = i_oValue.GetType();
			//t2 = Type.GetType("SBLocalRM.SBProcess");
			t2 = typeof(SBProcess);

			if(t1 != t2)
			{
				throw new ArgumentException("Value must be of type SBLocalRM.SBProcess!", "i_oValue");
			}
		}
	} // SBProcesses

	public class SBProcess
	{
		private	Process		m_Proc = null;
		private bool		m_bRunning = false;
		private string		m_sName = "";
		private string		m_sCommand = "";

		public Process Proc
		{
			get { return(m_Proc); }
			set { m_Proc = value; }
		}

		public bool Running
		{
			get { return(m_bRunning); }
			set { m_bRunning = value; }
		}

		public string Name
		{
			get { return(m_sName); }
			set { m_sName = value; }
		}

		public string Command
		{
			get { return(m_sCommand); }
			set { m_sCommand = value; }
		}
	} // SBProcess
}
