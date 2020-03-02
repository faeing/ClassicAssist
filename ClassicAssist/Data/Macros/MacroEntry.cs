﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using ClassicAssist.Data.Hotkeys;
using ClassicAssist.Resources;
using Microsoft.Scripting;
using UOScript;

namespace ClassicAssist.Data.Macros
{
    public class MacroEntry : HotkeyEntry, IComparable<MacroEntry>
    {
        private readonly Dispatcher _dispatcher;
        private Dictionary<string, int> _aliases = new Dictionary<string, int>();
        private bool _doNotAutoInterrupt;
        private bool _isAutostart;
        private bool _isBackground;
        private bool _isRunning;
        private bool _loop;
        private string _macro = string.Empty;
        private IMacroInvoker _macroInvoker;
        private MacroType _macroType;
        private string _name;

        public MacroEntry( MacroType type )
        {
            _dispatcher = Dispatcher.CurrentDispatcher;

            switch ( type )
            {
                case MacroType.Python:
                    _macroInvoker = new PythonInvoker();
                    break;
                case MacroType.Steam:
                    _macroInvoker = new SteamInvoker();
                    break;
                default:
                    throw new ArgumentOutOfRangeException( nameof( type ), type, null );
            }

            MacroType = type;
        }

        public Dictionary<string, int> Aliases
        {
            get => _aliases;
            set => SetProperty( ref _aliases, value );
        }

        public bool DoNotAutoInterrupt
        {
            get => _doNotAutoInterrupt;
            set => SetProperty( ref _doNotAutoInterrupt, value );
        }

        public bool IsAutostart
        {
            get => _isAutostart;
            set => SetProperty( ref _isAutostart, value );
        }

        public bool IsBackground
        {
            get => _isBackground;
            set => SetProperty( ref _isBackground, value );
        }

        public bool IsRunning
        {
            get => _isRunning;
            set => SetProperty( ref _isRunning, value );
        }

        public bool Loop
        {
            get => _loop;
            set => SetProperty( ref _loop, value );
        }

        public string Macro
        {
            get => _macro;
            set => SetProperty( ref _macro, value );
        }

        public IMacroInvoker MacroInvoker
        {
            get => _macroInvoker;
            set => SetProperty( ref _macroInvoker, value );
        }

        public MacroType MacroType
        {
            get => _macroType;
            set
            {
                if ( _macroType != value )
                {
                    switch ( value )
                    {
                        case MacroType.Python:
                            _macroInvoker = new PythonInvoker();
                            break;
                        case MacroType.Steam:
                            _macroInvoker = new SteamInvoker();
                            break;
                        default:
                            throw new ArgumentOutOfRangeException( nameof( value ), value, null );
                    }
                }

                SetProperty( ref _macroType, value );
            }
        }

        public override string Name
        {
            get => _name;
            set => SetName( _name, value );
        }

        public int CompareTo( MacroEntry other )
        {
            return string.Compare( Name, other.Name, StringComparison.OrdinalIgnoreCase );
        }

        private void OnStoppedEvent()
        {
            _macroInvoker.ExceptionEvent -= OnExceptionEvent;
            _macroInvoker.StoppedEvent -= OnStoppedEvent;

            _dispatcher.Invoke( () => IsRunning = false );

            if ( IsBackground )
            {
                UO.Commands.SystemMessage( string.Format( Strings.Background_macro___0___stopped___, Name ) );
            }
        }

        public override string ToString()
        {
            return Name;
        }

        private void SetName( string name, string value )
        {
            MacroManager manager = MacroManager.GetInstance();

            bool exists = manager.Items?.Any( m => m.Name == value && !ReferenceEquals( m, this ) ) ?? false;

            if ( exists && name == null )
            {
                SetName( null, $"{value}-" );
                return;
            }

            if ( exists )
            {
                MessageBox.Show( Strings.Macro_name_must_be_unique_, Strings.Error );
                return;
            }

            SetProperty( ref _name, value );
        }

        public void Execute()
        {
            if ( _macroInvoker.IsRunning )
            {
                _macroInvoker.Stop();
            }

            _dispatcher.Invoke( () => IsRunning = true );

            if ( IsBackground )
            {
                UO.Commands.SystemMessage( string.Format( Strings.Background_macro___0___started___, Name ) );
            }

            _macroInvoker.ExceptionEvent += OnExceptionEvent;
            _macroInvoker.StoppedEvent += OnStoppedEvent;

            _macroInvoker.Execute( this );
        }

        public void Stop()
        {
            if ( _macroInvoker.IsRunning )
            {
                _macroInvoker.Stop();
            }
        }

        private static void OnExceptionEvent( Exception exception )
        {
            UO.Commands.SystemMessage( string.Format( Strings.Macro_error___0_, exception.Message ) );

            if ( exception is SyntaxError se )
            {
                UO.Commands.SystemMessage( se.Node?.Lexeme );
            }

            if ( exception is RunTimeError re )
            {
                UO.Commands.SystemMessage( re.Node?.Lexeme );
            }

            if ( exception is SyntaxErrorException syntaxError )
            {
                UO.Commands.SystemMessage( $"{Strings.Line_Number}: {syntaxError.RawSpan.Start.Line}" );
            }
        }
    }
}