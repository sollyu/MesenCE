using Mesen.Debugger.ViewModels;
using Mesen.Interop;
using Mesen.Localization;
using Mesen.Debugger.Windows;
using System;
using System.Collections.Generic;
using static Mesen.Debugger.ViewModels.RegEntry;

namespace Mesen.Debugger.RegisterViewer;

public class SnesRegisterViewer
{
	public static List<RegisterViewerTab> GetTabs(ref SnesState snesState, HashSet<CpuType> cpuTypes, byte snesReg4210, byte snesReg4211, byte snesReg4212)
	{
		List<RegisterViewerTab> tabs = new() {
			GetSnesCpuTab(ref snesState, snesReg4210, snesReg4211, snesReg4212),
			GetSnesPpuTab(ref snesState),
			GetSnesDmaTab(ref snesState),
			GetSnesSpcTab(ref snesState),
			GetSnesDspTab(ref snesState)
		};

		if(cpuTypes.Contains(CpuType.Sa1)) {
			tabs.Add(GetSnesSa1Tab(ref snesState));
		} else if(cpuTypes.Contains(CpuType.Gameboy)) {
			GbState gbState = DebugApi.GetConsoleState<GbState>(ConsoleType.Gameboy);
			string tabPrefix = "GB - ";
			tabs.Add(GbRegisterViewer.GetGbLcdTab(ref gbState, tabPrefix));
			tabs.Add(GbRegisterViewer.GetGbApuTab(ref gbState, tabPrefix));
			tabs.Add(GbRegisterViewer.GetGbMiscTab(ref gbState, tabPrefix));
		} else if(cpuTypes.Contains(CpuType.Gsu)) {
			tabs.Add(GetSnesGsuTab(ref snesState.Gsu));
		} else if(cpuTypes.Contains(CpuType.St018)) {
			tabs.Add(GetSnesSt018Tab(ref snesState.St018));
		}

		return tabs;
	}

	private static RegisterViewerTab GetSnesGsuTab(ref GsuState gsu)
	{
		string viewName = nameof(RegisterViewerWindow);
		List<RegEntry> entries = new List<RegEntry>() {
			//new RegEntry("$3033.0", "Backup RAM Enabled", gsu.BackupRamEnabled),
			new RegEntry("", ResourceHelper.GetViewLabel(viewName, "regSnesRegisters")),
			new RegEntry("$3037.5", ResourceHelper.GetViewLabel(viewName, "regSnesHighSpeedMode"), gsu.HighSpeedMode),
			new RegEntry("$3037.7", ResourceHelper.GetViewLabel(viewName, "regSnesIrqDisabled"), gsu.IrqDisabled),
			new RegEntry("$3038", ResourceHelper.GetViewLabel(viewName, "regSnesScreenBaseAddress"), gsu.ScreenBase, Format.X8),
			new RegEntry("$3039.0", ResourceHelper.GetViewLabel(viewName, "regSnesClockSelect"), gsu.ClockSelect),
			new RegEntry("$303A.0-1", ResourceHelper.GetViewLabel(viewName, "regSnesColorGradient"), gsu.PlotBpp + " BPP", gsu.ColorGradient),
			new RegEntry("$303A.2+5", ResourceHelper.GetViewLabel(viewName, "regSnesScreenHeight"), gsu.ScreenHeight switch {
				0 => "128 px",
				1 => "160 px",
				2 => "192 px",
				3 or _ => ResourceHelper.GetViewLabel(viewName, "regSnesObjMode"),
			}, gsu.ScreenHeight),
			new RegEntry("$303A.3", ResourceHelper.GetViewLabel(viewName, "regSnesGsuRamAccessEnabled"), gsu.GsuRamAccess),
			new RegEntry("$303A.4", ResourceHelper.GetViewLabel(viewName, "regSnesGsuRomAccessEnabled"), gsu.GsuRomAccess),

			new RegEntry("", ResourceHelper.GetViewLabel(viewName, "regSnesPlotOptionRegister")),
			new RegEntry("", ResourceHelper.GetViewLabel(viewName, "regSnesTransparent"), gsu.PlotTransparent),
			new RegEntry("", ResourceHelper.GetViewLabel(viewName, "regSnesDither"), gsu.PlotDither),
			new RegEntry("", ResourceHelper.GetViewLabel(viewName, "regSnesColorHighNibble"), gsu.ColorHighNibble),
			new RegEntry("", ResourceHelper.GetViewLabel(viewName, "regSnesColorFreezeHigh"), gsu.ColorFreezeHigh),
			new RegEntry("", ResourceHelper.GetViewLabel(viewName, "regSnesObjectMode"), gsu.ObjMode),
			new RegEntry("", ResourceHelper.GetViewLabel(viewName, "regSnesTransparent"), gsu.PlotTransparent),
		};

		return new RegisterViewerTab("GSU", entries);
	}

	private static RegisterViewerTab GetSnesSt018Tab(ref St018State state)
	{
		string viewName = nameof(RegisterViewerWindow);
		List<RegEntry> entries = new List<RegEntry>() {
			new RegEntry("", ResourceHelper.GetViewLabel(viewName, "regSnesSnesRegisters")),
			new RegEntry("$3800", ResourceHelper.GetViewLabel(viewName, "regSnesArmToSnesData"), state.DataSnes),
			new RegEntry("$3804.0", ResourceHelper.GetViewLabel(viewName, "regSnesArmToSnesDataReady"), state.HasDataForSnes),
			new RegEntry("$3804.2", ResourceHelper.GetViewLabel(viewName, "regSnesAck"), state.Ack),
			new RegEntry("$3804.3", ResourceHelper.GetViewLabel(viewName, "regSnesSnesToArmDataReady"), state.HasDataForArm),
			new RegEntry("$3804.7", ResourceHelper.GetViewLabel(viewName, "regSnesArmCpuReset"), state.ArmReset),

			new RegEntry("", ResourceHelper.GetViewLabel(viewName, "regSnesSt018Registers")),
			new RegEntry("$40000010", ResourceHelper.GetViewLabel(viewName, "regSnesArmToSnesData"), state.DataArm),
			new RegEntry("$40000020.0", ResourceHelper.GetViewLabel(viewName, "regSnesArmToSnesDataReady"), state.HasDataForSnes),
			new RegEntry("$40000020.2", ResourceHelper.GetViewLabel(viewName, "regSnesAck"), state.Ack),
			new RegEntry("$40000020.3", ResourceHelper.GetViewLabel(viewName, "regSnesSnesToArmDataReady"), state.HasDataForArm),
			new RegEntry("$40000020.7", ResourceHelper.GetViewLabel(viewName, "regSnesArmCpuReset"), state.ArmReset),
		};

		return new RegisterViewerTab("ST018", entries);
	}

