﻿using ClassicAssist.Data.Macros.Steam;
using UOSteam;

namespace ClassicAssist.Data.Macros
{
    public class SteamEngineInvoker : IMacroInvoker
    {
        private static SteamEngineInvoker _instance;
        private static readonly object _lock = new object();

        public SteamEngineInvoker()
        {
            Aliases.Register();
            Steam.Commands.Register();
        }

        public event PythonInvoker.dMacroException ExceptionEvent;

        public void Stop()
        {
        }

        public void Execute( MacroEntry macro )
        {
            ASTNode ast = Lexer.Lex( macro.Macro.Split( '\n' ) );

            Script script = new Script( ast );

            try
            {
                Interpreter.StartScript( script );

                while ( Interpreter.ExecuteScripts() )
                {
                }
            }
            catch ( SyntaxError se )
            {
                UO.Commands.SystemMessage( se.Message );
            }
            catch ( RunTimeError re )
            {
                UO.Commands.SystemMessage( re.Message );
            }
        }

        public static SteamEngineInvoker GetInstance()
        {
            // ReSharper disable once InvertIf
            if ( _instance == null )
            {
                lock ( _lock )
                {
                    if ( _instance != null )
                    {
                        return _instance;
                    }

                    _instance = new SteamEngineInvoker();
                    return _instance;
                }
            }

            return _instance;
        }
    }
}