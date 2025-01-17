﻿// ClassicalSharp copyright 2014-2016 UnknownShadow200 | Licensed under MIT
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace Launcher {

	/// <summary> Represents a task that performs a series of GET or POST requests asynchronously. </summary>
	public abstract class IWebTask {
		
		public virtual void ResetSession() {
			Username = null;
			cookies = new CookieContainer();
		}
		
		/// <summary> Whether this web task is still performing GET or POST requests asynchronously. </summary>
		public bool Working;
		
		/// <summary> Whether this web task has finished. </summary>
		public bool Done;
		
		/// <summary> Handled exception that was generated by the last GET or POST request. </summary>
		public WebException Exception;
		
		/// <summary> Current status of this web task (e.g. downloading page X) </summary>
		public string Status;
		
		/// <summary> Username used when performing GET or POST requests, can be left null. </summary>
		public string Username;
		
		protected void Finish( bool success, WebException ex, string status ) {
			if( !success ) 
				Username = null;
			Working = false;
			Done = true;
			
			Exception = ex;
			Status = status;
		}
		
		protected CookieContainer cookies = new CookieContainer();
		
		protected HttpWebResponse MakeRequest( string uri, string referer, string data ) {
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create( uri );
			request.UserAgent = Program.AppName;
			request.ReadWriteTimeout = 90 * 1000;
			request.Timeout = 90 * 1000;
			request.Referer = referer;
			request.KeepAlive = true;
			request.CookieContainer = cookies;
			
			// On my machine, these reduce minecraft server list download time from 40 seconds to 4.
			request.Proxy = null;
			request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
			if( data != null ) {
				request.Method = "POST";
				request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8;";
				byte[] encodedData = Encoding.UTF8.GetBytes( data );
				request.ContentLength = encodedData.Length;
				using( Stream stream = request.GetRequestStream() ) {
					stream.Write( encodedData, 0, encodedData.Length );
				}
			}
			return (HttpWebResponse)request.GetResponse();
		}
		
		protected IEnumerable<string> GetHtml( string uri, string referer ) {
			HttpWebResponse response = MakeRequest( uri, referer, null );
			return GetResponseLines( response );
		}

		protected IEnumerable<string> PostHtml( string uri, string referer, string data ) {
			HttpWebResponse response = MakeRequest( uri, referer, data );
			return GetResponseLines( response );
		}
		
		protected string GetHtmlAll( string uri, string referer ) {
			HttpWebResponse response = MakeRequest( uri, referer, null );
			return GetResponseAll( response );
		}
		
		protected string PostHtmlAll( string uri, string referer, string data ) {
			HttpWebResponse response = MakeRequest( uri, referer, data );
			return GetResponseAll( response );
		}
		
		protected IEnumerable<string> GetResponseLines( HttpWebResponse response ) {
			using( Stream stream = response.GetResponseStream() ) {
				using( StreamReader reader = new StreamReader( stream ) ) {
					string line;
					while( (line = reader.ReadLine()) != null ) {
						yield return line;
					}
				}
			}
		}
		
		protected string GetResponseAll( HttpWebResponse response ) {
			using( Stream stream = response.GetResponseStream() ) {
				using( StreamReader reader = new StreamReader( stream ) ) {
					return reader.ReadToEnd();
				}
			}
		}
		
		protected static void Log( string text ) {
			Console.WriteLine( text );
		}
	}
}
