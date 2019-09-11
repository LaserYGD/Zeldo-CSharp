using Engine.Localization;
using Newtonsoft.Json.Linq;
using Zeldo.Entities.Core;
using Zeldo.UI.Hud;

namespace Zeldo.Entities.Bosses
{
	public abstract class Boss : LivingEntity
	{
		private BossHealthBar healthBar;
		private string name;

		protected Boss(string key) : base(EntityGroups.Boss)
		{
			name = Language.GetString(key + ".name");
		}

		public override void Initialize(Scene scene, JToken data)
		{
			healthBar = scene.Canvas.GetElement<BossHealthBar>();
			healthBar.MaxHealth = MaxHealth;
		}

		protected override void OnHealthChange(int oldHealth, int newHealth)
		{
			healthBar.Health = newHealth;
		}
	}
}
