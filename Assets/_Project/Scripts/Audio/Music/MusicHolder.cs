using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.Audio.Music
{
    [CreateAssetMenu(fileName = "MusicHolder", menuName = "Scriptable Objects/MusicHolder")]
    public class MusicHolder : ScriptableObject
    {
        [Header("Day Music")]
        public List<AudioClip> dayMusicCollection = new();

        [Header("Night Music")]
        public List<AudioClip> nightMusicCollection = new();

        [Header("Battle Music")]
        public List<AudioClip> battleMusicCollection = new();
    }
}
