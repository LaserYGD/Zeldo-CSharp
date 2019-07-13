using Engine;
using GlmSharp;
using Jitter.LinearMath;
using Newtonsoft.Json.Linq;
using Zeldo.Entities.Core;
using Zeldo.Interfaces;

namespace Zeldo.Entities.Objects
{
    public class Cannon : Entity, IInteractive
    {
	    private static float muzzleVelocity;
	    private static float muzzleAngularVelocity;

	    static Cannon()
	    {
		    muzzleVelocity = Properties.GetFloat("cannon.muzzle.velocity");
		    muzzleAngularVelocity = Properties.GetFloat("cannon.muzzle.angular.velocity");
	    }

	    private bool isLoaded;

	    public Cannon() : base(EntityGroups.Object)
	    {
	    }

	    public bool IsInteractionEnabled => true;

        public override void Initialize(Scene scene, JToken data)
	    {
		    base.Initialize(scene, data);
	    }

	    public void OnInteract(Entity entity)
	    {
		    Player player = (Player)entity;

		    if (!isLoaded)
		    {
		    }
		    else
		    {
		    }
	    }

	    private void Fire()
	    {
			// TODO: Set position to the end of the barrel.
			// TODO: Set angular velocity to rotate around the barrel's axis (as if the barrel was grooved).
			// TODO: Set velocity appropriately (fixed speed).
			Cannonball cannonball = new Cannonball();
		    cannonball.Position = vec3.Zero;

		    var body = controllingBody3D;
		    body.LinearVelocity = JVector.Zero * muzzleVelocity;
		    body.AngularVelocity = JVector.Zero;

			Scene.Add(cannonball);
	    }
    }
}
