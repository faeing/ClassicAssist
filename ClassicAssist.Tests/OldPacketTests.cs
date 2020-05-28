﻿#region License

// Copyright (C) 2020 Reetus
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System;
using System.Linq;
using System.Threading;
using Assistant;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network;
using ClassicAssist.UO.Network.Packets;
using ClassicAssist.UO.Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClassicAssist.Tests
{
    [TestClass]
    public class OldPacketTests
    {
        [TestInitialize]
        public void Initialize()
        {
            Engine.ClientVersion = new Version( 5, 0, 9, 1 );
        }

        [TestMethod]
        public void WillSendProperDropRequest()
        {
            AutoResetEvent are = new AutoResetEvent( false );

            void OnInternalPacketSentEvent( byte[] data, int length )
            {
                if ( data[0] != 0x08 )
                {
                    return;
                }

                int serial = ( data[1] << 24 ) | ( data[2] << 16 ) | ( data[3] << 8 ) | data[4];
                int x = ( data[5] << 8 ) | data[6];
                int y = ( data[7] << 8 ) | data[8];
                int z = data[9];
                int containerSerial = ( data[10] << 24 ) | ( data[11] << 16 ) | ( data[12] << 8 ) | data[13];

                Assert.AreEqual( 0x11223344, serial );
                Assert.AreEqual( 1, x );
                Assert.AreEqual( 2, y );
                Assert.AreEqual( 3, z );
                Assert.AreEqual( 0x55667788, containerSerial );

                are.Set();
            }

            Engine.InternalPacketSentEvent += OnInternalPacketSentEvent;

            Engine.SendPacketToServer( new DropItem( 0x11223344, 0x55667788, 1, 2, 3 ) );
            are.WaitOne();
        }

        [TestMethod]
        public void WillParseOldContainerContents()
        {
            byte[] packet =
            {
                0x3C, 0x00, 0x2B, 0x00, 0x02, 0x46, 0x13, 0xFF, 0x71, 0x0E, 0x76, 0x00, 0x00, 0x01, 0x00, 0x2C,
                0x00, 0x7F, 0x46, 0x13, 0xFF, 0x6D, 0x00, 0x00, 0x46, 0x58, 0x64, 0xFB, 0x0E, 0xED, 0x00, 0x03,
                0xE8, 0x00, 0x4C, 0x00, 0x48, 0x46, 0x13, 0xFF, 0x6D, 0x00, 0x00
            };

            IncomingPacketHandlers.Initialize();
            Engine.Items = new ItemCollection( 0 );

            PacketHandler handler = IncomingPacketHandlers.GetHandler( 0x3C );
            handler?.OnReceive( new PacketReader( packet, packet.Length, false ) );

            Assert.AreEqual( 1, Engine.Items.GetItemCount() );
            Assert.IsTrue( Engine.Items.Any( i => i.Serial == 0x4613ff6d ) );
            Item container = Engine.Items.GetItem( 0x4613ff6d );
            Assert.IsNotNull( container );
            Assert.AreEqual( 2, container.Container.GetItemCount() );
        }

        [TestCleanup]
        public void Cleanup()
        {
            Engine.ClientVersion = new Version( 7, 0, 45, 1 );
        }
    }
}