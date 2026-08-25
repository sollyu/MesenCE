using Mesen.Debugger.ViewModels;
using Mesen.Debugger.Windows;
using Mesen.Interop;
using Mesen.Localization;
using System;
using System.Collections.Generic;
using static Mesen.Debugger.ViewModels.RegEntry;

namespace Mesen.Debugger.RegisterViewer;

public class NesRegisterViewer
{
	public static List<RegisterViewerTab> GetTabs(ref NesState nesState)
	{
		List<RegisterViewerTab> tabs = new() {
			GetNesPpuTab(ref nesState),
			GetNesApuTab(ref nesState)
		};

		RegisterViewerTab cartTab = GetNesCartTab(ref nesState);
		if(cartTab.Data.Count > 0) {
			tabs.Add(cartTab);
		}

		return tabs;
	}

	private static RegisterViewerTab GetNesPpuTab(ref NesState state)
	{
		NesPpuState ppu = state.Ppu;
		string viewName = nameof(RegisterViewerWindow);

		List<RegEntry> entries = new List<RegEntry>() {
			new RegEntry("", ResourceHelper.GetViewLabel(viewName, "regState")),
			new RegEntry("", ResourceHelper.GetViewLabel(viewName, "regCycleH"), ppu.Cycle),
			new RegEntry("", ResourceHelper.GetViewLabel(viewName, "regScanlineV"), ppu.Scanline),
			new RegEntry("", ResourceHelper.GetViewLabel(viewName, "regFrameNumber"), ppu.FrameCount),
			new RegEntry("", ResourceHelper.GetViewLabel(viewName, "regPpuBusAddress"), ppu.BusAddress, Format.X16),
			new RegEntry("", ResourceHelper.GetViewLabel(viewName, "regPpuRegisterBuffer"), ppu.MemoryReadBuffer, Format.X8),

			new RegEntry("$2000", ResourceHelper.GetViewLabel(viewName, "regControl")),
			new RegEntry("$2000.2", ResourceHelper.GetViewLabel(viewName, "regIncrementMode"), ppu.Control.VerticalWrite ? ResourceHelper.GetViewLabel(viewName, "regThirtyTwoBytes") : ResourceHelper.GetViewLabel(viewName, "regOneByte"), ppu.Control.VerticalWrite),
			new RegEntry("$2000.3", ResourceHelper.GetViewLabel(viewName, "regSpriteTableAddress"), ppu.Control.SpritePatternAddr == 0 ? "$0000" : "$1000", ppu.Control.SpritePatternAddr),
			new RegEntry("$2000.4", ResourceHelper.GetViewLabel(viewName, "regBackgroundTableAddress"), ppu.Control.BackgroundPatternAddr == 0 ? "$0000" : "$1000", ppu.Control.BackgroundPatternAddr),
			new RegEntry("$2000.5", ResourceHelper.GetViewLabel(viewName, "regSpriteSize"), ppu.Control.LargeSprites ? "8x16" : "8x8", ppu.Control.LargeSprites),
			new RegEntry("$2000.6", ResourceHelper.GetViewLabel(viewName, "regPpuSelect"), ppu.Control.SecondaryPpu ? ResourceHelper.GetViewLabel(viewName, "regSecondary") : ResourceHelper.GetViewLabel(viewName, "regMain"), ppu.Control.SecondaryPpu),
			new RegEntry("$2000.7", ResourceHelper.GetViewLabel(viewName, "regNmiEnabled"), ppu.Control.NmiOnVerticalBlank),

			new RegEntry("$2001", ResourceHelper.GetViewLabel(viewName, "regMask")),
			new RegEntry("$2001.0", ResourceHelper.GetViewLabel(viewName, "regGrayscale"), ppu.Mask.Grayscale),
			new RegEntry("$2001.1", ResourceHelper.GetViewLabel(viewName, "regBackgroundShowLeftmostPixels"), ppu.Mask.BackgroundMask),
			new RegEntry("$2001.2", ResourceHelper.GetViewLabel(viewName, "regSpritesShowLeftmostPixels"), ppu.Mask.SpriteMask),
			new RegEntry("$2001.3", ResourceHelper.GetViewLabel(viewName, "regBackgroundEnabled"), ppu.Mask.BackgroundEnabled),
			new RegEntry("$2001.4", ResourceHelper.GetViewLabel(viewName, "regSpritesEnabled"), ppu.Mask.SpritesEnabled),
			new RegEntry("$2001.5", ResourceHelper.GetViewLabel(viewName, "regRedEmphasis"), ppu.Mask.IntensifyRed),
			new RegEntry("$2001.6", ResourceHelper.GetViewLabel(viewName, "regGreenEmphasis"), ppu.Mask.IntensifyGreen),
			new RegEntry("$2001.7", ResourceHelper.GetViewLabel(viewName, "regBlueEmphasis"), ppu.Mask.IntensifyBlue),

			new RegEntry("$2002", ResourceHelper.GetViewLabel(viewName, "regStatus")),
			new RegEntry("$2002.5", ResourceHelper.GetViewLabel(viewName, "regSpriteOverflow"), ppu.StatusFlags.SpriteOverflow),
			new RegEntry("$2002.6", ResourceHelper.GetViewLabel(viewName, "regSpriteZeroHit"), ppu.StatusFlags.Sprite0Hit),
			new RegEntry("$2002.7", ResourceHelper.GetViewLabel(viewName, "regVerticalBlank"), ppu.StatusFlags.VerticalBlank),

			new RegEntry("$2003", ResourceHelper.GetViewLabel(viewName, "regOam1Address"), ppu.SpriteRamAddr, Format.X8),
			new RegEntry("", ResourceHelper.GetViewLabel(viewName, "regOam2Address"), ppu.SecondaryOamAddr & 0x1F, Format.X8),

			new RegEntry("$2005-2006", ResourceHelper.GetViewLabel(viewName, "regVramAddressScrolling")),
			new RegEntry("", ResourceHelper.GetViewLabel(viewName, "regVramAddress"), ppu.VideoRamAddr, Format.X16),
			new RegEntry("", "T", ppu.TmpVideoRamAddr, Format.X16),
			new RegEntry("", ResourceHelper.GetViewLabel(viewName, "regXScroll"), ppu.ScrollX),
			new RegEntry("", ResourceHelper.GetViewLabel(viewName, "regWriteToggle"), ppu.WriteToggle)
		};

		return new RegisterViewerTab("PPU", entries, CpuType.Nes, MemoryType.NesMemory);
	}

