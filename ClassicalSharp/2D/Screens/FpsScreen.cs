﻿// ClassicalSharp copyright 2014-2016 UnknownShadow200 | Licensed under MIT
using System;
using System.Drawing;
using ClassicalSharp.Entities;
using ClassicalSharp.GraphicsAPI;

namespace ClassicalSharp.Gui {
	
	public class FpsScreen : Screen {
		
		Font font, posFont;
		StringBuffer text;
		
		public FpsScreen( Game game ) : base( game ) {
			text = new StringBuffer( 128 );
		}

		TextWidget fpsText, hackStates;
		Texture posTex;
		public override void Render( double delta ) {
			UpdateFPS( delta );
			if( game.HideGui || !game.ShowFPS ) return;
			
			api.Texturing = true;
			fpsText.Render( delta );
			if( !game.ClassicMode && game.activeScreen == null ) {
				UpdateHackState( false );
				DrawPosition();
				hackStates.Render( delta );
			}
			api.Texturing = false;
		}
		
		double accumulator, maxDelta;
		int fpsCount, totalSeconds;
		int oldMinutes = -1;
		
		void UpdateFPS( double delta ) {
			fpsCount++;
			maxDelta = Math.Max( maxDelta, delta );
			accumulator += delta;
			if( accumulator < 1 ) return;
			
			int index = 0;
			totalSeconds++;
			int FPS = (int)(fpsCount / accumulator);
			if( game.ClassicMode ) {
				text.Clear()
					.AppendNum( ref index, FPS ).Append( ref index, " fps, " )
					.AppendNum( ref index, game.ChunkUpdates ).Append( ref index, " chunk updates" );
			} else {
				text.Clear()
					.AppendNum( ref index, FPS ).Append( ref index, " fps (" )
					.AppendNum( ref index, (int)(1f / maxDelta) ).Append( ref index, " min), " )
					.AppendNum( ref index, game.ChunkUpdates ).Append( ref index, " chunks/s, " )
					.AppendNum( ref index, game.Vertices ).Append( ref index, " vertices" );
			}
			
			CheckClock();
			string textString = text.GetString();
			fpsText.SetText( textString );
			maxDelta = 0;
			accumulator = 0;
			fpsCount = 0;
			game.ChunkUpdates = 0;
		}
		
		void CheckClock() {
			int minutes = totalSeconds / 60;
			if( !game.ShowClock || minutes == oldMinutes ) return;
			
			oldMinutes = minutes;
			TimeSpan span = TimeSpan.FromMinutes( minutes );
			string format = null;
			
			if( span.TotalDays > 1 ) {
				format = "&eBeen playing for {2} day" + Q( span.Days ) + ", {1} hour" +
					Q( span.Hours ) + ", {0} min" + Q( span.Minutes );
			} else if( span.TotalHours > 1 ) {
				format = "&eBeen playing for {1} hour" + Q( span.Hours ) + ", {0} min" +
					Q( span.Minutes );
			} else {
				format = "&eBeen playing for {0} min" + Q( span.Minutes );
			}
			string spanText = String.Format( format, span.Minutes, span.Hours, span.Days );
			game.Chat.Add( spanText, MessageType.ClientClock );
		}
		
		string Q( int value ) { return value == 1 ? "" : "s"; }
		
		public override void Init() {
			font = new Font( game.FontName, 14 );
			posFont = new Font( game.FontName, 14, FontStyle.Italic );
			game.Events.ChatFontChanged += ChatFontChanged;
			
			fpsText = new ChatTextWidget( game, font );
			fpsText.XOffset = 2;
			fpsText.YOffset = 2;
			fpsText.ReducePadding = true;
			fpsText.Init();
			
			string msg = text.Length > 0 ? text.GetString() :
				"FPS: no data yet";
			fpsText.SetText( msg );
			MakePosTextWidget();
			
			hackStates = new ChatTextWidget( game, posFont );
			hackStates.XOffset = 2;
			hackStates.YOffset = fpsText.Height + posTex.Height + 2;
			hackStates.ReducePadding = true;
			hackStates.Init();
			UpdateHackState( true );
		}
		
		public override void Dispose() {
			font.Dispose();
			posFont.Dispose();
			fpsText.Dispose();
			api.DeleteTexture( ref posTex );
			game.Events.ChatFontChanged -= ChatFontChanged;
		}
		
		void ChatFontChanged( object sender, EventArgs e ) { Recreate(); }
		
		public override void OnResize( int oldWidth, int oldHeight, int width, int height ) {
		}
		
