using System;
using System.Collections.Generic;
using System.Linq;

using Cysharp.Threading.Tasks;

using UnityEngine;

namespace DigDig2.Debugging.Console
{
	public class CommandSystem
	{
		private readonly IEnumerable<ConsoleCommand> commands;

		public CommandSystem( IEnumerable<ConsoleCommand> commands ) { this.commands = commands; }

		internal List<string> GetSuggestions( CommandDescriptor inputCommand )
		{
			if ( inputCommand.HasArguments && inputCommand.args[ ^1 ] != null ) // if the command word is provided
			{
				Debug.Log( $"CommandSystem: Getting suggestions for command '{inputCommand.commandWord}' with arguments: '{string.Join( ", ", inputCommand.args )}'" );
				List<string> commandSuggestions = GetCommandSuggestions( inputCommand );
				if ( commandSuggestions == null || commandSuggestions.Count == 0 ) return null;

				return commandSuggestions
						.Where( suggestion => suggestion.StartsWith( inputCommand.args[ ^1 ], StringComparison.OrdinalIgnoreCase ) )
						.OrderBy( suggestion => LevenshteinDistance( suggestion, inputCommand.args[ 0 ] ) )
						.ToList( )
					;
			}

			Debug.Log( $"Commands: {string.Join( ", ", commands.Select( c => c.CommandWord ) )}" );

			// Return list of all commands
			return commands
					.Where( command => command.CommandWord.StartsWith( inputCommand.commandWord, StringComparison.OrdinalIgnoreCase ) )
					.OrderBy( command => LevenshteinDistance( command.CommandWord, inputCommand.commandWord ) )
					.Select( command => command.CommandWord )
					.ToList( )
				;
		}

		private List<string> GetCommandSuggestions( CommandDescriptor inputCommand )
		{
			ConsoleCommand command = commands.FirstOrDefault( c => c.CommandWord.Equals( inputCommand.commandWord, StringComparison.OrdinalIgnoreCase ) );

			return command ? command.GetSuggestions( inputCommand.args ) : null;
		}

		public async UniTask<CommandResultContext> ProcessCommand( CommandDescriptor inputCommand )
		{
			ConsoleCommand command = commands.FirstOrDefault( c => c.CommandWord.Equals( inputCommand.commandWord, StringComparison.OrdinalIgnoreCase ) );
			if ( command != null ) return await command.Process( inputCommand.args );

			return new( CommandResultContext.Type.Error, $"Command '{inputCommand.commandWord}' not found." );
		}

		private List<string> FuzzySort( IEnumerable<string> items, string input = "" )
		{
			return items
				.OrderBy( item =>
					{
						if ( item.Equals( input, StringComparison.OrdinalIgnoreCase ) ) return 0; // exact match
						if ( item.StartsWith( input, StringComparison.OrdinalIgnoreCase ) ) return 1; // prefix match
						if ( item.IndexOf( input, StringComparison.OrdinalIgnoreCase ) >= 0 ) return 2; // substring match

						return 3; // fallback to distance
					}
				)
				.ThenBy( item => LevenshteinDistance( item, input ) )
				.ThenBy( item => item, StringComparer.OrdinalIgnoreCase )
				.ToList( );
		}

		private static int LevenshteinDistance( string source, string target )
		{
			if ( source == null ) throw new ArgumentNullException( nameof( source ) );
			if ( target == null ) throw new ArgumentNullException( nameof( target ) );

			int sourceLength = source.Length;
			int targetLength = target.Length;

			if ( sourceLength == 0 ) return targetLength;
			if ( targetLength == 0 ) return sourceLength;

			int[ , ] distance = new int[ sourceLength + 1, targetLength + 1 ];

			for ( int i = 0; i <= sourceLength; distance[ i, 0 ] = i++ ) { }

			for ( int j = 0; j <= targetLength; distance[ 0, j ] = j++ ) { }

			for ( int i = 1; i <= sourceLength; i++ )
			{
				for ( int j = 1; j <= targetLength; j++ )
				{
					int cost = target[ j - 1 ] == source[ i - 1 ] ? 0 : 1;
					distance[ i, j ] = Math.Min(
						Math.Min( distance[ i - 1, j ] + 1, distance[ i, j - 1 ] + 1 ),
						distance[ i - 1, j - 1 ] + cost
					);
				}
			}

			return distance[ sourceLength, targetLength ];
		}
	}

	public readonly struct CommandDescriptor
	{
		public readonly string commandWord;
		public readonly string[ ] args;

		public bool HasArguments
		{
			get => args != null && args.Length > 0;
		}

		public bool IsValid
		{
			get => !string.IsNullOrWhiteSpace( commandWord );
		}

		public CommandDescriptor( string commandString )
		{
			if ( string.IsNullOrWhiteSpace( commandString ) )
			{
				commandWord = string.Empty;
				args = null;
				Debug.LogWarning( "CommandDescriptor: Trying to create CommandDescriptor of null or empty commandString." );
				return;
			}

			string[ ] parts = commandString.Split(
				new[ ]
				{
					' '
				},
				StringSplitOptions.None
			);

			// UnityEngine.Debug.Log(parts.Length + " parts found in commandString: " + commandString);
			commandWord = parts[ 0 ];
			args = parts.Skip( 1 ).ToArray( );
		}

		public string[ ] GetFullCommand( )
		{
			if ( args == null || args.Length == 0 )
			{
				return new[ ]
				{
					commandWord
				};
			}

			string[ ] fullCommand = new string[ args.Length + 1 ];
			fullCommand[ 0 ] = commandWord;
			Array.Copy(
				args,
				0,
				fullCommand,
				1,
				args.Length
			);

			return fullCommand;
		}

		public override string ToString( )
		{
			if ( args == null || args.Length == 0 ) return commandWord;

			return $"{commandWord} {string.Join( " ", args )}";
		}

		public static implicit operator CommandDescriptor( string commandString ) => new( commandString );

		public static implicit operator string( CommandDescriptor commandDescriptor ) => commandDescriptor.ToString( );
	}

	public struct CommandResultContext
	{
		public Type type;
		public enum Type { Success, Error, Warning }

		public string message;

		public CommandResultContext( Type type, string message )
		{
			this.type = type;
			this.message = message;
		}

		public CommandResultContext( Type type )
		{
			this.type = Type.Success;
			message = string.Empty;
		}

		public static CommandResultContext Success
		{
			get => new( Type.Success );
		}

		public static CommandResultContext InvalidArguments
		{
			get => new( Type.Error, "Invalid arguments provided." );
		}
	}
}
