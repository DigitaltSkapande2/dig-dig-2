using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

using UnityEngine;

namespace DigDig2.Debugging
{
	public enum LogSeverity
	{
		Debug,
		Info,
		Warning,
		Error
	}
	
	public static class BetterDebug
	{
		public static void Log( object message, LogSeverity severity = LogSeverity.Debug )
		{
			#if UNITY_EDITOR

			StackFrame lastStackFrame = new StackTrace( ).GetFrame( 1 );
			Type reflectedType = lastStackFrame.GetMethod( ).ReflectedType;
			if (reflectedType == null) { WriteLog(lastStackFrame.GetFileName(  ), message, severity); return; }
			
			WriteLog( reflectedType.Name, message, severity );

			#endif
		}

		private static void WriteLog( string caller, object message, LogSeverity severity ) {
			switch ( severity )
			{
				case LogSeverity.Debug:
					if ( Application.isEditor ) UnityEngine.Debug.Log( $"[{caller}]: {message}" );
					break;
				case LogSeverity.Info: UnityEngine.Debug.Log( $"[{caller}]: {message}" ); break;
				case LogSeverity.Warning: UnityEngine.Debug.LogWarning( $"[{caller}]: {message}" ); break;
				case LogSeverity.Error: UnityEngine.Debug.LogError( $"[{caller}]: {message}" ); break;
				default: throw new ArgumentOutOfRangeException( nameof( severity ), severity, null );
			}
		}
	}
}