// Copyright (c) 2018 Incendonet Inc.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;
using System.Collections.Generic;
using System.Text;

using NUnit.Framework;

using SBConfigStor;

namespace SBConfigStorTests
{
    [TestFixture]
    public class CommandsTest
    {
        [Test]
        public void TestThatEmptyCommandsCollectionHasNoDtmfCommands()
        {
            Commands commands = new Commands();

            Assert.That(commands.HasDtmfCommands, Is.False);
        }

        [Test]
        public void TestThatEmptyCommandsCollectionHasNoSpeechCommands()
        {
            Commands commands = new Commands();

            Assert.That(commands.HasSpeechCommands, Is.False);
        }

        [Test]
        public void TestThatEmptyCommandsCollectionHasNoTouchCommands()
        {
            Commands commands = new Commands();

            Assert.That(commands.HasTouchCommands, Is.False);
        }

        [Test]
        public void TestThatHasDtmfCommandsReturnsTrueIfCommandsCollectionContainsADtmfCommand()
        {
            Commands commands = new Commands();

            commands.Add(GetSpeakableDtmfCommand());

            Assert.That(commands.HasDtmfCommands, Is.True);
        }

        [Test]
        public void TestThatHasSpeechCommandsReturnsTrueIfCommandsCollectionContainsASpeechCommand()
        {
            Commands commands = new Commands();

            commands.Add(GetSpeechCommand());

            Assert.That(commands.HasSpeechCommands, Is.True);
        }

        [Test]
        public void TestThatHasTouchCommandsReturnsTrueIfCommandsCollectionContainsATouchCommand()
        {
            Commands commands = new Commands();

            commands.Add(GetTouchCommand());

            Assert.That(commands.HasTouchCommands, Is.True);
        }

        [Test]
        public void TestThatHasSpeechCommandsReturnsTrueIfCommandsCollectionContainsADtmfCommandThatCanBeSpoken()
        {
            Commands commands = new Commands();
            commands.Add(GetSpeakableDtmfCommand());

            commands.DtmfCanBeSpoken = true;

            Assert.That(commands.HasSpeechCommands, Is.True);
        }

        [Test]
        public void TestThatHasDtmfCommandsReturnsTrueAfterCommandsCollectionIsUpdated()
        {
            Commands commands = new Commands();

            Assert.That(commands.HasDtmfCommands, Is.False);

            commands.Add(GetSpeakableDtmfCommand());

            Assert.That(commands.HasDtmfCommands, Is.True);
        }

        [Test]
        public void TestThatHasSpeechCommandsReturnsTrueAfterCommandsCollectionIsUpdated()
        {
            Commands commands = new Commands();

            Assert.That(commands.HasSpeechCommands, Is.False);

            commands.Add(GetSpeechCommand());

            Assert.That(commands.HasSpeechCommands, Is.True);
        }

        [Test]
        public void TestThatHasTouchCommandsReturnsTrueAfterCommandsCollectionIsUpdated()
        {
            Commands commands = new Commands();

            Assert.That(commands.HasTouchCommands, Is.False);

            commands.Add(GetTouchCommand());

            Assert.That(commands.HasTouchCommands, Is.True);
        }


        [Test]
        public void TestThatHasSpeechCommandsReturnsTrueAfterDtmfCanBeSpokenIsEnabledOnCommandsCollection()
        {
            Commands commands = new Commands();

            commands.Add(GetSpeakableDtmfCommand());

            Assert.That(commands.HasSpeechCommands, Is.False);

            commands.DtmfCanBeSpoken = true;

            Assert.That(commands.HasSpeechCommands, Is.True);
        }

        [Test]
        public void TestThatHasSpeechCommandsReturnsFalseAfterDtmfCanBeSpokenIsDisabledOnCommandsCollection()
        {
            Commands commands = new Commands();

            commands.Add(GetSpeakableDtmfCommand());
            commands.DtmfCanBeSpoken = true;

            Assert.That(commands.HasSpeechCommands, Is.True);

            commands.DtmfCanBeSpoken = false;

            Assert.That(commands.HasSpeechCommands, Is.False);
        }

        [Test]
        public void TestThatSpeakableDtmfCommandCannotBeSpokenIfDtmfCanBeSpokenIsDisabledOnCommandsCollection()
        {
            Commands commands = new Commands();

            commands.DtmfCanBeSpoken = false;

            Assert.That(commands.IsDtmfCommandSpeakable(GetSpeakableDtmfCommand()), Is.False);
        }

        [Test]
        public void TestThatSpeakableDtmfCommandCanBeSpokenIfDtmfCanBeSpokenIsEnabledOnCommandsCollection()
        {
            Commands commands = new Commands();

            commands.DtmfCanBeSpoken = true;

            Assert.That(commands.IsDtmfCommandSpeakable(GetSpeakableDtmfCommand()), Is.True);
        }

        [Test]
        public void TestThatNonspeakableDtmfCommandCannotBeSpokenIfDtmfCanBeSpokenIsEnabledOnCommandsCollection()
        {
            Commands commands = new Commands();

            commands.DtmfCanBeSpoken = true;

            Assert.That(commands.IsDtmfCommandSpeakable(GetNonspeakableDtmfCommand()), Is.False);
        }

        
        // Helper methods

        private Commands.Command GetSpeakableDtmfCommand()
        {
            Commands.Command command = new Commands.Command();
            command.CommandType = Commands.eCommandType.Dtmf;
            command.OperationType = Commands.eOperationType.Prompt;

            return command;
        }

        private Commands.Command GetNonspeakableDtmfCommand()
        {
            Commands.Command command = new Commands.Command();
            command.CommandType = Commands.eCommandType.Dtmf;
            command.OperationType = Commands.eOperationType.DoNothing;

            return command;
        }
        
        private Commands.Command GetSpeechCommand()
        {
            Commands.Command command = new Commands.Command();
            command.CommandType = Commands.eCommandType.Speech;

            return command;
        }

        private Commands.Command GetTouchCommand()
        {
            Commands.Command command = new Commands.Command();
            command.CommandType = Commands.eCommandType.Touch;

            return command;
        }
    }
}
