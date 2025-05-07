using _Project.Scripts.Core.DataTypes;
using _Project.Scripts.Core.Managers;
using _Project.Scripts.Core.SaveSystem;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class DrawLetterGame : MonoBehaviour
{
    // // Start is called once before the first execution of Update after the MonoBehaviour is created
    // void Start()
    // {
        
    // }

    // // Update is called once per frame
    // void Update()
    // {
        
    // }

    // public void StartGame(LetterData letterData)
    // {
        
    // }

    //  public void CheckAnswer(WordData word, ChoiceButtonController button)
    //     {
    //         if (targetLetterData.words.Contains(word))
    //         {
    //             button.updateDissolveColors(Color.green, Color.green);
    //             HandleCorrectChoiceAsync(word).Forget();
    //         }
    //         else
    //         {
    //             button.updateDissolveColors(Color.red, Color.red);
    //             HandleIncorrectChoiceAsync(word).Forget();
    //         }
    //     }

    //     private async UniTask HandleCorrectChoiceAsync(WordData word)
    //     {
    //         var dataBuilder = PersistentSOManager.GetSO<LetterHuntData>().GetBuilder();

    //         dataBuilder
    //             .IncrementCorrectScore()
    //             .AddRound(targetLetter, word.arabicWord, isCorrect: true);

    //         PersistentSOManager.GetSO<LetterHuntData>().UpdateData(dataBuilder);
    //         await FirebaseManager.Instance.SaveLetterHuntData(PersistentSOManager.GetSO<LetterHuntData>());

    //         PersistentSOManager.GetSO<PlayerAbilityStats>().AddEnergyPoint();

    //         Debug.Log("✅ " + ArabicSupport.Fix("صحيح!", true, true));
    //         await UniTask.Delay(System.TimeSpan.FromSeconds(correctChoiceDelay));
    //         NextLevel();
    //     }

    //     private async UniTaskVoid HandleIncorrectChoiceAsync(WordData word)
    //     {
    //         var dataBuilder = PersistentSOManager.GetSO<LetterHuntData>().GetBuilder();

    //         dataBuilder
    //             .IncrementIncorrectScore()
    //             .AddRound(targetLetter, word.arabicWord, isCorrect: false);

    //         PersistentSOManager.GetSO<LetterHuntData>().UpdateData(dataBuilder);
    //         await FirebaseManager.Instance.SaveLetterHuntData(PersistentSOManager.GetSO<LetterHuntData>());

    //         Debug.Log("❌ " + ArabicSupport.Fix("خطأ! حاول مرة أخرى.", true, true));
    //         await UniTask.Delay(0);
    //     }
}
