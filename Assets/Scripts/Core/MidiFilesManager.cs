using Melanchall.DryWetMidi.Core;
using System;
using System.IO;

public static class MidiFilesManager
{
    public const string ClipsDirectoryName = "MidiClips";

    const string k_ClipNameFormat = "{0}_{1}.mid";
    const string k_ClipDateTimeFormat = "yyyy_MM_dd_HHmmss";

    public static void WriteNewMidiFile(MidiFile midiFile, string instrumentName)
    {
        if (!Directory.Exists(ClipsDirectoryName))
            Directory.CreateDirectory(ClipsDirectoryName);

        var subdir = Path.Combine(ClipsDirectoryName, instrumentName);
        if (!Directory.Exists(subdir))
            Directory.CreateDirectory(subdir);

        var fileName = string.Format(k_ClipNameFormat, instrumentName, DateTime.Now.ToString(k_ClipDateTimeFormat));
        midiFile.Write(Path.Combine(subdir, fileName));
    }
}
