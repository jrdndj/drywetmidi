﻿using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    /// <summary>
    /// Settings according to which chords should be quantized.
    /// </summary>
    public class ChordsQuantizingSettings : LengthedObjectsQuantizingSettings<Chord>
    {
    }

    /// <summary>
    /// Provides methods to quantize chords time.
    /// </summary>
    /// <remarks>
    /// See <see href="xref:wiki_Quantizer">Quantizer</see> article on Wiki to learn more.
    /// </remarks>
    public class ChordsQuantizer : LengthedObjectsQuantizer<Chord, ChordsQuantizingSettings>
    {
    }
}
