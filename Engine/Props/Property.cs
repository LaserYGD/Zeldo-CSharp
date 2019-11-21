using System;
using System.Diagnostics;
using Engine.Core;

namespace Engine.Props
{
	public class Property
	{
		public Property(string key, PropertyTypes type)
		{
			Debug.Assert(!string.IsNullOrEmpty(key), "Property key is null or empty.");

			Key = key;
			Type = type;
		}

		public string Key { get; }

		public PropertyTypes Type { get; }

		public bool Test(string s)
		{
			switch (Type)
			{
				case PropertyTypes.Color: return Color.TryParse(s, out _);
				case PropertyTypes.Float: return float.TryParse(s, out _);
				case PropertyTypes.Integer: return int.TryParse(s, out _);
				case PropertyTypes.String: return !string.IsNullOrEmpty(s);
			}

			return true;
		}


		public override bool Equals(object obj)
		{
			if (!(obj is Property))
			{
				return false;
			}

			var other = (Property)obj;

			return Key == other.Key && Type == other.Type;
		}

		public override int GetHashCode()
		{
			return new Tuple<string, PropertyTypes>(Key, Type).GetHashCode();
		}
	}
}
