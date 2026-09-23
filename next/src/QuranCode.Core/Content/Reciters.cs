namespace QuranCode.Core.Content;

/// <summary>A reciter whose verse-by-verse recordings everyayah.com serves (Features.txt #65).</summary>
/// <param name="Folder">The path under https://everyayah.com/data/, for example Alafasy_64kbps.</param>
/// <param name="Quality">Bit rate in kbps, as the catalog gives it.</param>
public sealed record Reciter(string Folder, string Language, string Name, string Quality);
