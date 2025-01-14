﻿// ClassicalSharp copyright 2014-2016 UnknownShadow200 | Licensed under MIT
using System;
using ClassicalSharp.Entities;
using ClassicalSharp.GraphicsAPI;
using OpenTK;

namespace ClassicalSharp.Model {

	public class ChibiModel : HumanoidModel {
		
		public ChibiModel( Game window ) : base( window ) { }
		
		protected override void MakeDescriptions() {
			head = MakeBoxBounds( -4, 12, -4, 4, 20, 4 ).RotOrigin( 0, 13, 0 );
			torso = MakeBoxBounds( -4, -6, -2, 4, 6, 2 )
				.SetModelBounds( -2, 6, -1, 2, 12, 1 );
			lLeg = MakeBoxBounds( -2, -6, -2, 2, 6, 2 ).RotOrigin( 0, 6, 0 )
				.SetModelBounds( -2, 0, -1, 0, 6, 1 );
			rLeg = MakeBoxBounds( -2, -6, -2, 2, 6, 2 ).RotOrigin( 0, 6, 0 )
				.SetModelBounds( 0, 0, -1, 2, 6, 1 );
			lArm = MakeBoxBounds( -2, -6, -2, 2, 6, 2 ).RotOrigin( -3, 11, 0 )
				.SetModelBounds( -4, 6, -1, -2, 12, 1 );
			rArm = MakeBoxBounds( -2, -6, -2, 2, 6, 2 ).RotOrigin( 3, 11, 0 )
				.SetModelBounds( 2, 6, -1, 4, 12, 1 );
			offset = 0.5f * 0.5f;
		}

		public override float NameYOffset { get { return 1.3875f; } }
		
		public override float GetEyeY( Entity entity ) { return 14/16f; }
		
		public override Vector3 CollisionSize {
			get { return new Vector3( 4/16f + 0.6f/16f, 20.1f/16f, 4/16f + 0.6f/16f ); }
		}
		
		public override BoundingBox PickingBounds {
			get { return new BoundingBox( -4/16f, 0, -4/16f, 4/16f, 16/16f, 4/16f ); }
		}
	}
	
	public class GiantModel : HumanoidModel {
		
		const float scale = 2f;
		public GiantModel( Game window ) : base( window ) { }
		
		protected override void MakeDescriptions() {
			base.MakeDescriptions();
			head = head.Scale( scale ); torso = torso.Scale( scale );
			lLeg = lLeg.Scale( scale ); rLeg = rLeg.Scale( scale );
			lArm = lArm.Scale( scale ); rArm = rArm.Scale( scale );
			offset = 0.5f * scale;
		}

		public override float NameYOffset { get { return 2 * scale + 0.1375f; } }
		
		public override float GetEyeY( Entity entity ) { return base.GetEyeY( entity ) * scale; }
		
		public override Vector3 CollisionSize {
			get { return new Vector3( 8/16f * scale + 0.6f/16f, 
			                         28.1f/16f * scale, 8/16f * scale + 0.6f/16f ); }
		}
		
		public override BoundingBox PickingBounds {
			get { return base.PickingBounds.Scale( scale ); }
		}
	}
}