	private static RegisterViewerTab GetSnesSa1Tab(ref SnesState state)
	{
		Sa1State sa1 = state.Sa1;
		string viewName = nameof(RegisterViewerWindow);

		List<RegEntry> entries = new List<RegEntry>() {
			new RegEntry("$2200", ResourceHelper.GetViewLabel(viewName, "regSa1CpuControl")),
			new RegEntry("$2200.0-3", ResourceHelper.GetViewLabel(viewName, "regSa1Message"), sa1.Sa1MessageReceived, Format.X8),
			new RegEntry("$2200.4", ResourceHelper.GetViewLabel(viewName, "regSa1NmiRequested"), sa1.Sa1NmiRequested),
			new RegEntry("$2200.5", ResourceHelper.GetViewLabel(viewName, "regReset"), sa1.Sa1Reset),
			new RegEntry("$2200.6", ResourceHelper.GetViewLabel(viewName, "regSa1Wait"), sa1.Sa1Wait),
			new RegEntry("$2200.7", ResourceHelper.GetViewLabel(viewName, "regSa1IrqRequested"), sa1.Sa1IrqRequested),

			new RegEntry("$2201", ResourceHelper.GetViewLabel(viewName, "regSa1ScpuInterruptEnable")),
			new RegEntry("$2201.5", ResourceHelper.GetViewLabel(viewName, "regSa1CharacterConversionIrqEnable"), sa1.CharConvIrqEnabled),
			new RegEntry("$2201.7", ResourceHelper.GetViewLabel(viewName, "regIrqEnabled"), sa1.CpuIrqEnabled),

			new RegEntry("$2202", ResourceHelper.GetViewLabel(viewName, "regSa1ScpuInterruptClear")),
			new RegEntry("$2202.5", ResourceHelper.GetViewLabel(viewName, "regSa1CharacterIrqFlag"), sa1.CharConvIrqFlag),
			new RegEntry("$2202.7", ResourceHelper.GetViewLabel(viewName, "regSnesIrqFlag"), sa1.CpuIrqRequested),

			new RegEntry("$2203/4", ResourceHelper.GetViewLabel(viewName, "regSa1ResetVector"), sa1.Sa1ResetVector, Format.X16),
			new RegEntry("$2205/6", ResourceHelper.GetViewLabel(viewName, "regSa1NmiVector"), sa1.Sa1NmiVector, Format.X16),
			new RegEntry("$2207/8", ResourceHelper.GetViewLabel(viewName, "regSa1IrqVector"), sa1.Sa1IrqVector, Format.X16),

			new RegEntry("$2209", ResourceHelper.GetViewLabel(viewName, "regSa1ScpuControl")),
			new RegEntry("$2209.0-3", ResourceHelper.GetViewLabel(viewName, "regSa1Message"), sa1.CpuMessageReceived, Format.X8),
			new RegEntry("$2209.4", ResourceHelper.GetViewLabel(viewName, "regSa1UseNmiVector"), sa1.UseCpuNmiVector),
			new RegEntry("$2209.6", ResourceHelper.GetViewLabel(viewName, "regSa1UseIrqVector"), sa1.UseCpuIrqVector),
			new RegEntry("$2209.7", ResourceHelper.GetViewLabel(viewName, "regSa1IrqRequested"), sa1.CpuIrqRequested),

			new RegEntry("$220A", ResourceHelper.GetViewLabel(viewName, "regSa1CpuInterruptEnable")),
			new RegEntry("$220A.4", ResourceHelper.GetViewLabel(viewName, "regSa1NmiEnabled"), sa1.Sa1NmiEnabled),
			new RegEntry("$220A.5", ResourceHelper.GetViewLabel(viewName, "regSa1DmaIrqEnabled"), sa1.DmaIrqEnabled),
			new RegEntry("$220A.6", ResourceHelper.GetViewLabel(viewName, "regSa1TimerIrqEnabled"), sa1.TimerIrqEnabled),
			new RegEntry("$220A.7", ResourceHelper.GetViewLabel(viewName, "regSa1IrqEnabled"), sa1.Sa1IrqEnabled),

			new RegEntry("$220B", ResourceHelper.GetViewLabel(viewName, "regSa1ScpuInterruptClear")),
			new RegEntry("$220B.4", ResourceHelper.GetViewLabel(viewName, "regSa1NmiRequested"), sa1.Sa1NmiRequested),
			new RegEntry("$220B.5", ResourceHelper.GetViewLabel(viewName, "regSa1DmaIrqFlag"), sa1.DmaIrqFlag),
			new RegEntry("$220B.7", ResourceHelper.GetViewLabel(viewName, "regSa1IrqRequested"), sa1.Sa1IrqRequested),

			new RegEntry("$220C/D", ResourceHelper.GetViewLabel(viewName, "regSa1ScpuNmiVector"), sa1.CpuNmiVector, Format.X16),
			new RegEntry("$220E/F", ResourceHelper.GetViewLabel(viewName, "regSa1ScpuIrqVector"), sa1.CpuIrqVector, Format.X16),

			new RegEntry("$2210", ResourceHelper.GetViewLabel(viewName, "regSa1HvTimerControl")),
			new RegEntry("$2210.0", ResourceHelper.GetViewLabel(viewName, "regSa1HorizontalTimerEnabled"), sa1.HorizontalTimerEnabled),
			new RegEntry("$2210.1", ResourceHelper.GetViewLabel(viewName, "regSa1VerticalTimerEnabled"), sa1.VerticalTimerEnabled),
			new RegEntry("$2210.7", ResourceHelper.GetViewLabel(viewName, "regSa1LinearTimer"), sa1.UseLinearTimer),

			new RegEntry("$2212/3", ResourceHelper.GetViewLabel(viewName, "regSa1HTimer"), sa1.HTimer, Format.X16),
			new RegEntry("$2214/5", ResourceHelper.GetViewLabel(viewName, "regSa1VTimer"), sa1.VTimer, Format.X16),

			new RegEntry("", ResourceHelper.GetViewLabel(viewName, "regSa1MemoryMappings")),
			new RegEntry("$2220", string.Format(ResourceHelper.GetViewLabel(viewName, "regSa1MmcBank"), "C"), sa1.Banks[0], Format.X8),
			new RegEntry("$2221", string.Format(ResourceHelper.GetViewLabel(viewName, "regSa1MmcBank"), "D"), sa1.Banks[1], Format.X8),
			new RegEntry("$2222", string.Format(ResourceHelper.GetViewLabel(viewName, "regSa1MmcBank"), "E"), sa1.Banks[2], Format.X8),
			new RegEntry("$2223", string.Format(ResourceHelper.GetViewLabel(viewName, "regSa1MmcBank"), "F"), sa1.Banks[3], Format.X8),

			new RegEntry("$2224", ResourceHelper.GetViewLabel(viewName, "regSa1ScpuBwRamBank"), sa1.CpuBwBank, Format.X8),
			new RegEntry("$2225.0-6", ResourceHelper.GetViewLabel(viewName, "regSa1CpuBwRamBank"), sa1.Sa1BwBank, Format.X8),
			new RegEntry("$2225.7", ResourceHelper.GetViewLabel(viewName, "regSa1CpuBwRamMode"), sa1.Sa1BwMode, Format.X8),
			new RegEntry("$2226.7", ResourceHelper.GetViewLabel(viewName, "regSa1ScpuBwRamWriteEnabled"), sa1.CpuBwWriteEnabled),
			new RegEntry("$2227.7", ResourceHelper.GetViewLabel(viewName, "regSa1BwRamWriteEnabled"), sa1.Sa1BwWriteEnabled),
			new RegEntry("$2228.0-3", ResourceHelper.GetViewLabel(viewName, "regSa1ScpuBwRamWriteProtectedArea"), sa1.BwWriteProtectedArea, Format.X8),
			new RegEntry("$2229", ResourceHelper.GetViewLabel(viewName, "regSa1ScpuIRamWriteProtection"), sa1.CpuIRamWriteProtect, Format.X8),
			new RegEntry("$222A", ResourceHelper.GetViewLabel(viewName, "regSa1CpuIRamWriteProtection"), sa1.Sa1IRamWriteProtect, Format.X8),

			new RegEntry("$2230", ResourceHelper.GetViewLabel(viewName, "regSa1DmaControl")),
			new RegEntry("$2230.0-1", ResourceHelper.GetViewLabel(viewName, "regSa1DmaSourceDevice"), sa1.DmaSrcDevice),
			new RegEntry("$2230.2-3", ResourceHelper.GetViewLabel(viewName, "regSa1DmaDestinationDevice"), sa1.DmaDestDevice),
			new RegEntry("$2230.4", ResourceHelper.GetViewLabel(viewName, "regSa1AutomaticDmaCharacterConversion"), sa1.DmaCharConvAuto),
			new RegEntry("$2230.5", ResourceHelper.GetViewLabel(viewName, "regSa1DmaCharacterConversion"), sa1.DmaCharConv),
			new RegEntry("$2230.6", ResourceHelper.GetViewLabel(viewName, "regSa1DmaPriority"), sa1.DmaPriority),
			new RegEntry("$2230.7", ResourceHelper.GetViewLabel(viewName, "regSa1DmaEnabled"), sa1.DmaEnabled),

			new RegEntry("$2231.0-1", ResourceHelper.GetViewLabel(viewName, "regSa1CharacterFormat"), sa1.CharConvBpp),
			new RegEntry("$2231.2-5", ResourceHelper.GetViewLabel(viewName, "regSa1CharacterConversionWidth"), sa1.CharConvWidth, Format.X8),
			new RegEntry("$2231.7", ResourceHelper.GetViewLabel(viewName, "regSa1CharacterDmaActive"), sa1.CharConvDmaActive),

			new RegEntry("$2232/3/4", ResourceHelper.GetViewLabel(viewName, "regSa1DmaSourceAddress"), sa1.DmaSrcAddr, Format.X24),
			new RegEntry("$2235/6/7", ResourceHelper.GetViewLabel(viewName, "regSa1DmaDestinationAddress"), sa1.DmaDestAddr, Format.X24),

			new RegEntry("$2238/9", ResourceHelper.GetViewLabel(viewName, "regSa1DmaSize"), sa1.DmaSize, Format.X16),
			new RegEntry("$223F.7", ResourceHelper.GetViewLabel(viewName, "regSa1BwRam2BppMode"), sa1.BwRam2BppMode)
		};

		entries.Add(new RegEntry("", ResourceHelper.GetViewLabel(viewName, "regSa1BitmapRegisterFile")));
		for(int i = 0; i < 8; i++) {
			entries.Add(new RegEntry("$224" + i, string.Format(ResourceHelper.GetViewLabel(viewName, "regSa1Brf"), i), sa1.BitmapRegister1[i]));
		}
		for(int i = 0; i < 8; i++) {
			entries.Add(new RegEntry("$224" + (8 + i).ToString("X"), string.Format(ResourceHelper.GetViewLabel(viewName, "regSa1Brf"), i + 8), sa1.BitmapRegister2[i]));
		}

		entries.AddRange(new List<RegEntry>() {
			new RegEntry("", ResourceHelper.GetViewLabel(viewName, "regSa1MathRegisters")),
			new RegEntry("$2250.0-1", ResourceHelper.GetViewLabel(viewName, "regSa1MathOperation"), sa1.MathOp),
			new RegEntry("$2251/2", ResourceHelper.GetViewLabel(viewName, "regSa1MultiplicandDividend"), sa1.MultiplicandDividend, Format.X16),
			new RegEntry("$2253/4", ResourceHelper.GetViewLabel(viewName, "regSa1MultiplierDivisor"), sa1.MultiplierDivisor, Format.X16),

			new RegEntry("", ResourceHelper.GetViewLabel(viewName, "regSa1VariableLengthRegisters")),
			new RegEntry("$2258", ResourceHelper.GetViewLabel(viewName, "regSa1VariableLengthBitProcessing")),
			new RegEntry("$2258.0-3", ResourceHelper.GetViewLabel(viewName, "regSa1VariableLengthBitCount"), sa1.VarLenBitCount, Format.X8),
			new RegEntry("$2258.7", ResourceHelper.GetViewLabel(viewName, "regSa1VariableLengthAutoIncrement"), sa1.VarLenAutoInc),
			new RegEntry("$2259/A/B", ResourceHelper.GetViewLabel(viewName, "regSa1VariableLengthAddress"), sa1.VarLenAddress, Format.X24),

			new RegEntry("$2300", ResourceHelper.GetViewLabel(viewName, "regSa1ScpuStatusFlags")),
			new RegEntry("$2300.0-3", ResourceHelper.GetViewLabel(viewName, "regSa1MessageReceived"), sa1.CpuMessageReceived, Format.X8),
			new RegEntry("$2300.4", ResourceHelper.GetViewLabel(viewName, "regSa1UseNmiVector"), sa1.UseCpuNmiVector),
			new RegEntry("$2300.5", ResourceHelper.GetViewLabel(viewName, "regSa1CharacterConversionIrqFlag"), sa1.CharConvIrqFlag),
			new RegEntry("$2300.6", ResourceHelper.GetViewLabel(viewName, "regSa1UseIrqVector"), sa1.UseCpuIrqVector),
			new RegEntry("$2300.7", ResourceHelper.GetViewLabel(viewName, "regSa1IrqRequested"), sa1.CpuIrqRequested),

			new RegEntry("$2301", ResourceHelper.GetViewLabel(viewName, "regSa1StatusFlags")),
			new RegEntry("$2301.0-3", ResourceHelper.GetViewLabel(viewName, "regSa1MessageReceived"), sa1.Sa1MessageReceived, Format.X8),
			new RegEntry("$2301.4", ResourceHelper.GetViewLabel(viewName, "regSa1NmiRequested"), sa1.Sa1NmiRequested),
			new RegEntry("$2301.5", ResourceHelper.GetViewLabel(viewName, "regSa1DmaIrqFlag"), sa1.DmaIrqFlag),
			new RegEntry("$2301.7", ResourceHelper.GetViewLabel(viewName, "regSa1IrqRequested"), sa1.Sa1IrqRequested),

			new RegEntry("$2302/3", ResourceHelper.GetViewLabel(viewName, "regSa1HCounter"), 0, Format.X16),
			new RegEntry("$2304/5", ResourceHelper.GetViewLabel(viewName, "regSa1VCounter"), 0, Format.X16),

			new RegEntry("$2306/7/8/9/A", ResourceHelper.GetViewLabel(viewName, "regSa1MathResult"), sa1.MathOpResult),
			new RegEntry("$230B.7", ResourceHelper.GetViewLabel(viewName, "regSa1MathOverflow"), sa1.MathOverflow)
		});

		return new RegisterViewerTab("SA-1", entries);
	}

