﻿// ClassicalSharp copyright 2014-2016 UnknownShadow200 | Licensed under MIT
using System;
using OpenTK;

namespace ClassicalSharp.Particles {
	
	public partial class ParticleManager : IDisposable {
		
		public void BreakBlockEffect( Vector3I position, byte block ) {
			Vector3 startPos = new Vector3( position.X, position.Y, position.Z );
			int texLoc = game.BlockInfo.GetTextureLoc( block, TileSide.Left ), texIndex = 0;
			TextureRec baseRec = game.TerrainAtlas1D.GetTexRec( texLoc, 1, out texIndex );			
			float uScale = (1/16f), vScale = (1/16f) * game.TerrainAtlas1D.invElementSize;
			
			Vector3 minBB = game.BlockInfo.MinBB[block];
			Vector3 maxBB = game.BlockInfo.MaxBB[block];
			int minU = Math.Min( (int)(minBB.X * 16), (int)(minBB.Z * 16) );
			int maxU = Math.Min( (int)(maxBB.X * 16), (int)(maxBB.Z * 16) );
			int minV = (int)(16 - maxBB.Y * 16), maxV = (int)(16 - minBB.Y * 16);
			int maxUsedU = maxU, maxUsedV = maxV;
			// This way we can avoid creating particles which outside the bounds and need to be clamped
			if( minU < 12 && maxU > 12 ) maxUsedU = 12;
			if( minV < 12 && maxV > 12 ) maxUsedV = 12;
			
			for( int i = 0; i < 30; i++ ) {
				double velX = rnd.NextDouble() * 0.8 - 0.4; // [-0.4, 0.4]
				double velZ = rnd.NextDouble() * 0.8 - 0.4;
				double velY = rnd.NextDouble() + 0.2;
				Vector3 velocity = new Vector3( (float)velX, (float)velY, (float)velZ );
				
				double xOffset = rnd.NextDouble() - 0.5; // [-0.5, 0.5]
				double yOffset = (rnd.NextDouble() - 0.125) * maxBB.Y;
				double zOffset = rnd.NextDouble() - 0.5;
				Vector3 pos = startPos + new Vector3( 0.5f + (float)xOffset,
				                                     (float)yOffset, 0.5f + (float)zOffset );
				
				TextureRec rec = baseRec;
				rec.U1 = baseRec.U1 + rnd.Next( minU, maxUsedU ) * uScale;
				rec.V1 = baseRec.V1 + rnd.Next( minV, maxUsedV ) * vScale;
				rec.U2 = Math.Min( baseRec.U1 + maxU * uScale, rec.U1 + 4 * uScale ) - 0.01f * uScale;
				rec.V2 = Math.Min( baseRec.V1 + maxV * vScale, rec.V1 + 4 * vScale ) - 0.01f * vScale;
				double life = 0.3 + rnd.NextDouble() * 0.7;
				
				TerrainParticle p = AddParticle( terrainParticles, ref terrainCount, false );
				p.ResetState( pos, velocity, life );
				p.rec = rec;
				p.texLoc = texLoc;
			}
		}
		
		public void AddRainParticle( Vector3 pos ) {
			Vector3 startPos = pos;			
			for( int i = 0; i < 2; i++ ) {
				double velX = rnd.NextDouble() * 0.8 - 0.4; // [-0.4, 0.4]
				double velZ = rnd.NextDouble() * 0.8 - 0.4;
				double velY = rnd.NextDouble() + 0.4;
				Vector3 velocity = new Vector3( (float)velX, (float)velY, (float)velZ );
				
				double xOffset = rnd.NextDouble() - 0.5; // [-0.5, 0.5]
				double yOffset = rnd.NextDouble() * 0.1 + 0.01;
				double zOffset = rnd.NextDouble() - 0.5;
				pos = startPos + new Vector3( 0.5f + (float)xOffset,
				                                     (float)yOffset, 0.5f + (float)zOffset );
				double life = 40;
				RainParticle p = AddParticle( rainParticles, ref rainCount, true );
				p.ResetState( pos, velocity, life );
				p.Big = rnd.Next( 0, 20 ) >= 18;
				p.Tiny = rnd.Next( 0, 30 ) >= 28;
			}
		}
		
		T AddParticle<T>( T[] particles, ref int count, bool rain ) where T : Particle, new() {
			if( count == maxParticles )
				RemoveAt( 0, particles, ref count );
			count++;
			
			T old = particles[count - 1];
			if( old != null ) return old;
			
			T newT = rain ? (T)(object)new RainParticle() : (T)(object)new TerrainParticle();
			particles[count - 1] = newT;
			return newT;
		}	
	}
}
