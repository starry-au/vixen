namespace VixenModules.Output.DmxUsbProMk2
{
	using System;
	using Vixen.Module.Controller;

	public class Descriptor : ControllerModuleDescriptorBase
	{
		private readonly Guid _typeId = new Guid("6C21DD5C-CDBE-4371-8A72-1F9893BC2A8C");

		public override string Author
		{
			get { return "Starry"; }
		}

		public override string Description
		{
			get { return "DMX USB Pro Mk2 hardware controller module"; }
		}

		public override Type ModuleClass
		{
			get { return typeof (Module); }
		}

		public override Type ModuleDataClass
		{
			get { return typeof (Data); }
		}

		public override Guid TypeId
		{
			get { return _typeId; }
		}

		public override string TypeName
		{
			get { return "DMX Pro Mk2"; }
		}

		public override string Version
		{
			get { return "1.0"; }
		}
	}
}