	private static RegisterViewerTab GetSnesPpuTab(ref SnesState state)
	{
		SnesPpuState ppu = state.Ppu;
		string viewName = nameof(RegisterViewerWindow);

		string GetLayerSize(LayerConfig layer)
		{
			return (layer.DoubleWidth ? "64" : "32") + "x" + (layer.DoubleHeight ? "64" : "32");
		}

		List<RegEntry> entries = new List<RegEntry>() {
			new RegEntry("", ResourceHelper.GetViewLabel(viewName, "regState")),
			new RegEntry("", ResourceHelper.GetViewLabel(viewName, "regCycleH"), ppu.Cycle),
			new RegEntry("", ResourceHelper.GetViewLabel(viewName, "regSnesHClock"), ppu.HClock),
			new RegEntry("", ResourceHelper.GetViewLabel(viewName, "regScanlineV"), ppu.Scanline),
			new RegEntry("", ResourceHelper.GetViewLabel(viewName, "regFrameNumber"), ppu.FrameCount),

			new RegEntry("$2100", ResourceHelper.GetViewLabel(viewName, "regSnesBrightness")),
			new RegEntry("$2100.0-3", ResourceHelper.GetViewLabel(viewName, "regSnesBrightness"), ppu.ScreenBrightness),
			new RegEntry("$2100.7", ResourceHelper.GetViewLabel(viewName, "regSnesForcedBlank"), ppu.ForcedBlank),
			new RegEntry("$2101", ResourceHelper.GetViewLabel(viewName, "regSnesOamSettings")),
			new RegEntry("$2101.0-2", ResourceHelper.GetViewLabel(viewName, "regSnesOamTableAddress"), ppu.OamBaseAddress, Format.X16),
			new RegEntry("$2101.3-4", ResourceHelper.GetViewLabel(viewName, "regSnesOamSecondTableAddress"), (ppu.OamBaseAddress + ppu.OamAddressOffset) & 0x7FFF, Format.X16),
			new RegEntry("$2101.5-7", ResourceHelper.GetViewLabel(viewName, "regSnesOamSizeMode"), ppu.OamMode),
			new RegEntry("$2102-2103", ResourceHelper.GetViewLabel(viewName, "regSnesOamBaseAddress"), ppu.OamRamAddress),
			new RegEntry("$2103.7", ResourceHelper.GetViewLabel(viewName, "regSnesOamPriority"), ppu.EnableOamPriority),
			new RegEntry("", ResourceHelper.GetViewLabel(viewName, "regSnesOamAddress"), ppu.InternalOamRamAddress),

			new RegEntry("$2105", ResourceHelper.GetViewLabel(viewName, "regSnesBgModeSize")),
			new RegEntry("$2105.0-2", ResourceHelper.GetViewLabel(viewName, "regSnesBgMode"), ppu.BgMode),
			new RegEntry("$2105.3", ResourceHelper.GetViewLabel(viewName, "regSnesMode1Bg3Priority"), ppu.Mode1Bg3Priority),
			new RegEntry("$2105.4", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesBgTiles"), 1), ppu.Layers[0].LargeTiles),
			new RegEntry("$2105.5", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesBgTiles"), 2), ppu.Layers[1].LargeTiles),
			new RegEntry("$2105.6", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesBgTiles"), 3), ppu.Layers[2].LargeTiles),
			new RegEntry("$2105.7", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesBgTiles"), 4), ppu.Layers[3].LargeTiles),

			new RegEntry("$2106", ResourceHelper.GetViewLabel(viewName, "regSnesMosaic")),
			new RegEntry("$2106.0", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesBgMosaicEnabled"), 1), (ppu.MosaicEnabled & 0x01) != 0),
			new RegEntry("$2106.1", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesBgMosaicEnabled"), 2), (ppu.MosaicEnabled & 0x02) != 0),
			new RegEntry("$2106.2", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesBgMosaicEnabled"), 3), (ppu.MosaicEnabled & 0x04) != 0),
			new RegEntry("$2106.3", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesBgMosaicEnabled"), 4), (ppu.MosaicEnabled & 0x08) != 0),
			new RegEntry("$2106.4-7", ResourceHelper.GetViewLabel(viewName, "regSnesMosaicSize"), (ppu.MosaicSize - 1).ToString() + " (" + ppu.MosaicSize.ToString() + "x" + ppu.MosaicSize.ToString() + ")", ppu.MosaicSize - 1),

			new RegEntry("$2107 - $210A", ResourceHelper.GetViewLabel(viewName, "regSnesTilemapAddressesSizes")),
			new RegEntry("$2107.0-1", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesBgSize"), 1), GetLayerSize(ppu.Layers[0]), (ppu.Layers[0].DoubleWidth ? 0x01 : 0) | (ppu.Layers[0].DoubleHeight ? 0x02 : 0)),
			new RegEntry("$2107.2-6", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesBgAddress"), 1), ppu.Layers[0].TilemapAddress, Format.X16),
			new RegEntry("$2108.0-1", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesBgSize"), 2), GetLayerSize(ppu.Layers[1]), (ppu.Layers[1].DoubleWidth ? 0x01 : 0) | (ppu.Layers[1].DoubleHeight ? 0x02 : 0)),
			new RegEntry("$2108.2-6", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesBgAddress"), 2), ppu.Layers[1].TilemapAddress, Format.X16),
			new RegEntry("$2109.0-1", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesBgSize"), 3), GetLayerSize(ppu.Layers[2]), (ppu.Layers[2].DoubleWidth ? 0x01 : 0) | (ppu.Layers[2].DoubleHeight ? 0x02 : 0)),
			new RegEntry("$2109.2-6", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesBgAddress"), 3), ppu.Layers[2].TilemapAddress, Format.X16),
			new RegEntry("$210A.0-1", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesBgSize"), 4), GetLayerSize(ppu.Layers[3]), (ppu.Layers[3].DoubleWidth ? 0x01 : 0) | (ppu.Layers[3].DoubleHeight ? 0x02 : 0)),
			new RegEntry("$210A.2-6", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesBgAddress"), 4), ppu.Layers[3].TilemapAddress, Format.X16),

			new RegEntry("$210B - $210C", ResourceHelper.GetViewLabel(viewName, "regSnesTileAddresses")),
			new RegEntry("$210B.0-2", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesBgTileAddress"), 1), ppu.Layers[0].ChrAddress, Format.X16),
			new RegEntry("$210B.4-6", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesBgTileAddress"), 2), ppu.Layers[1].ChrAddress, Format.X16),
			new RegEntry("$210C.0-2", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesBgTileAddress"), 3), ppu.Layers[2].ChrAddress, Format.X16),
			new RegEntry("$210C.4-6", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesBgTileAddress"), 4), ppu.Layers[3].ChrAddress, Format.X16),

			new RegEntry("$210D - $2114", ResourceHelper.GetViewLabel(viewName, "regSnesHvScrollOffsets")),
			new RegEntry("$210D", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesBgHOffset"), 1), ppu.Layers[0].HScroll, Format.X16),
			new RegEntry("$210D", ResourceHelper.GetViewLabel(viewName, "regSnesMode7HOffset"), ppu.Mode7.HScroll, Format.X16),
			new RegEntry("$210E", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesBgVOffset"), 1), ppu.Layers[0].VScroll, Format.X16),
			new RegEntry("$210E", ResourceHelper.GetViewLabel(viewName, "regSnesMode7VOffset"), ppu.Mode7.VScroll, Format.X16),

			new RegEntry("$210F", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesBgHOffset"), 2), ppu.Layers[1].HScroll, Format.X16),
			new RegEntry("$2110", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesBgVOffset"), 2), ppu.Layers[1].VScroll, Format.X16),
			new RegEntry("$2111", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesBgHOffset"), 3), ppu.Layers[2].HScroll, Format.X16),
			new RegEntry("$2112", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesBgVOffset"), 3), ppu.Layers[2].VScroll, Format.X16),
			new RegEntry("$2113", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesBgHOffset"), 4), ppu.Layers[3].HScroll, Format.X16),
			new RegEntry("$2114", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesBgVOffset"), 4), ppu.Layers[3].VScroll, Format.X16),

			new RegEntry("$2115 - $2117", "VRAM"),
			new RegEntry("$2115.0-1", ResourceHelper.GetViewLabel(viewName, "regSnesIncrementValue"), ppu.VramIncrementValue),
			new RegEntry("$2115.2-3", ResourceHelper.GetViewLabel(viewName, "regSnesAddressMapping"), ppu.VramAddressRemapping),
			new RegEntry("$2115.7", ResourceHelper.GetViewLabel(viewName, "regSnesIncrementOn2119"), ppu.VramAddrIncrementOnSecondReg),
			new RegEntry("$2116/7", ResourceHelper.GetViewLabel(viewName, "regVramAddress"), ppu.VramAddress, Format.X16),

			new RegEntry("$211A - $2120", ResourceHelper.GetViewLabel(viewName, "regSnesMode7")),
			new RegEntry("$211A.0", ResourceHelper.GetViewLabel(viewName, "regSnesMode7HorizontalMirroring"), ppu.Mode7.HorizontalMirroring),
			new RegEntry("$211A.1", ResourceHelper.GetViewLabel(viewName, "regSnesMode7VerticalMirroring"), ppu.Mode7.VerticalMirroring),
			new RegEntry("$211A.6", ResourceHelper.GetViewLabel(viewName, "regSnesMode7FillTile0"), ppu.Mode7.FillWithTile0),
			new RegEntry("$211A.7", ResourceHelper.GetViewLabel(viewName, "regSnesMode7LargeTilemap"), ppu.Mode7.LargeMap),

			new RegEntry("$211B", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesMode7Matrix"), "A"), ppu.Mode7.Matrix[0], Format.X16),
			new RegEntry("$211C", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesMode7Matrix"), "B"), ppu.Mode7.Matrix[1], Format.X16),
			new RegEntry("$211D", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesMode7Matrix"), "C"), ppu.Mode7.Matrix[2], Format.X16),
			new RegEntry("$211E", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesMode7Matrix"), "D"), ppu.Mode7.Matrix[3], Format.X16),

			new RegEntry("$211F", ResourceHelper.GetViewLabel(viewName, "regSnesMode7CenterX"), ppu.Mode7.CenterX, Format.X16),
			new RegEntry("$2120", ResourceHelper.GetViewLabel(viewName, "regSnesMode7CenterY"), ppu.Mode7.CenterY, Format.X16),

			new RegEntry("$2121", "CGRAM"),
			new RegEntry("$2121", ResourceHelper.GetViewLabel(viewName, "regSnesCgramAddress"), ppu.CgramAddress, Format.X16),
			new RegEntry("", ResourceHelper.GetViewLabel(viewName, "regSnesCgramNextWriteMsb"), ppu.CgramAddressLatch),

			new RegEntry("$2123 - $212B", ResourceHelper.GetViewLabel(viewName, "regSnesWindows")),
			new RegEntry("", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesBgWindows"), 1)),
			new RegEntry("$2123.0", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesBgWindowInverted"), 1, 1), ppu.Window[0].InvertedLayers[0] != 0),
			new RegEntry("$2123.1", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesBgWindowActive"), 1, 1), ppu.Window[0].ActiveLayers[0] != 0),
			new RegEntry("$2123.2", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesBgWindowInverted"), 1, 2), ppu.Window[1].InvertedLayers[0] != 0),
			new RegEntry("$2123.3", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesBgWindowActive"), 1, 2), ppu.Window[1].ActiveLayers[0] != 0),

			new RegEntry("", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesBgWindows"), 2)),
			new RegEntry("$2123.4", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesBgWindowInverted"), 2, 1), ppu.Window[0].InvertedLayers[1] != 0),
			new RegEntry("$2123.5", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesBgWindowActive"), 2, 1), ppu.Window[0].ActiveLayers[1] != 0),
			new RegEntry("$2123.6", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesBgWindowInverted"), 2, 2), ppu.Window[1].InvertedLayers[1] != 0),
			new RegEntry("$2123.7", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesBgWindowActive"), 2, 2), ppu.Window[1].ActiveLayers[1] != 0),

			new RegEntry("", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesBgWindows"), 3)),
			new RegEntry("$2124.0", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesBgWindowInverted"), 3, 1), ppu.Window[0].InvertedLayers[2] != 0),
			new RegEntry("$2124.1", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesBgWindowActive"), 3, 1), ppu.Window[0].ActiveLayers[2] != 0),
			new RegEntry("$2124.2", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesBgWindowInverted"), 3, 2), ppu.Window[1].InvertedLayers[2] != 0),
			new RegEntry("$2124.3", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesBgWindowActive"), 3, 2), ppu.Window[1].ActiveLayers[2] != 0),

			new RegEntry("", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesBgWindows"), 4)),
			new RegEntry("$2124.4", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesBgWindowInverted"), 4, 1), ppu.Window[0].InvertedLayers[3] != 0),
			new RegEntry("$2124.5", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesBgWindowActive"), 4, 1), ppu.Window[0].ActiveLayers[3] != 0),
			new RegEntry("$2124.6", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesBgWindowInverted"), 4, 2), ppu.Window[1].InvertedLayers[3] != 0),
			new RegEntry("$2124.7", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesBgWindowActive"), 4, 2), ppu.Window[1].ActiveLayers[3] != 0),

			new RegEntry("", ResourceHelper.GetViewLabel(viewName, "regSnesOamWindows")),
			new RegEntry("$2125.0", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesOamWindowInverted"), 1), ppu.Window[0].InvertedLayers[4] != 0),
			new RegEntry("$2125.1", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesOamWindowActive"), 1), ppu.Window[0].ActiveLayers[4] != 0),
			new RegEntry("$2125.2", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesOamWindowInverted"), 2), ppu.Window[1].InvertedLayers[4] != 0),
			new RegEntry("$2125.3", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesOamWindowActive"), 2), ppu.Window[1].ActiveLayers[4] != 0),

			new RegEntry("", ResourceHelper.GetViewLabel(viewName, "regSnesColorWindows")),
			new RegEntry("$2125.4", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesColorWindowInverted"), 1), ppu.Window[0].InvertedLayers[5] != 0),
			new RegEntry("$2125.5", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesColorWindowActive"), 1), ppu.Window[0].ActiveLayers[5] != 0),
			new RegEntry("$2125.6", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesColorWindowInverted"), 2), ppu.Window[1].InvertedLayers[5] != 0),
			new RegEntry("$2125.7", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesColorWindowActive"), 2), ppu.Window[1].ActiveLayers[5] != 0),

			new RegEntry("", ResourceHelper.GetViewLabel(viewName, "regSnesWindowPosition")),
			new RegEntry("$2126", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesWindowLeft"), 1), ppu.Window[0].Left),
			new RegEntry("$2127", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesWindowRight"), 1), ppu.Window[0].Right),
			new RegEntry("$2128", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesWindowLeft"), 2), ppu.Window[1].Left),
			new RegEntry("$2129", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesWindowRight"), 2), ppu.Window[1].Right),

			new RegEntry("", ResourceHelper.GetViewLabel(viewName, "regSnesWindowMasks")),
			new RegEntry("$212A.0-1", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesBgWindowMask"), 1), ppu.MaskLogic[0]),
			new RegEntry("$212A.2-3", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesBgWindowMask"), 2), ppu.MaskLogic[1]),
			new RegEntry("$212A.4-5", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesBgWindowMask"), 3), ppu.MaskLogic[2]),
			new RegEntry("$212A.6-7", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesBgWindowMask"), 4), ppu.MaskLogic[3]),
			new RegEntry("$212B.6-7", ResourceHelper.GetViewLabel(viewName, "regSnesOamWindowMask"), ppu.MaskLogic[4]),
			new RegEntry("$212B.6-7", ResourceHelper.GetViewLabel(viewName, "regSnesColorWindowMask"), ppu.MaskLogic[5]),

			new RegEntry("$212C", ResourceHelper.GetViewLabel(viewName, "regSnesMainScreenLayers")),
			new RegEntry("$212C.0", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesBgEnabled"), 1), (ppu.MainScreenLayers & 0x01) != 0),
			new RegEntry("$212C.1", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesBgEnabled"), 2), (ppu.MainScreenLayers & 0x02) != 0),
			new RegEntry("$212C.2", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesBgEnabled"), 3), (ppu.MainScreenLayers & 0x04) != 0),
			new RegEntry("$212C.3", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesBgEnabled"), 4), (ppu.MainScreenLayers & 0x08) != 0),
			new RegEntry("$212C.4", ResourceHelper.GetViewLabel(viewName, "regSnesOamEnabled"), (ppu.MainScreenLayers & 0x10) != 0),

			new RegEntry("$212D", ResourceHelper.GetViewLabel(viewName, "regSnesSubScreenLayers")),
			new RegEntry("$212D.0", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesBgEnabled"), 1), (ppu.SubScreenLayers & 0x01) != 0),
			new RegEntry("$212D.1", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesBgEnabled"), 2), (ppu.SubScreenLayers & 0x02) != 0),
			new RegEntry("$212D.2", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesBgEnabled"), 3), (ppu.SubScreenLayers & 0x04) != 0),
			new RegEntry("$212D.3", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesBgEnabled"), 4), (ppu.SubScreenLayers & 0x08) != 0),
			new RegEntry("$212D.4", ResourceHelper.GetViewLabel(viewName, "regSnesOamEnabled"), (ppu.SubScreenLayers & 0x10) != 0),

			new RegEntry("$212E", ResourceHelper.GetViewLabel(viewName, "regSnesMainScreenWindows")),
			new RegEntry("$212E.0", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesBgMainscreenWindowEnabled"), 1), ppu.WindowMaskMain[0] != 0),
			new RegEntry("$212E.1", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesBgMainscreenWindowEnabled"), 2), ppu.WindowMaskMain[1] != 0),
			new RegEntry("$212E.2", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesBgMainscreenWindowEnabled"), 3), ppu.WindowMaskMain[2] != 0),
			new RegEntry("$212E.3", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesBgMainscreenWindowEnabled"), 4), ppu.WindowMaskMain[3] != 0),
			new RegEntry("$212E.4", ResourceHelper.GetViewLabel(viewName, "regSnesOamMainscreenWindowEnabled"), ppu.WindowMaskMain[4] != 0),

			new RegEntry("$212F", ResourceHelper.GetViewLabel(viewName, "regSnesSubScreenWindows")),
			new RegEntry("$212F.0", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesBgSubscreenWindowEnabled"), 1), ppu.WindowMaskSub[0] != 0),
			new RegEntry("$212F.1", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesBgSubscreenWindowEnabled"), 2), ppu.WindowMaskSub[1] != 0),
			new RegEntry("$212F.2", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesBgSubscreenWindowEnabled"), 3), ppu.WindowMaskSub[2] != 0),
			new RegEntry("$212F.3", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesBgSubscreenWindowEnabled"), 4), ppu.WindowMaskSub[3] != 0),
			new RegEntry("$212F.4", ResourceHelper.GetViewLabel(viewName, "regSnesOamSubscreenWindowEnabled"), ppu.WindowMaskSub[4] != 0),

			new RegEntry("$2130 - $2131", ResourceHelper.GetViewLabel(viewName, "regSnesColorMath")),
			new RegEntry("$2130.0", ResourceHelper.GetViewLabel(viewName, "regSnesDirectColorMode"), ppu.DirectColorMode),
			new RegEntry("$2130.1", ResourceHelper.GetViewLabel(viewName, "regSnesColorMathAddSubscreen"), ppu.ColorMathAddSubscreen),
			new RegEntry("$2130.4-5", ResourceHelper.GetViewLabel(viewName, "regSnesColorMathPreventMode"), ppu.ColorMathPreventMode),
			new RegEntry("$2130.6-7", ResourceHelper.GetViewLabel(viewName, "regSnesColorMathClipMode"), ppu.ColorMathClipMode),

			new RegEntry("$2131.0", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesColorMathBgEnabled"), 1), (ppu.ColorMathEnabled & 0x01) != 0),
			new RegEntry("$2131.1", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesColorMathBgEnabled"), 2), (ppu.ColorMathEnabled & 0x02) != 0),
			new RegEntry("$2131.2", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesColorMathBgEnabled"), 3), (ppu.ColorMathEnabled & 0x04) != 0),
			new RegEntry("$2131.3", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesColorMathBgEnabled"), 4), (ppu.ColorMathEnabled & 0x08) != 0),
			new RegEntry("$2131.4", ResourceHelper.GetViewLabel(viewName, "regSnesColorMathOamEnabled"), (ppu.ColorMathEnabled & 0x10) != 0),
			new RegEntry("$2131.5", ResourceHelper.GetViewLabel(viewName, "regSnesColorMathBackgroundEnabled"), (ppu.ColorMathEnabled & 0x20) != 0),
			new RegEntry("$2131.6", ResourceHelper.GetViewLabel(viewName, "regSnesColorMathHalfMode"), ppu.ColorMathHalveResult),
			new RegEntry("$2131.7", ResourceHelper.GetViewLabel(viewName, "regSnesColorMathSubtractMode"), ppu.ColorMathSubtractMode),

			new RegEntry("$2132 - $2133", ResourceHelper.GetViewLabel(viewName, "regSnesMisc")),
			new RegEntry("$2132", ResourceHelper.GetViewLabel(viewName, "regSnesFixedColorBgr"), ppu.FixedColor, Format.X16),

			new RegEntry("$2133.0", ResourceHelper.GetViewLabel(viewName, "regSnesScreenInterlace"), ppu.ScreenInterlace),
			new RegEntry("$2133.1", ResourceHelper.GetViewLabel(viewName, "regSnesOamInterlace"), ppu.ObjInterlace),
			new RegEntry("$2133.2", ResourceHelper.GetViewLabel(viewName, "regSnesOverscanMode"), ppu.OverscanMode),
			new RegEntry("$2133.3", ResourceHelper.GetViewLabel(viewName, "regSnesHighResolutionMode"), ppu.HiResMode),
			new RegEntry("$2133.4", ResourceHelper.GetViewLabel(viewName, "regSnesExtBgEnabled"), ppu.ExtBgEnabled),
		};

		return new RegisterViewerTab("PPU", entries, CpuType.Snes, MemoryType.SnesRegister);
	}

	private static RegisterViewerTab GetSnesDspTab(ref SnesState state)
	{
		DspState dsp = state.Dsp;
		string viewName = nameof(RegisterViewerWindow);
		List<RegEntry> entries = new List<RegEntry>();

		void AddReg(int i, string name, bool signed = false)
		{
			entries.Add(new RegEntry("$" + i.ToString("X2"), name, signed ? (sbyte)dsp.Regs[i] : dsp.Regs[i], Format.X8));
		}

		AddReg(0x0C, ResourceHelper.GetViewLabel(viewName, "regSnesMainVolumeLeft"), true);
		AddReg(0x1C, ResourceHelper.GetViewLabel(viewName, "regSnesMainVolumeRight"), true);
		AddReg(0x2C, ResourceHelper.GetViewLabel(viewName, "regSnesEchoVolumeLeft"), true);
		AddReg(0x3C, ResourceHelper.GetViewLabel(viewName, "regSnesEchoVolumeRight"), true);

		AddReg(0x4C, ResourceHelper.GetViewLabel(viewName, "regSnesKeyOn"));
		AddReg(0x5C, ResourceHelper.GetViewLabel(viewName, "regSnesKeyOff"));

		AddReg(0x7C, ResourceHelper.GetViewLabel(viewName, "regSnesSourceEndBlock"));
		AddReg(0x0D, ResourceHelper.GetViewLabel(viewName, "regSnesEchoFeedback"));
		AddReg(0x2D, ResourceHelper.GetViewLabel(viewName, "regSnesPitchModulation"));
		AddReg(0x3D, ResourceHelper.GetViewLabel(viewName, "regSnesNoiseEnable"));
		AddReg(0x4D, ResourceHelper.GetViewLabel(viewName, "regSnesEchoEnable"));
		AddReg(0x5D, ResourceHelper.GetViewLabel(viewName, "regSnesSourceDirectory"));
		AddReg(0x6D, ResourceHelper.GetViewLabel(viewName, "regSnesEchoBuffer"));
		AddReg(0x7D, ResourceHelper.GetViewLabel(viewName, "regSnesEchoDelay"));

		entries.Add(new RegEntry("$6C", ResourceHelper.GetViewLabel(viewName, "regSnesFlags")));
		entries.Add(new RegEntry("$6C.0-4", ResourceHelper.GetViewLabel(viewName, "regSnesNoiseClock"), dsp.Regs[0x6C] & 0x1F, Format.X8));
		entries.Add(new RegEntry("$6C.5", ResourceHelper.GetViewLabel(viewName, "regSnesEchoDisabled"), (dsp.Regs[0x6C] & 0x20) != 0));
		entries.Add(new RegEntry("$6C.6", ResourceHelper.GetViewLabel(viewName, "regSnesMute"), (dsp.Regs[0x6C] & 0x40) != 0));
		entries.Add(new RegEntry("$6C.7", ResourceHelper.GetViewLabel(viewName, "regReset"), (dsp.Regs[0x6C] & 0x80) != 0));

		entries.Add(new RegEntry("$xF", ResourceHelper.GetViewLabel(viewName, "regSnesCoefficients")));
		for(int i = 0; i < 8; i++) {
			AddReg((i << 4) | 0x0F, string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesCoefficient"), i));
		}

		for(int i = 0; i < 8; i++) {
			entries.Add(new RegEntry(string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesVoice"), i), ""));

			int voice = i << 4;
			AddReg(voice | 0x00, ResourceHelper.GetViewLabel(viewName, "regSnesLeftVolume"), true);
			AddReg(voice | 0x01, ResourceHelper.GetViewLabel(viewName, "regSnesRightVolume"), true);
			entries.Add(new RegEntry("$" + i + "2 + $" + i + "3", ResourceHelper.GetViewLabel(viewName, "regSnesPitch"), dsp.Regs[voice | 0x02] | (dsp.Regs[voice | 0x03] << 8), Format.X16));
			AddReg(voice | 0x04, ResourceHelper.GetViewLabel(viewName, "regSnesSource"));
			AddReg(voice | 0x05, "ADSR1");
			AddReg(voice | 0x06, "ADSR2");
			AddReg(voice | 0x07, "GAIN");
			AddReg(voice | 0x08, "ENVX");
			AddReg(voice | 0x09, "OUTX");
		}

		return new RegisterViewerTab("DSP", entries);
	}

	private static RegisterViewerTab GetSnesSpcTab(ref SnesState state)
	{
		string GetTimerFrequency(double baseFreq, int divider)
		{
			return (divider == 0 ? (baseFreq / 256) : (baseFreq / divider)).ToString(".00") + " Hz";
		}

		SpcState spc = state.Spc;
		string viewName = nameof(RegisterViewerWindow);
		List<RegEntry> entries = new List<RegEntry>() {
			new RegEntry("$F0", ResourceHelper.GetViewLabel(viewName, "regSnesTest")),
			new RegEntry("$F0.0", ResourceHelper.GetViewLabel(viewName, "regSnesTimersDisabled"), spc.TimersDisabled),
			new RegEntry("$F0.1", ResourceHelper.GetViewLabel(viewName, "regSnesRamWriteEnabled"), spc.WriteEnabled),
			new RegEntry("$F0.3", ResourceHelper.GetViewLabel(viewName, "regSnesTimersEnabled"), spc.TimersEnabled),
			new RegEntry("$F0.4-5", ResourceHelper.GetViewLabel(viewName, "regSnesExternalSpeed"), spc.ExternalSpeed),
			new RegEntry("$F0.6-7", ResourceHelper.GetViewLabel(viewName, "regSnesInternalSpeed"), spc.InternalSpeed),

			new RegEntry("$F1", ResourceHelper.GetViewLabel(viewName, "regControl")),
			new RegEntry("$F1.0", ResourceHelper.GetViewLabel(viewName, "regSnesTimer0Enabled"), spc.Timer0.Enabled),
			new RegEntry("$F1.1", ResourceHelper.GetViewLabel(viewName, "regSnesTimer1Enabled"), spc.Timer1.Enabled),
			new RegEntry("$F1.2", ResourceHelper.GetViewLabel(viewName, "regSnesTimer2Enabled"), spc.Timer2.Enabled),
			new RegEntry("$F1.7", ResourceHelper.GetViewLabel(viewName, "regSnesIplRomEnabled"), spc.RomEnabled),

			new RegEntry("$F2", "DSP"),
			new RegEntry("$F2", ResourceHelper.GetViewLabel(viewName, "regSnesDspRegister"), spc.DspReg, Format.X8),

			new RegEntry("$F4 - $F7", ResourceHelper.GetViewLabel(viewName, "regSnesCpuSpcPorts")),
			new RegEntry("$F4", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesPortCpuRead"), 0), spc.OutputReg[0], Format.X8),
			new RegEntry("$F4", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesPortSpcRead"), 0), spc.CpuRegs[0], Format.X8),
			new RegEntry("$F5", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesPortCpuRead"), 1), spc.OutputReg[1], Format.X8),
			new RegEntry("$F5", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesPortSpcRead"), 1), spc.CpuRegs[1], Format.X8),
			new RegEntry("$F6", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesPortCpuRead"), 2), spc.OutputReg[2], Format.X8),
			new RegEntry("$F6", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesPortSpcRead"), 2), spc.CpuRegs[2], Format.X8),
			new RegEntry("$F7", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesPortCpuRead"), 3), spc.OutputReg[3], Format.X8),
			new RegEntry("$F7", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesPortSpcRead"), 3), spc.CpuRegs[3], Format.X8),

			new RegEntry("$F8 - $F9", ResourceHelper.GetViewLabel(viewName, "regSnesRamRegisters")),
			new RegEntry("$F8", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesRamReg"), 0), spc.RamReg[0], Format.X8),
			new RegEntry("$F9", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesRamReg"), 1), spc.RamReg[1], Format.X8),

			new RegEntry("", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesTimerNumber"), 0)),
			new RegEntry("$F1.0", ResourceHelper.GetViewLabel(viewName, "regEnabled"), spc.Timer0.Enabled),
			new RegEntry("$FA", ResourceHelper.GetViewLabel(viewName, "regSnesDivider"), spc.Timer0.Target, Format.X8),
			new RegEntry("$FD", ResourceHelper.GetViewLabel(viewName, "regOutput"), spc.Timer0.Output, Format.X8),
			new RegEntry("", ResourceHelper.GetViewLabel(viewName, "regTimer"), spc.Timer0.Stage2, Format.X8),
			new RegEntry("", ResourceHelper.GetViewLabel(viewName, "regFrequency"), GetTimerFrequency(8000, spc.Timer0.Target), spc.Timer0.Target),

			new RegEntry("", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesTimerNumber"), 1)),
			new RegEntry("$F1.1", ResourceHelper.GetViewLabel(viewName, "regEnabled"), spc.Timer1.Enabled),
			new RegEntry("$FB", ResourceHelper.GetViewLabel(viewName, "regSnesDivider"), spc.Timer1.Target, Format.X8),
			new RegEntry("$FE", ResourceHelper.GetViewLabel(viewName, "regOutput"), spc.Timer1.Output, Format.X8),
			new RegEntry("", ResourceHelper.GetViewLabel(viewName, "regTimer"), spc.Timer1.Stage2, Format.X8),
			new RegEntry("", ResourceHelper.GetViewLabel(viewName, "regFrequency"), GetTimerFrequency(8000, spc.Timer1.Target), spc.Timer1.Target),

			new RegEntry("", string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesTimerNumber"), 2)),
			new RegEntry("$F1.2", ResourceHelper.GetViewLabel(viewName, "regEnabled"), spc.Timer2.Enabled),
			new RegEntry("$FC", ResourceHelper.GetViewLabel(viewName, "regSnesDivider"), spc.Timer2.Target, Format.X8),
			new RegEntry("$FF", ResourceHelper.GetViewLabel(viewName, "regOutput"), spc.Timer2.Output, Format.X8),
			new RegEntry("", ResourceHelper.GetViewLabel(viewName, "regTimer"), spc.Timer2.Stage2, Format.X8),
			new RegEntry("", ResourceHelper.GetViewLabel(viewName, "regFrequency"), GetTimerFrequency(64000, spc.Timer2.Target), spc.Timer2.Target),
		};

		return new RegisterViewerTab("SPC", entries, CpuType.Spc, MemoryType.SpcMemory);
	}

	private static RegisterViewerTab GetSnesDmaTab(ref SnesState state)
	{
		List<RegEntry> entries = new List<RegEntry>();
		string viewName = nameof(RegisterViewerWindow);

		for(int i = 0; i < 8; i++) {
			DmaChannelConfig ch = state.Dma.Channels[i];
			entries.Add(new RegEntry(string.Format(ResourceHelper.GetViewLabel(viewName, "regSnesDmaChannel"), i), ""));
			entries.Add(new RegEntry("$420B." + i.ToString(), ResourceHelper.GetViewLabel(viewName, "regSnesChannelEnabled"), ch.DmaActive));
			entries.Add(new RegEntry("$420C." + i.ToString(), ResourceHelper.GetViewLabel(viewName, "regSnesHdmaEnabled"), (state.Dma.HdmaChannels & (1 << i)) != 0));

			entries.Add(new RegEntry("$43" + i.ToString() + "0.0-2", ResourceHelper.GetViewLabel(viewName, "regSnesTransferMode"), ch.TransferMode));
			entries.Add(new RegEntry("$43" + i.ToString() + "0.3", ResourceHelper.GetViewLabel(viewName, "regSnesFixed"), ch.FixedTransfer));
			entries.Add(new RegEntry("$43" + i.ToString() + "0.4", ResourceHelper.GetViewLabel(viewName, "regSnesDecrement"), ch.Decrement));
			entries.Add(new RegEntry("$43" + i.ToString() + "0.6", ResourceHelper.GetViewLabel(viewName, "regSnesIndirectHdma"), ch.HdmaIndirectAddressing));
			entries.Add(new RegEntry("$43" + i.ToString() + "0.7", ResourceHelper.GetViewLabel(viewName, "regSnesDirection"), ch.InvertDirection ? "B -> A" : "A -> B", ch.InvertDirection));

			entries.Add(new RegEntry("$43" + i.ToString() + "1", ResourceHelper.GetViewLabel(viewName, "regSnesBBusAddress"), ch.DestAddress, Format.X8));
			entries.Add(new RegEntry("$43" + i.ToString() + "2/3", ResourceHelper.GetViewLabel(viewName, "regSnesABusAddress"), ch.SrcAddress, Format.X16));
			entries.Add(new RegEntry("$43" + i.ToString() + "4", ResourceHelper.GetViewLabel(viewName, "regSnesABusBank"), ch.SrcBank, Format.X8));
			entries.Add(new RegEntry("$43" + i.ToString() + "5/6", ResourceHelper.GetViewLabel(viewName, "regSnesSize"), ch.TransferSize, Format.X16));

			entries.Add(new RegEntry("$43" + i.ToString() + "7", ResourceHelper.GetViewLabel(viewName, "regSnesHdmaBank"), ch.HdmaBank, Format.X8));
			entries.Add(new RegEntry("$43" + i.ToString() + "8/9", ResourceHelper.GetViewLabel(viewName, "regSnesHdmaAddress"), ch.HdmaTableAddress, Format.X16));
			entries.Add(new RegEntry("$43" + i.ToString() + "A", ResourceHelper.GetViewLabel(viewName, "regSnesHdmaLineCounter"), ch.HdmaLineCounterAndRepeat, Format.X8));
			entries.Add(new RegEntry("$43" + i.ToString() + "B", ResourceHelper.GetViewLabel(viewName, "regSnesUnusedRegister"), ch.UnusedRegister, Format.X8));
		}

		return new RegisterViewerTab("DMA", entries, CpuType.Snes, MemoryType.SnesRegister);
	}

	private static RegisterViewerTab GetSnesCpuTab(ref SnesState state, byte snesReg4210, byte snesReg4211, byte snesReg4212)
	{
		InternalRegisterState regs = state.InternalRegs;
		AluState alu = state.Alu;
		string viewName = nameof(RegisterViewerWindow);

		List<RegEntry> entries = new List<RegEntry>() {
			new RegEntry("$2181 - $2183", ResourceHelper.GetViewLabel(viewName, "regSnesWorkRamPosition"), state.WramPosition, Format.X24),

			new RegEntry("$4200 - $4201", ResourceHelper.GetViewLabel(viewName, "regSnesIrqNmiAutopollEnabled")),
			new RegEntry("$4200.0", ResourceHelper.GetViewLabel(viewName, "regSnesAutoJoypadPoll"), regs.EnableAutoJoypadRead),
			new RegEntry("$4200.4", ResourceHelper.GetViewLabel(viewName, "regSnesHIrqEnabled"), regs.EnableHorizontalIrq),
			new RegEntry("$4200.5", ResourceHelper.GetViewLabel(viewName, "regSnesVIrqEnabled"), regs.EnableVerticalIrq),
			new RegEntry("$4200.7", ResourceHelper.GetViewLabel(viewName, "regNmiEnabled"), regs.EnableNmi),

			new RegEntry("$4201", ResourceHelper.GetViewLabel(viewName, "regSnesIoPort"), regs.IoPortOutput, Format.X8),

			new RegEntry("$4202 - $4206", ResourceHelper.GetViewLabel(viewName, "regSnesMultDivInput")),
			new RegEntry("$4202", ResourceHelper.GetViewLabel(viewName, "regSnesMultiplicand"), alu.MultOperand1, Format.X8),
			new RegEntry("$4203", ResourceHelper.GetViewLabel(viewName, "regSnesMultiplier"), alu.MultOperand2, Format.X8),
			new RegEntry("$4204/5", ResourceHelper.GetViewLabel(viewName, "regSnesDividend"), alu.Dividend, Format.X16),
			new RegEntry("$4206", ResourceHelper.GetViewLabel(viewName, "regSnesDivisor"), alu.Divisor, Format.X8),

			new RegEntry("$4207 - $420A", ResourceHelper.GetViewLabel(viewName, "regSnesHIrqTimers")),
			new RegEntry("$4207/8", ResourceHelper.GetViewLabel(viewName, "regSnesHTimer"), regs.HorizontalTimer, Format.X16),
			new RegEntry("$4209/A", ResourceHelper.GetViewLabel(viewName, "regSnesVTimer"), regs.VerticalTimer, Format.X16),

			new RegEntry("$420D - $4212", ResourceHelper.GetViewLabel(viewName, "regSnesMiscFlags")),

			new RegEntry("$420D.0", ResourceHelper.GetViewLabel(viewName, "regSnesFastRomEnabled"), regs.EnableFastRom),
			new RegEntry("$4210.7", ResourceHelper.GetViewLabel(viewName, "regSnesNmiFlag"), (snesReg4210 & 0x80) != 0),
			new RegEntry("$4211.7", ResourceHelper.GetViewLabel(viewName, "regSnesIrqFlag"), (snesReg4211 & 0x80) != 0),

			new RegEntry("$4212.0", ResourceHelper.GetViewLabel(viewName, "regSnesAutoJoypadReadActive"), (snesReg4212 & 0x01) != 0),
			new RegEntry("$4212.6", ResourceHelper.GetViewLabel(viewName, "regSnesHBlankFlag"), (snesReg4212 & 0x40) != 0),
			new RegEntry("$4212.7", ResourceHelper.GetViewLabel(viewName, "regSnesVBlankFlag"), (snesReg4212 & 0x80) != 0),

			new RegEntry("$4214 - $4217", ResourceHelper.GetViewLabel(viewName, "regSnesMultDivResult")),
			new RegEntry("$4214/5", ResourceHelper.GetViewLabel(viewName, "regSnesQuotient"), alu.DivResult, Format.X16),
			new RegEntry("$4216/7", ResourceHelper.GetViewLabel(viewName, "regSnesProductRemainder"), alu.MultOrRemainderResult, Format.X16),

			new RegEntry("$4218 - $421F", ResourceHelper.GetViewLabel(viewName, "regSnesInputData")),
			new RegEntry("$4218/9", ResourceHelper.GetViewLabel(viewName, "regSnesP1Data"), regs.ControllerData[0], Format.X16),
			new RegEntry("$421A/B", ResourceHelper.GetViewLabel(viewName, "regSnesP2Data"), regs.ControllerData[1], Format.X16),
			new RegEntry("$421C/D", ResourceHelper.GetViewLabel(viewName, "regSnesP3Data"), regs.ControllerData[2], Format.X16),
			new RegEntry("$421E/F", ResourceHelper.GetViewLabel(viewName, "regSnesP4Data"), regs.ControllerData[3], Format.X16),
		};

		return new RegisterViewerTab("CPU", entries, CpuType.Snes, MemoryType.SnesRegister);
	}
}
