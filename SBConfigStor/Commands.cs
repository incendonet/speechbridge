// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;
using System.Collections;

namespace SBConfigStor
{
	public class Commands : CollectionBase
	{
		private const int m_ciNumberNeedsToBeDetermined = -1;
		private int m_iNumberOfSpeechCommands = m_ciNumberNeedsToBeDetermined;
		private int m_iNumberOfDtmfCommands = m_ciNumberNeedsToBeDetermined;
		private int m_iNumberOfTouchCommands = m_ciNumberNeedsToBeDetermined;
		private bool m_bDtmfCanBeSpoken = false;

		public enum eCommandType
		{
			Speech,
			Dtmf,
			Touch,
			IntroBlock,
			UnrecognizedDtmfHandler,
			UnrecognizedSpeechHandler
		}

		public enum eOperationType
		{
			DoNothing,
			Repeat,
			GotoMenu,
			Transfer,
			Prompt,
			CodeBlock,
            Hangup
		}

		public class Command
		{
			private const string m_csDefaultStringValue = "";
			private const eCommandType m_ceDefaultCommandType = eCommandType.Dtmf;
			private const eOperationType m_ceDefaultOperationType = eOperationType.DoNothing;

			private int m_iCommandId = -1;
			private string m_sCommandName = m_csDefaultStringValue;
			private eCommandType m_eCommandType = m_ceDefaultCommandType;
			private eOperationType m_eOperationType = m_ceDefaultOperationType;
			private string m_sCommandOption = m_csDefaultStringValue;
			private string m_sConfirmationText = m_csDefaultStringValue;
			private string m_sConfirmationWavUrl = m_csDefaultStringValue;
			private string m_sResponse = m_csDefaultStringValue;


			public int CommandId
			{
				get { return m_iCommandId; }
				set { m_iCommandId = value; }
			}

			public string CommandName
			{
				get { return m_sCommandName; }
				set { m_sCommandName = (null == value) ? m_csDefaultStringValue : value; }
			}

			public eCommandType CommandType
			{
				get { return m_eCommandType; }
				set { m_eCommandType = Enum.IsDefined(typeof(eCommandType), value) ? value : m_ceDefaultCommandType; }
			}

			public eOperationType OperationType
			{
				get { return m_eOperationType; }
				set { m_eOperationType = Enum.IsDefined(typeof(eOperationType), value) ? value : m_ceDefaultOperationType; }
			}

			public string CommandOption
			{
				get { return m_sCommandOption; }
				set { m_sCommandOption = (null == value) ? m_csDefaultStringValue : value.Trim(); }
			}

			public string ConfirmationText
			{
				get { return m_sConfirmationText; }
				set { m_sConfirmationText = (null == value) ? m_csDefaultStringValue : value.Trim(); }
			}

			public string ConfirmationWavUrl
			{
				get { return m_sConfirmationWavUrl; }
				set { m_sConfirmationWavUrl = (null == value) ? m_csDefaultStringValue : value.Trim(); }
			}

			public string Response
			{
				get { return m_sResponse; }
				set { m_sResponse = (null == value) ? m_csDefaultStringValue : value.Trim(); }
			}
		} // Command

		public int Add(Command i_Command)
		{
            InvalidateCommandCounts();
            int position = List.Add(i_Command);

			return position;
		}

		public bool Contains(Command i_Command)
		{
			return List.Contains(i_Command);
		}

		public int IndexOf(Command i_Command)
		{
			return List.IndexOf(i_Command);
		}

		public void Insert(int i_iIndex, Command i_Command)
		{
			List.Insert(i_iIndex, i_Command);
		}

		public void Remove(Command i_Command)
		{
            InvalidateCommandCounts();
            List.Remove(i_Command);
        }

		public Command this[int i_iIndex]
		{
			get { return (Command)List[i_iIndex]; }
			set	{ List[i_iIndex] = value; }
		}

		public bool HasSpeechCommands
		{
			get
			{
				if (m_ciNumberNeedsToBeDetermined == m_iNumberOfSpeechCommands)
				{
					m_iNumberOfSpeechCommands = 0;

					foreach (Command command in this)
					{
						if (Commands.eCommandType.Speech == command.CommandType)
						{
							++m_iNumberOfSpeechCommands;
						}
					}
				}

				int iNumberOfDtmfCommandsToSpeak = 0;
				if (DtmfCanBeSpoken)
				{
					foreach (Command command in this)
					{
						if ((Commands.eCommandType.Dtmf == command.CommandType) &&
							IsDtmfCommandSpeakable(command))
						{
							++iNumberOfDtmfCommandsToSpeak;
						}
					}
				}

				return ((m_iNumberOfSpeechCommands > 0) || (iNumberOfDtmfCommandsToSpeak > 0));
			}
		}

		public bool HasDtmfCommands
		{
			get
			{
				if (m_ciNumberNeedsToBeDetermined == m_iNumberOfDtmfCommands)
				{
					m_iNumberOfDtmfCommands = 0;

					foreach (Command command in this)
					{
						if (Commands.eCommandType.Dtmf== command.CommandType)
						{
							++m_iNumberOfDtmfCommands;
						}
					}
				}

				return (m_iNumberOfDtmfCommands > 0);
			}
		}

		public bool HasTouchCommands
		{
			get
			{
				if (m_ciNumberNeedsToBeDetermined == m_iNumberOfTouchCommands)
				{
					m_iNumberOfTouchCommands = 0;

					foreach (Command command in this)
					{
						if (Commands.eCommandType.Touch == command.CommandType)
						{
							++m_iNumberOfTouchCommands;
						}
					}
				}
				
				return (m_iNumberOfTouchCommands > 0);
			}
		}

		public bool DtmfCanBeSpoken
		{
			private get { return m_bDtmfCanBeSpoken; }
			set 
			{ 
				m_bDtmfCanBeSpoken = value;
				m_iNumberOfSpeechCommands = m_ciNumberNeedsToBeDetermined;					// Force number of speech commands to be recalculated the next time we ask since setting this property can impact that count.
			}
		}

        public bool IsDtmfCommandSpeakable(Command i_command)
        {
            return (DtmfCanBeSpoken && (Commands.eOperationType.DoNothing != i_command.OperationType));
        }

		public bool HasCommand(string i_sRequest)
		{
			bool bHasCommand = false;

			foreach (Command command in this)
			{
				if (command.CommandOption.ToLower() == i_sRequest.ToLower())
				{
					bHasCommand = true;
					break;
				}
			}

			return bHasCommand;
		}

		protected override void OnValidate(object i_oValue)
		{
			if (i_oValue.GetType() != typeof(SBConfigStor.Commands.Command))
			{
				throw new ArgumentException("Value must be of type SBConfigStor.Commands.Command", "i_Command");
			}
		}

        private void InvalidateCommandCounts()
        {
            m_iNumberOfSpeechCommands = m_ciNumberNeedsToBeDetermined;
            m_iNumberOfDtmfCommands = m_ciNumberNeedsToBeDetermined;
            m_iNumberOfTouchCommands = m_ciNumberNeedsToBeDetermined;
        }

	} // Commands
}
