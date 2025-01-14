﻿// ClassicalSharp copyright 2014-2016 UnknownShadow200 | Licensed under MIT
// Part of fCraft | Copyright (c) 2009-2014 Matvei Stefarov <me@matvei.org> | BSD-3 | See LICENSE.txt
using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using ClassicalSharp.Entities;

namespace ClassicalSharp.Map {

	/// <summary> Imports a world from a LVL map file (MCLawl server map) </summary>
	public sealed class MapLvlImporter : IMapFormatImporter {

		const int Version = 1874;
		public byte[] Load( Stream stream, Game game, out int width, out int height, out int length ) {
			using( GZipStream gs = new GZipStream( stream, CompressionMode.Decompress ) ) {
				BinaryReader reader = new BinaryReader( gs );
				ushort header = reader.ReadUInt16();

				width = header == Version ? reader.ReadUInt16() : header;
				length = reader.ReadUInt16();
				height = reader.ReadUInt16();
				
				LocalPlayer p = game.LocalPlayer;
				p.Spawn.X = reader.ReadUInt16();
				p.Spawn.Z = reader.ReadUInt16();
				p.Spawn.Y = reader.ReadUInt16();
				p.SpawnYaw = (float)Utils.PackedToDegrees( reader.ReadByte() );
				p.SpawnPitch = (float)Utils.PackedToDegrees( reader.ReadByte() );
				
				if( header == Version )
					reader.ReadUInt16(); // pervisit and perbuild perms
				byte[] blocks = new byte[width * height * length];
				int read = gs.Read( blocks, 0, blocks.Length );
				ConvertPhysicsBlocks( blocks );
				
				if( gs.ReadByte() != 0xBD ) return blocks;
				ReadCustomBlocks( gs, width, height, length, blocks );
				return blocks;
			}
		}
		
		void ReadCustomBlocks( GZipStream gs, int width, int height, int length, byte[] blocks ) {
			byte[] chunk = new byte[16 * 16 * 16];
			
			for( int y = 0; y < height; y += 16 )
				for( int z = 0; z < length; z += 16 )
					for( int x = 0; x < width; x += 16 ) 
			{
				if( gs.ReadByte() != 1 ) continue;
				gs.Read( chunk, 0, chunk.Length );
				
				int baseIndex = (y * length + z) * width + x;
				for( int i = 0; i < chunk.Length; i++ ) {
					int bx = i & 0xF, by = (i >> 8) & 0xF, bz = (i >> 4) & 0xF;
					int index = baseIndex + (by * length + bz) * width + bx;
					
					if( blocks[index] == 163 ) // custom block id
						blocks[index] = chunk[i];
				}
			}
		}
			
		unsafe void ConvertPhysicsBlocks( byte[] blocks ) {
			byte* conv = stackalloc byte[256];
			int count = BlockInfo.CpeBlocksCount;
			for( int i = 0; i < count; i++ )
				conv[i] = (byte)i;
			for( int i = count; i < 256; i++ )
				conv[i] = table[i - count];
			
			for( int i = 0; i < blocks.Length; i++ )
				blocks[i] = conv[blocks[i]];
		}
		
		static byte[] table = new byte[] { 0, 0, 0, 0, 39, 36, 36, 10, 46, 21, 22,
			22, 22, 22, 4, 0, 22, 21, 0, 22, 23, 24, 22, 26, 27, 28, 30, 31, 32, 33,
			34, 35, 36, 22, 20, 49, 45, 1, 4, 0, 9, 11, 4, 19, 5, 17, 10, 49, 20, 1,
			18, 12, 5, 25, 46, 44, 17, 49, 20, 1, 18, 12, 5, 25, 36, 34, 0, 9, 11, 46,
			44, 0, 9, 11, 8, 10, 22, 27, 22, 8, 10, 28, 17, 49, 20, 1, 18, 12, 5, 25, 46,
			44, 11, 9, 0, 9, 11, 163, 0, 0, 9, 11, 0, 0, 0, 0, 0, 0, 0, 28, 22, 21, 11,
			0, 0, 0, 46, 46, 10, 10, 46, 20, 41, 42, 11, 9, 0, 8, 10, 10, 8, 0, 22, 22,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 21, 10, 0, 0, 0, 0, 0, 22, 22, 42, 3, 2, 29,
			47, 0, 0, 0, 0, 0, 27, 46, 48, 24, 22, 36, 34, 8, 10, 21, 29, 22, 10, 22, 22,
			41, 19, 35, 21, 29, 49, 34, 16, 41, 0, 22 };
	}
}