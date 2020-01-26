﻿using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Assistant;
using ClassicAssist.Data.Autoloot;
using ClassicAssist.UI.ViewModels.Agents;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network;
using ClassicAssist.UO.Network.PacketFilter;
using ClassicAssist.UO.Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClassicAssist.Tests.Agents
{
    [TestClass]
    public class AutolootTests
    {
        [TestMethod]
        public void WillRehueMatchingItemProperties()
        {
            const string localPath = @"C:\Users\johns\Desktop\KvG Client 2.0";

            if ( !Directory.Exists( localPath ) )
            {
                Debug.WriteLine( "Not running test, requires Cliloc.enu" );
                return;
            }

            Cliloc.Initialize( localPath );

            Item corpse = new Item( 0x40000000 ) { ID = 0x2006 };

            Engine.Items.Add( corpse );

            IncomingPacketHandlers.Initialize();
            AutolootViewModel vm = new AutolootViewModel { Enabled = true };

            AutolootEntry lootEntry = new AutolootEntry
            {
                Rehue = true,
                Autoloot = false,
                Constraints = new ObservableCollection<AutolootConstraints>(),
                ID = 0x108a
            };

            AutolootConstraints constraint = vm.Constraints.FirstOrDefault( c => c.Name == "Faster Casting" );
            Debug.Assert( constraint != null, nameof( constraint ) + " != null" );
            constraint.Operator = AutolootOperator.GreaterThan;
            constraint.Value = 1;
            lootEntry.Constraints.Add( constraint );

            vm.Items.Add( lootEntry );

            Engine.PacketWaitEntries = new PacketWaitEntries();

            AutoResetEvent are = new AutoResetEvent( false );

            void OnPacketReceivedEvent( byte[] data, int length )
            {
                if ( data[0] == 0x25 )
                {
                    are.Set();
                }
                else
                {
                    Assert.Fail();
                }
            }

            Engine.InternalPacketReceivedEvent += OnPacketReceivedEvent;

            Engine.PacketWaitEntries.WaitEntryAddedEvent += entry =>
            {
                byte[] containerContentsPacket =
                {
                    0x3C, 0x00, 0x19, 0x00, 0x01, 0x43, 0x13, 0xFC, 0x5E, 0x10, 0x8A, 0x00, 0x00, 0x01, 0x00, 0x13,
                    0x00, 0x82, 0x00, 0x40, 0x00, 0x00, 0x00, 0x00, 0x00
                };

                PacketHandler handler = IncomingPacketHandlers.GetHandler( 0x3C );

                handler.OnReceive( new PacketReader( containerContentsPacket, containerContentsPacket.Length, false ) );
                entry.Packet = containerContentsPacket;
                entry.Lock.Set();

                byte[] propertiesPacket =
                {
                    0xD6, 0x00, 0x53, 0x00, 0x01, 0x43, 0x13, 0xFC, 0x5E, 0x00, 0x00, 0x01, 0x6E, 0x08, 0xB4, 0x00,
                    0x0F, 0xA0, 0xEA, 0x00, 0x00, 0x00, 0x10, 0x5E, 0x94, 0x00, 0x02, 0x31, 0x00, 0x00, 0x10, 0x2E,
                    0x64, 0x00, 0x14, 0x23, 0x00, 0x31, 0x00, 0x30, 0x00, 0x34, 0x00, 0x34, 0x00, 0x30, 0x00, 0x38,
                    0x00, 0x35, 0x00, 0x09, 0x00, 0x39, 0x00, 0x00, 0x10, 0x2E, 0x3C, 0x00, 0x02, 0x32, 0x00, 0x00,
                    0x10, 0x2E, 0x3D, 0x00, 0x02, 0x31, 0x00, 0x00, 0x10, 0x2E, 0x83, 0x00, 0x02, 0x34, 0x00, 0x00,
                    0x00, 0x00, 0x00
                };

                handler = IncomingPacketHandlers.GetHandler( 0xD6 );

                handler.OnReceive( new PacketReader( propertiesPacket, propertiesPacket.Length, false ) );
            };

            vm.OnCorpseContainerDisplayEvent( corpse.Serial );

            bool result = are.WaitOne( 5000 );

            Assert.IsTrue( result );

            Engine.Items.Clear();
            Engine.PacketWaitEntries = null;

            Engine.InternalPacketReceivedEvent -= OnPacketReceivedEvent;
        }

        [TestMethod]
        public void WillRehueMatchingObjectProperties()
        {
            Item corpse = new Item( 0x40000000 ) { ID = 0x2006 };

            Engine.Items.Add( corpse );

            IncomingPacketHandlers.Initialize();
            AutolootViewModel vm = new AutolootViewModel { Enabled = true };

            AutolootEntry lootEntry = new AutolootEntry
            {
                Rehue = true,
                Autoloot = false,
                Constraints = new ObservableCollection<AutolootConstraints>(),
                ID = 0x108a
            };

            AutolootConstraints constraint = vm.Constraints.FirstOrDefault( c => c.Name == "Hue" );
            Debug.Assert( constraint != null, nameof( constraint ) + " != null" );
            constraint.Operator = AutolootOperator.Equal;
            constraint.Value = 0;
            lootEntry.Constraints.Add( constraint );

            vm.Items.Add( lootEntry );

            Engine.PacketWaitEntries = new PacketWaitEntries();

            AutoResetEvent are = new AutoResetEvent( false );

            void OnPacketReceivedEvent( byte[] data, int length )
            {
                if ( data[0] == 0x25 )
                {
                    are.Set();
                }
                else
                {
                    Assert.Fail();
                }
            }

            Engine.InternalPacketReceivedEvent += OnPacketReceivedEvent;

            Engine.PacketWaitEntries.WaitEntryAddedEvent += entry =>
            {
                byte[] containerContentsPacket =
                {
                    0x3C, 0x00, 0x19, 0x00, 0x01, 0x43, 0x13, 0xFC, 0x5E, 0x10, 0x8A, 0x00, 0x00, 0x01, 0x00, 0x13,
                    0x00, 0x82, 0x00, 0x40, 0x00, 0x00, 0x00, 0x00, 0x00
                };

                PacketHandler handler = IncomingPacketHandlers.GetHandler( 0x3C );

                handler.OnReceive( new PacketReader( containerContentsPacket, containerContentsPacket.Length, false ) );
                entry.Packet = containerContentsPacket;
                entry.Lock.Set();
            };

            vm.OnCorpseContainerDisplayEvent( corpse.Serial );

            bool result = are.WaitOne( 5000 );

            Assert.IsTrue( result );

            Engine.Items.Clear();
            Engine.PacketWaitEntries = null;

            Engine.InternalPacketReceivedEvent -= OnPacketReceivedEvent;
        }
    }
}