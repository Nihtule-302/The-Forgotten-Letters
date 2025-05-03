using System.Collections.Generic;
using _Project.Scripts.Core.DataTypes;

public interface ILetterSelectionStrategy
{
    LetterData SelectLetter(List<LetterData> availableLetters);
}
