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
		public static Dictionary<string, AttackData> Load(string filename)
		{
			return JsonUtilities.Deserialize<Dictionary<string, AttackData>>("Combat/" + filename);
		}

		// Using an array for durations simplifies advancing through phases, including attacks where one or more phases
		// are unused (defined as that phase's duration being zero).
		private float[] phaseDurations;

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

		public string Animation { get; set; }

		[JsonProperty("Prepare")]
		public float PreparationTime
		{
			get => phaseDurations[(int)AttackPhases.Prepare - 1];
			set => phaseDurations[(int)AttackPhases.Prepare - 1] = value;
		}

		[JsonProperty("Execute")]
		public float ExecutionTime
		{
			get => phaseDurations[(int)AttackPhases.Execute - 1];
			set => phaseDurations[(int)AttackPhases.Execute - 1] = value;
		}

		[JsonProperty("Cooldown")]
		public float CooldownTime
		{
			get => phaseDurations[(int)AttackPhases.Cooldown - 1];
			set => phaseDurations[(int)AttackPhases.Cooldown - 1] = value;
		}

		[JsonProperty("Reset")]
		public float ResetTime
		{
			get => phaseDurations[(int)AttackPhases.Reset - 1];
			set => phaseDurations[(int)AttackPhases.Reset - 1] = value;
		}

		public Attack<T> Activate<T>(T parent) where T : Entity
		{
			Debug.Assert(parent != null, "Can't activate an attack with a null parent.");

			return (Attack<T>)Activator.CreateInstance(linkedType, this, parent);
		}
	}
}