		void DrawPosition() {
			int index = 0;
			TextureRec xy = new TextureRec( 2, posTex.Y1, baseWidth, posTex.Height );
			TextureRec uv = new TextureRec( 0, posTex.V1, posTex.U2, posTex.V2 );
			IGraphicsApi.Make2DQuad( xy, uv, game.ModelCache.vertices, ref index );
			
			Vector3I pos = Vector3I.Floor( game.LocalPlayer.Position );
			curX = baseWidth + 2;
			AddChar( 13, ref index );
			AddInt( pos.X, ref index, true );
			AddInt( pos.Y, ref index, true );
			AddInt( pos.Z, ref index, false );
			AddChar( 14, ref index );
			
			api.BindTexture( posTex.ID );
			api.UpdateDynamicIndexedVb( DrawMode.Triangles,
			                           game.ModelCache.vb, game.ModelCache.vertices, index, index * 6 / 4 );
		}
		
		bool speeding, halfSpeeding, noclip, fly;
		int lastFov;
		void UpdateHackState( bool force ) {
			HacksComponent hacks = game.LocalPlayer.Hacks;
			if( force || hacks.Speeding != speeding || hacks.HalfSpeeding != halfSpeeding || hacks.Noclip != noclip ||
			   hacks.Flying != fly || game.Fov != lastFov ) {
				speeding = hacks.Speeding; halfSpeeding = hacks.HalfSpeeding; noclip = hacks.Noclip; fly = hacks.Flying;
				lastFov = game.Fov;
				int index = 0;
				text.Clear();
				
				if( game.Fov != game.DefaultFov ) text.Append( ref index, "Zoom fov " )
					.AppendNum( ref index, lastFov ).Append( ref index, "  " );
				if( fly ) text.Append( ref index, "Fly ON   " );
				if( speeding || halfSpeeding ) text.Append( ref index, "Speed ON   " );
				if( noclip ) text.Append( ref index, "Noclip ON   " );
				hackStates.SetText( text.GetString() );
			}
		}

		const string possibleChars = "0123456789-, ()";
		int[] widths = new int[possibleChars.Length];
		int baseWidth, curX, posHeight;
		float texWidth;
		void MakePosTextWidget() {
			DrawTextArgs args = new DrawTextArgs( "", posFont, true );
			for( int i = 0; i < possibleChars.Length; i++ ) {
				args.Text = new String( possibleChars[i], 1 );
				widths[i] = game.Drawer2D.MeasureChatSize( ref args ).Width;
			}
			
			using( IDrawer2D drawer = game.Drawer2D ) {
				args.Text = "Feet pos: ";
				Size size = game.Drawer2D.MeasureChatSize( ref args );
				baseWidth = size.Width;
				size.Width += 16 * possibleChars.Length;
				
				using( Bitmap bmp = IDrawer2D.CreatePow2Bitmap( size ) ) {
					drawer.SetBitmap( bmp );
					drawer.DrawChatText( ref args, 0, 0 );
					
					for( int i = 0; i < possibleChars.Length; i++ ) {
						args.Text = new String( possibleChars[i], 1 );
						drawer.DrawChatText( ref args, baseWidth + 16 * i, 0 );
					}
					
					int y = fpsText.Height + 2;
					posTex = drawer.Make2DTexture( bmp, size, 0, y );
					game.Drawer2D.ReducePadding( ref posTex,
					                            Utils.Floor( font.Size ) );
					
					posTex.U2 = (float)baseWidth / bmp.Width;
					posTex.Width = baseWidth;
					texWidth = bmp.Width;
				}
			}
		}
		
		void AddChar( int charIndex, ref int index ) {
			int width = widths[charIndex];
			TextureRec xy = new TextureRec( curX, posTex.Y1, width, posTex.Height );
			TextureRec uv = new TextureRec( (baseWidth + charIndex * 16) / texWidth, 
			                               posTex.V1, width / texWidth, posTex.V2 );
			
			curX += width;
			IGraphicsApi.Make2DQuad( xy, uv, game.ModelCache.vertices, ref index );
		}
		
		void AddInt( int value, ref int index, bool more ) {
			if( value < 0 )
				AddChar( 10, ref index );
			
			int count = 0;
			value = Reverse( Math.Abs( value ), out count );
			for( int i = 0; i < count; i++ ) {
				AddChar( value % 10, ref index ); value /= 10;
			}
			
			if( more )
				AddChar( 11, ref index );
		}
		
		static int Reverse( int value, out int count ) {
			int orig = value, reversed = 0;
			count = 1; value /= 10;
			while( value > 0 ) {
				count++; value /= 10;
			}
			
			for( int i = 0; i < count; i++ ) {
				reversed *= 10;
				reversed += orig % 10;
				orig /= 10;
			}
			return reversed;
		}
	}
}
