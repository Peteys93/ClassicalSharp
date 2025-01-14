﻿// ClassicalSharp copyright 2014-2016 UnknownShadow200 | Licensed under MIT
using System;
using System.Drawing;
using ClassicalSharp.GraphicsAPI;

namespace ClassicalSharp.TexturePack {
	
	/// <summary> Represents a 2D packed texture atlas that has been converted into an array of 1D atlases. </summary>
	public sealed class TerrainAtlas1D : IDisposable {
		
		internal int elementsPerAtlas1D;
		internal int elementsPerBitmap;
		public float invElementSize;
		public int[] TexIds;
		IGraphicsApi graphics;
		
		public TerrainAtlas1D( IGraphicsApi graphics ) {
			this.graphics = graphics;
		}
		
		public TextureRec GetTexRec( int texId, int uCount, out int index ) {
			index = texId / elementsPerAtlas1D;
			int y = texId % elementsPerAtlas1D;
			// Adjust coords to be slightly inside - fixes issues with AMD/ATI cards.
			return new TextureRec( 0, y * invElementSize, (uCount - 1) + 15.99f/16f, (15.99f/16f) * invElementSize );
		}
		
		/// <summary> Returns the index of the 1D texture within the array of 1D textures
		/// containing the given texture id. </summary>
		public int Get1DIndex( int texId ) {
			return texId / elementsPerAtlas1D;
		}
		
		/// <summary> Returns the index of the given texture id within a 1D texture. </summary>
		public int Get1DRowId( int texId ) {
			return texId % elementsPerAtlas1D;
		}
		
		public void UpdateState( TerrainAtlas2D atlas2D ) {
			int maxVerticalSize = Math.Min( 4096, graphics.MaxTextureDimensions );
			int elementsPerFullAtlas = maxVerticalSize / atlas2D.elementSize;
			int totalElements = TerrainAtlas2D.RowsCount * TerrainAtlas2D.ElementsPerRow;
			
			int atlasesCount = Utils.CeilDiv( totalElements, elementsPerFullAtlas );
			elementsPerAtlas1D = Math.Min( elementsPerFullAtlas, totalElements );
			int atlas1DHeight = Utils.NextPowerOf2( elementsPerAtlas1D * atlas2D.elementSize );
			
			Convert2DTo1D( atlas2D, atlasesCount, atlas1DHeight );
			elementsPerBitmap = atlas1DHeight / atlas2D.elementSize;
			invElementSize = 1f / elementsPerBitmap;
		}
		
		void Convert2DTo1D( TerrainAtlas2D atlas2D, int atlasesCount, int atlas1DHeight ) {
			TexIds = new int[atlasesCount];
			Utils.LogDebug( "Loaded new atlas: {0} bmps, {1} per bmp", atlasesCount, elementsPerAtlas1D );
			int index = 0;
			
			using( FastBitmap atlas = new FastBitmap( atlas2D.AtlasBitmap, true, true ) ) {
				for( int i = 0; i < TexIds.Length; i++ )
					Make1DTexture( i, atlas, atlas2D, atlas1DHeight, ref index );
			}
		}
		
		void Make1DTexture( int i, FastBitmap atlas, TerrainAtlas2D atlas2D, int atlas1DHeight, ref int index ) {
			int elemSize = atlas2D.elementSize;
			using( Bitmap atlas1d = new Bitmap( atlas2D.elementSize, atlas1DHeight ) )
				using( FastBitmap dst = new FastBitmap( atlas1d, true, false ) )
			{
				for( int index1D = 0; index1D < elementsPerAtlas1D; index1D++ ) {
					FastBitmap.MovePortion( (index & 0x0F) * elemSize, (index >> 4) * elemSize,
					                       0, index1D * elemSize, atlas, dst, elemSize );
					index++;
				}
				TexIds[i] = graphics.CreateTexture( dst );
			}
		}
		
		public int CalcMaxUsedRow( TerrainAtlas2D atlas2D, BlockInfo info ) {
			int maxVerSize = Math.Min( 4096, graphics.MaxTextureDimensions );
			int verElements = maxVerSize / atlas2D.elementSize;
			int totalElements = GetMaxUsedRow( info.textures ) * TerrainAtlas2D.ElementsPerRow;
			
			Utils.LogDebug( "Used atlases: " + Utils.CeilDiv( totalElements, verElements ) );
			return Utils.CeilDiv( totalElements, verElements );
		}
		
		int GetMaxUsedRow( int[] textures ) {
			int maxElem = 0;
			for( int i = 0; i < textures.Length; i++ )
				maxElem = Math.Max( maxElem, textures[i] );
			return (maxElem >> 4) + 1;
		}
		
		public void Dispose() {
			if( TexIds == null ) return;
			
			for( int i = 0; i < TexIds.Length; i++ ) {
				graphics.DeleteTexture( ref TexIds[i] );
			}
		}
	}
}