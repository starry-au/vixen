﻿using System.Collections.Generic;
using System.Linq;
using Vixen.Sys;
using Vixen.Sys.Output;

namespace Vixen.Rule.Patch {
	public class ToNOutputsAtOutput : IPatchingRule {
		public ToNOutputsAtOutput() {
		}

		public ToNOutputsAtOutput(ControllerReference startingPoint, int outputCountToPatch) {
			StartingPoint = startingPoint;
			OutputCountToPatch = outputCountToPatch;
		}

		public ControllerReference StartingPoint { get; set; }
		public int OutputCountToPatch { get; set; }

		public string Description {
			get { return "To multiple outputs on a single controller"; }
		}

		public IEnumerable<ControllerReferenceCollection> GenerateControllerReferenceCollections(int channelCount) {
			List<ControllerReferenceCollection> controllerReferences = new List<ControllerReferenceCollection>();

			// If the controller doesn't exist, abandon.
			OutputController controller = (OutputController)VixenSystem.Controllers.Get(StartingPoint.ControllerId);
			if(controller == null) return controllerReferences;

			int outputIndex = StartingPoint.OutputIndex;
			while(channelCount-- > 0 && PatchingHelper.IsValidOutputIndex(outputIndex, controller)) {
				IEnumerable<ControllerReference> references = PatchingHelper.GenerateControllerReferences(controller, outputIndex, OutputCountToPatch);
				controllerReferences.Add(new ControllerReferenceCollection(references));
				outputIndex += OutputCountToPatch;
			}

			return controllerReferences.ToArray();
		}
	}
}