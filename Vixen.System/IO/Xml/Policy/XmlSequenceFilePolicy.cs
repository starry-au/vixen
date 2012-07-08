﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Vixen.IO.Policy;
using Vixen.Module;
using Vixen.Module.Media;
using Vixen.Sys;

namespace Vixen.IO.Xml.Policy {
	class XmlSequenceFilePolicy : SequenceFilePolicy {
		private Sequence _sequence;
		private XElement _content;

		private const string ATTR_LENGTH = "length";

		public XmlSequenceFilePolicy() {
			// Used when wanting just the current version of the sequence file.
		}

		public XmlSequenceFilePolicy(Sequence sequence, XElement content) {
			_sequence = sequence;
			_content = content;
		}

		protected override void WriteLength() {
			_content.Add(new XAttribute(ATTR_LENGTH, _sequence.Length.Ticks));
		}

		protected override void WriteTimingSource() {
			XmlSelectedTimingProviderSerializer serializer = new XmlSelectedTimingProviderSerializer();
			XElement element = serializer.WriteObject(_sequence.TimingProvider.SelectedTimingProvider);
			_content.Add(element);
		}

		protected override void WriteMedia() {
			XmlMediaCollectionSerializer serializer = new XmlMediaCollectionSerializer();
			XElement element = serializer.WriteObject(_sequence.GetAllMedia());
			_content.Add(element);
		}

		protected override void WriteModuleData() {
			XmlModuleLocalDataSetSerializer serializer = new XmlModuleLocalDataSetSerializer();
			XElement element = serializer.WriteObject(_sequence.ModuleDataSet);
			_content.Add(element);
		}

		protected override void WriteEffectNodes() {
			XmlEffectNodeCollectionSerializer serializer = new XmlEffectNodeCollectionSerializer();
			XElement element = serializer.WriteObject(_sequence.Data.GetMainStreamData().Cast<EffectNode>());
			_content.Add(element);
		}

		protected override void WriteFilterNodes() {
			XmlSequenceFilterNodeCollectionSerializer serializer = new XmlSequenceFilterNodeCollectionSerializer();
			XElement element = serializer.WriteObject(_sequence.GetAllSequenceFilters());
			_content.Add(element);
		}

		protected override void ReadLength() {
			string length = XmlHelper.GetAttribute(_content, ATTR_LENGTH);

			TimeSpan value = TimeSpan.Zero;
			if(length != null) {
				value = TimeSpan.FromTicks(long.Parse(length));
			}

			_sequence.Length = value;
		}

		protected override void ReadTimingSource() {
			XmlSelectedTimingProviderSerializer serializer = new XmlSelectedTimingProviderSerializer();
			_sequence.TimingProvider.SelectedTimingProvider = serializer.ReadObject(_content);
		}

		protected override void ReadMedia() {
			XmlMediaCollectionSerializer serializer = new XmlMediaCollectionSerializer();
			IEnumerable<IMediaModuleInstance> modules = serializer.ReadObject(_content);
			_sequence.ClearMedia();
			_sequence.AddMedia(modules);
		}

		protected override void ReadModuleData() {
			XmlModuleLocalDataSetSerializer serializer = new XmlModuleLocalDataSetSerializer();
			_sequence.ModuleDataSet = serializer.ReadObject(_content);
			// Side effect: With the module dataset now being available, get the sequence's runtime
			// behaviors' data.
			_GetBehaviorData(_sequence);
		}

		protected override void ReadEffectNodes() {
			XmlEffectNodeCollectionSerializer serializer = new XmlEffectNodeCollectionSerializer();
			IEnumerable<EffectNode> effectNodes = serializer.ReadObject(_content);
			_sequence.Data.ClearStream();
			_sequence.InsertData(effectNodes);
		}

		protected override void ReadFilterNodes() {
			XmlSequenceFilterNodeCollectionSerializer serializer = new XmlSequenceFilterNodeCollectionSerializer();
			IEnumerable<ISequenceFilterNode> filterNodes = serializer.ReadObject(_content);
			_sequence.ClearSequenceFilters();
			_sequence.AddSequenceFilters(filterNodes);
		}

		private void _GetBehaviorData(Sequence sequence) {
			foreach(IModuleInstance runtimeBehavior in sequence.RuntimeBehaviors) {
				sequence.ModuleDataSet.AssignModuleTypeData(runtimeBehavior);
			}
		}
	}
}