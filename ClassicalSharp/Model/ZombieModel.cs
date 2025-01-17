﻿// ClassicalSharp copyright 2014-2016 UnknownShadow200 | Licensed under MIT
using System;
using ClassicalSharp.Entities;
using ClassicalSharp.GraphicsAPI;
using OpenTK;

namespace ClassicalSharp.Model {

	public class ZombieModel : IModel {
		
		public ZombieModel( Game window ) : base( window ) { }
		
		internal override void CreateParts() {
			vertices = new ModelVertex[boxVertices * 6];
			Head = BuildBox( MakeBoxBounds( -4, 24, -4, 4, 32, 4 )
			                .TexOrigin( 0, 0 )
			                .RotOrigin( 0, 24, 0 ) );
			Torso = BuildBox( MakeBoxBounds( -4, 12, -2, 4, 24, 2 )
			                  .TexOrigin( 16, 16 ) );
			LeftLeg = BuildBox( MakeBoxBounds( 0, 0, -2, -4, 12, 2 )
			                    .TexOrigin( 0, 16 )
			                    .RotOrigin( 0, 12, 0 ) );
			RightLeg = BuildBox( MakeBoxBounds( 0, 0, -2, 4, 12, 2 )
			                    .TexOrigin( 0, 16 )
			                    .RotOrigin( 0, 12, 0 ) );
			LeftArm = BuildBox( MakeBoxBounds( -4, 12, -2, -8, 24, 2 )
			                   .TexOrigin( 40, 16 )
			                   .RotOrigin( -6, 22, 0 ) );
			RightArm = BuildBox( MakeBoxBounds( 4, 12, -2, 8, 24, 2 )
			                    .TexOrigin( 40, 16 )
			                    .RotOrigin( 6, 22, 0 ) );
		}
		
		public override bool Bobbing { get { return true; } }

		public override float NameYOffset { get { return 2.075f; } }
		
		public override float GetEyeY( Entity entity ) { return 26/16f; }
		
		public override Vector3 CollisionSize {
			get { return new Vector3( 8/16f + 0.6f/16f, 28.1f/16f, 8/16f + 0.6f/16f ); }
		}
		
		public override BoundingBox PickingBounds {
			get { return new BoundingBox( -4/16f, 0, -4/16f, 4/16f, 32/16f, 4/16f ); }
		}
		
		protected override void DrawModel( Player p ) {
			int texId = p.MobTextureId <= 0 ? cache.ZombieTexId : p.MobTextureId;
			graphics.BindTexture( texId );
			DrawHeadRotate( -p.PitchRadians, 0, 0, Head );
			
			DrawPart( Torso );
			DrawRotate( p.anim.legXRot, 0, 0, LeftLeg );
			DrawRotate( -p.anim.legXRot, 0, 0, RightLeg );
			DrawRotate( 90 * Utils.Deg2Rad, 0, p.anim.armZRot, LeftArm );
			DrawRotate( 90 * Utils.Deg2Rad, 0, -p.anim.armZRot, RightArm );
			graphics.UpdateDynamicIndexedVb( DrawMode.Triangles, cache.vb, cache.vertices, index, index * 6 / 4 );
		}
		
		ModelPart Head, Torso, LeftLeg, RightLeg, LeftArm, RightArm;
	}
}