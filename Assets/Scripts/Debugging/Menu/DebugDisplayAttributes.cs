using System;

namespace DigDig2.Debugging.Menu
{
	public enum DebugMenuToggleable { Toggleable, NonToggleable }

	[AttributeUsage( AttributeTargets.Class )]
	public class DebugAttribute : Attribute
	{
		public DebugAttribute( DebugMenuToggleable isToggleable = DebugMenuToggleable.Toggleable ) { IsToggleable = isToggleable; }

		public DebugMenuToggleable IsToggleable { get; }
	}

	[AttributeUsage( AttributeTargets.Field )]
	public class DebugSerializedAttribute : Attribute
	{ }
}
