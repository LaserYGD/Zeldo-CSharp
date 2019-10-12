using System;
using System.Collections.Generic;
using System.Diagnostics;
using Engine.Utility;
using Newtonsoft.Json;
using Zeldo.Entities.Core;

namespace Zeldo.Combat
{
	public class AttackData
	{
		public static Dictionary<int, AttackData> Load(string filename)
		{
			var map = JsonUtilities.Deserialize<Dictionary<string, AttackData>>("Combat/" + filename);
			var hashed = new Dictionary<int, AttackData>();

			// Attack names are internal, so they're stored using hash codes rather than the raw string.=
			foreach (var pair in map)
			{
				hashed.Add(pair.Key.GetHashCode(), pair.Value);
			}

			return hashed;
		}

		// Using an array for durations simplifies advancing through phases, including attacks where one or more phases
		// are unused (defined as that phase's duration being zero).
		private float[] phaseDurations;
		private string animation;

		private Type linkedType;

		public AttackData()
		{
			phaseDurations = new float[4];
		}

		public float[] Durations => phaseDurations;

		[JsonProperty("Class")]
		public string RawLinkedType
		{
			set => linkedType = Type.GetType(value);
		}

		public string Animation
		{
			get => animation;
			set
			{
				Debug.Assert(!string.IsNullOrEmpty(value), "Animation string can't be null or empty.");

				animation = value;
			}
		}

		[JsonProperty("Prepare")]
		public float PreparationTime
		{
			get => phaseDurations[(int)AttackPhases.Prepare - 1];
			set
			{
				Debug.Assert(value >= 0, "Preparation time can't be negative.");

				phaseDurations[(int)AttackPhases.Prepare - 1] = value;
			}
		}

		[JsonProperty("Execute")]
		public float ExecutionTime
		{
			get => phaseDurations[(int)AttackPhases.Execute - 1];
			set
			{
				Debug.Assert(value > 0, "Execution time must be positive.");

				phaseDurations[(int)AttackPhases.Execute - 1] = value;
			}
		}

		[JsonProperty("Cooldown")]
		public float CooldownTime
		{
			get => phaseDurations[(int)AttackPhases.Cooldown - 1];
			set
			{
				Debug.Assert(value >= 0, "Cooldown time can't be negative.");

				phaseDurations[(int)AttackPhases.Cooldown - 1] = value;
			}
		}

		[JsonProperty("Reset")]
		public float ResetTime
		{
			get => phaseDurations[(int)AttackPhases.Reset - 1];
			set
			{
				Debug.Assert(value >= 0, "Reset time can't be negative.");

				phaseDurations[(int)AttackPhases.Reset - 1] = value;
			}
		}

		public Attack<T> CreateAttack<T>(T parent) where T : LivingEntity
		{
			return (Attack<T>)Activator.CreateInstance(linkedType, this, parent);
		}
	}
}
