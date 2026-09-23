namespace QuranCode.Core.Content;

/// <summary>A translation, transliteration or other text kept verse by verse beside the Arabic.</summary>
/// <param name="Key">Stable name, for example <c>submission.en</c>.</param>
/// <param name="Language">BCP 47 tag: en, fa, ar, en-Latn.</param>
/// <param name="Kind">translation, transliteration, or emlaaei (the Arabic in standard spelling).</param>
/// <param name="RightToLeft">Whether the text reads right to left.</param>
/// <param name="Source">Where it is kept: 0 for the content database, 1 and up for translation packs.</param>
public sealed record TranslationInfo(
    int Id, string Key, string Language, string Name, string Translator, string Kind, bool RightToLeft);
