﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vixen.Sys;
using Vixen.Common;
using Vixen.Module.Timing;

namespace Vixen.Module.RuntimeBehavior {
	public interface IRuntimeBehavior {
		void Startup(ISequence sequence, ITiming timingSource);
		void Shutdown();
		void Handle(CommandNode commandNode);
		bool Enabled { get; set; }
		Tuple<string, Action>[] BehaviorActions { get; }
	}
}