	private static RegisterViewerTab GetNesApuTab(ref NesState state)
	{
		List<RegEntry> entries = new List<RegEntry>();
		NesApuState apu = state.Apu;
		string viewName = nameof(RegisterViewerWindow);

		NesApuSquareState sq1 = apu.Square1;
		entries.AddRange(new List<RegEntry>() {
			new RegEntry("$4000-$4003", ResourceHelper.GetViewLabel(viewName, "regSquare1")),
			new RegEntry("$4000.0-3", ResourceHelper.GetViewLabel(viewName, "regEnvelopeVolume"), sq1.Envelope.Volume, Format.X8),
			new RegEntry("$4000.4", ResourceHelper.GetViewLabel(viewName, "regEnvelopeConstantVolume"), sq1.Envelope.ConstantVolume),
			new RegEntry("$4000.5", ResourceHelper.GetViewLabel(viewName, "regLengthCounterHalted"), sq1.LengthCounter.Halt),
			new RegEntry("$4000.6-7", ResourceHelper.GetViewLabel(viewName, "regDuty"), sq1.Duty),

			new RegEntry("$4001.0-2", ResourceHelper.GetViewLabel(viewName, "regSweepShift"), sq1.SweepShift),
			new RegEntry("$4001.3", ResourceHelper.GetViewLabel(viewName, "regSweepNegate"), sq1.SweepNegate),
			new RegEntry("$4001.4-6", ResourceHelper.GetViewLabel(viewName, "regSweepPeriod"), sq1.SweepPeriod),
			new RegEntry("$4001.7", ResourceHelper.GetViewLabel(viewName, "regSweepEnabled"), sq1.SweepEnabled),

			new RegEntry("$4002/$4003.0-2", ResourceHelper.GetViewLabel(viewName, "regPeriod"), sq1.Period, Format.X16),
			new RegEntry("$4003.3-7", ResourceHelper.GetViewLabel(viewName, "regLengthCounterReloadValue"), sq1.LengthCounter.ReloadValue, Format.X16),

			new RegEntry("--", ResourceHelper.GetViewLabel(viewName, "regEnabled"), sq1.Enabled),
			new RegEntry("--", ResourceHelper.GetViewLabel(viewName, "regTimer"), sq1.Timer, Format.X16),
			new RegEntry("--", ResourceHelper.GetViewLabel(viewName, "regFrequency"), Math.Round(sq1.Frequency).ToString("0.") + " Hz", null),
			new RegEntry("--", ResourceHelper.GetViewLabel(viewName, "regDutyPosition"), sq1.DutyPosition),

			new RegEntry("--", ResourceHelper.GetViewLabel(viewName, "regLengthCounterCounter"), sq1.LengthCounter.Counter, Format.X8),

			new RegEntry("--", ResourceHelper.GetViewLabel(viewName, "regEnvelopeCounter"), sq1.Envelope.Counter, Format.X8),
			new RegEntry("--", ResourceHelper.GetViewLabel(viewName, "regEnvelopeDivider"), sq1.Envelope.Divider, Format.X8),

			new RegEntry("--", ResourceHelper.GetViewLabel(viewName, "regOutput"), sq1.OutputVolume, Format.X8),
		});

		NesApuSquareState sq2 = apu.Square2;
		entries.AddRange(new List<RegEntry>() {
			new RegEntry("$4004-$4007", ResourceHelper.GetViewLabel(viewName, "regSquare2")),
			new RegEntry("$4004.0-3", ResourceHelper.GetViewLabel(viewName, "regEnvelopeVolume"), sq2.Envelope.Volume, Format.X8),
			new RegEntry("$4004.4", ResourceHelper.GetViewLabel(viewName, "regEnvelopeConstantVolume"), sq2.Envelope.ConstantVolume),
			new RegEntry("$4004.5", ResourceHelper.GetViewLabel(viewName, "regLengthCounterHalted"), sq2.LengthCounter.Halt),
			new RegEntry("$4004.6-7", ResourceHelper.GetViewLabel(viewName, "regDuty"), sq2.Duty),

			new RegEntry("$4005.0-2", ResourceHelper.GetViewLabel(viewName, "regSweepShift"), sq2.SweepShift),
			new RegEntry("$4005.3", ResourceHelper.GetViewLabel(viewName, "regSweepNegate"), sq2.SweepNegate),
			new RegEntry("$4005.4-6", ResourceHelper.GetViewLabel(viewName, "regSweepPeriod"), sq2.SweepPeriod),
			new RegEntry("$4005.7", ResourceHelper.GetViewLabel(viewName, "regSweepEnabled"), sq2.SweepEnabled),

			new RegEntry("$4006/$4007.0-2", ResourceHelper.GetViewLabel(viewName, "regPeriod"), sq2.Period, Format.X16),
			new RegEntry("$4007.3-7", ResourceHelper.GetViewLabel(viewName, "regLengthCounterReloadValue"), sq2.LengthCounter.ReloadValue, Format.X16),

			new RegEntry("--", ResourceHelper.GetViewLabel(viewName, "regEnabled"), sq2.Enabled),
			new RegEntry("--", ResourceHelper.GetViewLabel(viewName, "regTimer"), sq2.Timer, Format.X16),
			new RegEntry("--", ResourceHelper.GetViewLabel(viewName, "regFrequency"), Math.Round(sq2.Frequency).ToString("0.") + " Hz", null),
			new RegEntry("--", ResourceHelper.GetViewLabel(viewName, "regDutyPosition"), sq2.DutyPosition),

			new RegEntry("--", ResourceHelper.GetViewLabel(viewName, "regLengthCounterCounter"), sq2.LengthCounter.Counter, Format.X8),

			new RegEntry("--", ResourceHelper.GetViewLabel(viewName, "regEnvelopeCounter"), sq2.Envelope.Counter, Format.X8),
			new RegEntry("--", ResourceHelper.GetViewLabel(viewName, "regEnvelopeDivider"), sq2.Envelope.Divider, Format.X8),

			new RegEntry("--", ResourceHelper.GetViewLabel(viewName, "regOutput"), sq2.OutputVolume, Format.X8),
		});

		NesApuTriangleState trg = apu.Triangle;
		entries.AddRange(new List<RegEntry>() {
			new RegEntry("$4008-$400B", ResourceHelper.GetViewLabel(viewName, "regTriangle")),
			new RegEntry("$4008.0-6", ResourceHelper.GetViewLabel(viewName, "regLinearCounterReload"), trg.LinearCounterReload, Format.X8),
			new RegEntry("$4008.7", ResourceHelper.GetViewLabel(viewName, "regLengthCounterHalted"), trg.LengthCounter.Halt),

			new RegEntry("$400A/$400B.0-2", ResourceHelper.GetViewLabel(viewName, "regPeriod"), trg.Period, Format.X16),
			new RegEntry("$400B.3-7", ResourceHelper.GetViewLabel(viewName, "regLengthCounterReloadValue"), trg.LengthCounter.ReloadValue, Format.X16),

			new RegEntry("--", ResourceHelper.GetViewLabel(viewName, "regEnabled"), trg.Enabled),
			new RegEntry("--", ResourceHelper.GetViewLabel(viewName, "regTimer"), trg.Timer),
			new RegEntry("--", ResourceHelper.GetViewLabel(viewName, "regFrequency"), Math.Round(trg.Frequency).ToString("0.") + " Hz", null),
			new RegEntry("--", ResourceHelper.GetViewLabel(viewName, "regSequencePosition"), trg.SequencePosition),

			new RegEntry("--", ResourceHelper.GetViewLabel(viewName, "regLengthCounterCounter"), trg.LengthCounter.Counter),

			new RegEntry("--", ResourceHelper.GetViewLabel(viewName, "regLinearCounterCounter"), trg.LinearCounter),
			new RegEntry("--", ResourceHelper.GetViewLabel(viewName, "regLinearCounterReloadFlag"), trg.LinearReloadFlag),

			new RegEntry("--", ResourceHelper.GetViewLabel(viewName, "regOutput"), trg.OutputVolume),
		});

		NesApuNoiseState noise = apu.Noise;
		entries.AddRange(new List<RegEntry>() {
			new RegEntry("$400C-$400F", ResourceHelper.GetViewLabel(viewName, "regNoise")),
			new RegEntry("$400C.0-3", ResourceHelper.GetViewLabel(viewName, "regEnvelopeVolume"), noise.Envelope.Volume, Format.X8),
			new RegEntry("$400C.4", ResourceHelper.GetViewLabel(viewName, "regEnvelopeConstantVolume"), noise.Envelope.ConstantVolume),
			new RegEntry("$400C.5", ResourceHelper.GetViewLabel(viewName, "regLengthCounterHalted"), noise.LengthCounter.Halt),

			new RegEntry("$400E.0-3", ResourceHelper.GetViewLabel(viewName, "regPeriod"), noise.Period, Format.X16),
			new RegEntry("$400E.7", ResourceHelper.GetViewLabel(viewName, "regModeFlag"), noise.ModeFlag),

			new RegEntry("$400F.3-7", ResourceHelper.GetViewLabel(viewName, "regLengthCounterReloadValue"), noise.LengthCounter.ReloadValue, Format.X8),

			new RegEntry("--", ResourceHelper.GetViewLabel(viewName, "regEnabled"), noise.Enabled),
			new RegEntry("--", ResourceHelper.GetViewLabel(viewName, "regTimer"), noise.Timer),
			new RegEntry("--", ResourceHelper.GetViewLabel(viewName, "regFrequency"), Math.Round(noise.Frequency).ToString("0.") + " Hz", null),
			new RegEntry("--", ResourceHelper.GetViewLabel(viewName, "regShiftRegister"), noise.ShiftRegister),

			new RegEntry("--", ResourceHelper.GetViewLabel(viewName, "regEnvelopeCounter"), noise.Envelope.Counter, Format.X8),
			new RegEntry("--", ResourceHelper.GetViewLabel(viewName, "regEnvelopeDivider"), noise.Envelope.Divider, Format.X8),

			new RegEntry("--", ResourceHelper.GetViewLabel(viewName, "regLengthCounterCounter"), noise.LengthCounter.Counter),

			new RegEntry("--", ResourceHelper.GetViewLabel(viewName, "regOutput"), noise.OutputVolume),
		});

		NesApuDmcState dmc = apu.Dmc;
		entries.AddRange(new List<RegEntry>() {
			new RegEntry("$4010-4013", "DMC"),
			new RegEntry("$4010.0-3", ResourceHelper.GetViewLabel(viewName, "regPeriod"), dmc.Period, Format.X16),
			new RegEntry("$4010.6", ResourceHelper.GetViewLabel(viewName, "regLoopFlag"), dmc.Loop),
			new RegEntry("$4010.7", ResourceHelper.GetViewLabel(viewName, "regIrqEnabled"), dmc.IrqEnabled),

			new RegEntry("$4011", ResourceHelper.GetViewLabel(viewName, "regOutputLevel"), dmc.OutputVolume),

			new RegEntry("$4012", ResourceHelper.GetViewLabel(viewName, "regSampleAddress"), dmc.SampleAddr, Format.X16),
			new RegEntry("$4013", ResourceHelper.GetViewLabel(viewName, "regSampleLength"), dmc.SampleLength, Format.X16),

			new RegEntry("--", ResourceHelper.GetViewLabel(viewName, "regTimer"), dmc.Timer),
			new RegEntry("--", ResourceHelper.GetViewLabel(viewName, "regFrequency"), Math.Round(dmc.SampleRate).ToString("0."), null),
			new RegEntry("--", ResourceHelper.GetViewLabel(viewName, "regBytesRemaining"), dmc.BytesRemaining),
			new RegEntry("--", ResourceHelper.GetViewLabel(viewName, "regNextSampleAddress"), dmc.NextSampleAddr, Format.X16),
		});

		NesApuFrameCounterState frameCounter = apu.FrameCounter;
		entries.AddRange(new List<RegEntry>() {
			new RegEntry("$4017", ResourceHelper.GetViewLabel(viewName, "regFrameCounter")),
			new RegEntry("$4017.6", ResourceHelper.GetViewLabel(viewName, "regIrqEnabled"), frameCounter.IrqEnabled),
			new RegEntry("$4017.7", ResourceHelper.GetViewLabel(viewName, "regFiveStepMode"), frameCounter.FiveStepMode),
			new RegEntry("--", ResourceHelper.GetViewLabel(viewName, "regSequencePosition"), frameCounter.SequencePosition),
		});

		return new RegisterViewerTab("APU", entries, CpuType.Nes, MemoryType.NesMemory);
	}

	private static RegisterViewerTab GetNesCartTab(ref NesState state)
	{
		NesCartridgeState cart = state.Cartridge;

		List<RegEntry> entries = new List<RegEntry>();
		for(int i = 0; i < cart.CustomEntryCount; i++) {
			ref MapperStateEntry entry = ref cart.CustomEntries[i];
			Format format = entry.Type switch {
				MapperStateValueType.Number8 => Format.X8,
				MapperStateValueType.Number16 => Format.X16,
				MapperStateValueType.Number32 => Format.X32,
				_ => Format.None
			};

			object? value = entry.GetValue();
			string addr = entry.GetAddress();
			string name = entry.GetName();

			if(value is ISpanFormattable) {
				entries.Add(new RegEntry(addr, name, (ISpanFormattable)value, format));
			} else if(value is bool) {
				entries.Add(new RegEntry(addr, name, (bool)value));
			} else if(value is string) {
				entries.Add(new RegEntry(addr, name, (string)value, entry.RawValue != Int64.MinValue ? entry.RawValue : null));
			} else {
				entries.Add(new RegEntry(addr, name));
			}
		}

		return new RegisterViewerTab("Cart", entries, CpuType.Nes, MemoryType.NesMemory);
	}
}
