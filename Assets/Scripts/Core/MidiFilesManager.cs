using Melanchall.DryWetMidi.Core;
using System;
using System.Collections.Generic;
using System.IO;

public static class MidiFilesManager
{
    public const string ClipsDirectoryName = "MidiClips";

    const string k_MidiExtension = ".mid";
    const string k_ClipNameFormat = "{0}_{1}" + k_MidiExtension;
    const string k_ClipDateTimeFormat = "yyyy_MM_dd_HHmmss";

    public static void WriteNewMidiFile(MidiFile midiFile, Type instrumentType)
    {
        var instrumentName = instrumentType.Name;

        if (!Directory.Exists(ClipsDirectoryName))
            Directory.CreateDirectory(ClipsDirectoryName);

        var subdir = Path.Combine(ClipsDirectoryName, instrumentName);
        if (!Directory.Exists(subdir))
            Directory.CreateDirectory(subdir);

        var fileName = string.Format(k_ClipNameFormat, instrumentName, DateTime.Now.ToString(k_ClipDateTimeFormat));
        midiFile.Write(Path.Combine(subdir, fileName));
    }

    public static void GetMidiFiles(ICollection<MidiFile> midiFiles, ICollection<string> midiFileNames, Type instrumentType)
    {
        midiFiles.Clear();
        midiFileNames.Clear();
        var instrumentName = instrumentType.Name;

        var subdir = Path.Combine(ClipsDirectoryName, instrumentName);
        if (!Directory.Exists(subdir))
            return;

        foreach (var filePath in Directory.EnumerateFiles(subdir))
        {
            if (Path.GetExtension(filePath).Equals(k_MidiExtension))
            {
                midiFiles.Add(MidiFile.Read(filePath));
                midiFileNames.Add(Path.GetFileNameWithoutExtension(filePath));
            }
        }
    }
}
