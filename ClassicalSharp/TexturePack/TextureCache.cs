﻿// ClassicalSharp copyright 2014-2016 UnknownShadow200 | Licensed under MIT
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
#if ANDROID
using Android.Graphics;
#endif

namespace ClassicalSharp.TexturePack {
	
	/// <summary> Caches terrain atlases and texture packs to avoid making redundant downloads. </summary>
	public static class TextureCache {
		
		/// <summary> Gets the bitmap associated with the url from the cache, returning null if the bitmap
		/// for the url was not found in the cache or the bitmap in the cache was corrupted. </summary>
		public static Bitmap GetBitmapFromCache( string url ) {
			string path = MakePath( url );
			if( !File.Exists( path ) ) return null;
			
			try {
				return new Bitmap( path );
			} catch( ArgumentException ex ) {
				ErrorHandler.LogError( "Cache.GetBitmapFromCache", ex );
				return null;
			} catch( IOException ex ) {
				ErrorHandler.LogError( "Cache.GetBitmapFromCache", ex );
				return null;
			}
		}
		
		/// <summary> Gets the data associated with the url from the cache, returning null if the 
		/// data for the url was not found in the cache. </summary>
		public static byte[] GetDataFromCache( string url ) {
			string path = MakePath( url );
			if( !File.Exists( path ) ) return null;
			
			try {
				return File.ReadAllBytes( path );
			} catch( IOException ex ) {
				ErrorHandler.LogError( "Cache.GetDataFromCache", ex );
				return null;
			}
		}
		
		/// <summary> Gets the time the data associated with the url from the cache was last modified, 
		/// returning DateTime.MinValue if data for the url was not found in the cache. </summary>
		public static DateTime GetLastModifiedFromCache( string url ) {
			string path = MakePath( url );
			if( !File.Exists( path ) ) 
				return DateTime.MinValue;
			
			return File.GetLastWriteTimeUtc( path );
		}
		
		/// <summary> Gets whether the given url has a bitmap associated with it in the cache. </summary>
		public static bool IsInCache( string url ) {
			return File.Exists( MakePath( url ) );
		}
		
		/// <summary> Adds the url and the bitmap associated with it to the cache. </summary>
		public static void AddToCache( string url, Bitmap bmp ) {
			string path = MakePath( url );
			try {
				string basePath = Path.Combine( Program.AppDirectory, Folder );
				if( !Directory.Exists( basePath ) )
					Directory.CreateDirectory( basePath );
				
				using( FileStream fs = File.Create( path ) )
					Platform.WriteBmp( bmp, fs );
			} catch( IOException ex ) {
				ErrorHandler.LogError( "Cache.AddToCache", ex );
			}
		}
		
		/// <summary> Adds the url and the data associated with it to the cache. </summary>
		public static void AddToCache( string url, byte[] data ) {
			string path = MakePath( url );
			try {
				string basePath = Path.Combine( Program.AppDirectory, Folder );
				if( !Directory.Exists( basePath ) )
					Directory.CreateDirectory( basePath );
				
				File.WriteAllBytes( path, data );
			} catch( IOException ex ) {
				ErrorHandler.LogError( "Cache.AddToCache", ex );
			}
		}
		
		const string Folder = "texturecache";
		
		static string MakePath( string url ) {
			byte[] utf8 = Encoding.UTF8.GetBytes( url );
			uint crc32 = CRC32( utf8 );
			string basePath = Path.Combine( Program.AppDirectory, Folder );
			return Path.Combine( basePath, crc32.ToString() );
		}
		
		static uint CRC32( byte[] data ) {
			uint crc = 0xffffffffU;
			for( int i = 0; i < data.Length; i++ ) {
				crc ^= data[i];
				for( int j = 0; j < 8; j++ )
					crc = (crc >> 1) ^ (crc & 1) * 0xEDB88320;
			}
			return crc ^ 0xffffffffU;
		}
	}
}
