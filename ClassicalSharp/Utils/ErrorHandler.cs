﻿// ClassicalSharp copyright 2014-2016 UnknownShadow200 | Licensed under MIT
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace ClassicalSharp {
	
	/// <summary> Displays a message box when an unhandled exception occurs
	/// and also logs it to a specified log file. </summary>
	public static class ErrorHandler {

		static string logFile = "crash.log";
		static string fileName = "crash.log";
		
		/// <summary> Adds a handler for when a unhandled exception occurs, unless 
		/// a debugger is attached to the process in which case this does nothing. </summary>
		public static void InstallHandler( string logFile ) {
			ErrorHandler.logFile = logFile;
			fileName = Path.GetFileName( logFile );
			if( !Debugger.IsAttached )
				AppDomain.CurrentDomain.UnhandledException += UnhandledException;
		}
		
		/// <summary> Additional text that should be logged to the log file 
		/// when an unhandled exception occurs. </summary>
		public static string[] AdditionalInfo;

		static void UnhandledException( object sender, UnhandledExceptionEventArgs e ) {
			// So we don't get the normal unhelpful crash dialog on Windows.
			Exception ex = (Exception)e.ExceptionObject;
			string error = ex.GetType().FullName + ": " + ex.Message + Environment.NewLine + ex.StackTrace;
			bool wroteToCrashLog = true;
			try {
				using( StreamWriter writer = new StreamWriter( logFile, true ) ) {
					writer.WriteLine( "=== crash occurred ===" );
					writer.WriteLine( "Time: " + DateTime.Now.ToString() );
					writer.WriteLine( error );
					writer.WriteLine();
					
					if( AdditionalInfo != null ) {
						foreach( string l in AdditionalInfo )
							writer.WriteLine( l );
						writer.WriteLine();
					}
				}
			} catch( Exception ) {
				wroteToCrashLog = false;
			}
			
			string message = wroteToCrashLog ?
				"The cause of the error has been logged to \"" + fileName + "\". Please consider reporting the error to " +
				"github.com/UnknownShadow200/ClassicalSharp/issues so we can fix it." :
				
				"Failed to log the cause of the the error. Please consider reporting the circumstances " +
				"of the error to github.com/UnknownShadow200/ClassicalSharp/issues so we can fix it." +
				Environment.NewLine + Environment.NewLine + error;
			
			MessageBox.Show( "An error occured and ClassicalSharp was forced to close." + Environment.NewLine +
			                Environment.NewLine + message, "We're sorry" );
			Environment.Exit( 1 );
		}
		
		/// <summary> Logs a handled exception that occured at the specified location to the log file. </summary>
		public static bool LogError( string location, Exception ex ) {
			string error = ex.GetType().FullName + ": " + ex.Message 
				+ Environment.NewLine + ex.StackTrace;
			return LogError( location, error );
		}
		
		/// <summary> Logs an error that occured at the specified location to the log file. </summary>
		public static bool LogError( string location, string text ) {
			try {
				using( StreamWriter writer = new StreamWriter( logFile, true ) ) {
					writer.WriteLine( "=== handled error ===" );
					writer.WriteLine( "Occured when: " + location );
					writer.WriteLine( text );
					writer.WriteLine();
				}
			} catch( Exception ) {
				return false;
			}
			return true;
		}
	}
}