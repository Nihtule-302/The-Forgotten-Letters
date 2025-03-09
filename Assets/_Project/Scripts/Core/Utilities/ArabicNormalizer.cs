using System.Text;
using System.Text.RegularExpressions;

public static class ArabicNormalizer
{
    // Removes diacritics (Tashkeel) from Arabic text
    public static string RemoveTashkeel(string text)
    {
        return Regex.Replace(text, "[\u064B-\u0652]", ""); // Arabic diacritics range
    }

    public static string NormalizeArabicLetter(string letter)
    {

        var noTashkeelText = RemoveTashkeel(letter);

        StringBuilder sb = new StringBuilder(noTashkeelText);
        
        for (int i = 0; i < sb.Length; i++)
        {
            switch (sb[i])
            {
                case 'أ': case 'إ': case 'آ':
                    sb[i] = 'ا'; // Convert all forms of Alif to a simple Alif
                    break;
                case 'ة':
                    sb[i] = 'ه'; // Convert Taa Marbouta to Haa
                    break;
                case 'ى':
                    sb[i] = 'ي'; // Convert Alif Maqsura to Ya
                    break;
            }
        }

        return sb.ToString();
    }

    public static string NormalizeArabicWord(string word)
    {
        return word.Normalize(NormalizationForm.FormKD);
    }

    public static bool DoesWordContainsTargetLetter(string word, string targetLetter)
    {
        string normalizedWord = NormalizeArabicWord(word);
        string normalizedTarget = NormalizeArabicLetter(targetLetter);

        return normalizedWord.Contains(normalizedTarget);
    }
}
