using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager _instance;
    private void Awake()
    {
        _instance = this;
    }
    //通过名字播放音效
    private static void Play(string soundName)
    {
        AudioSource.PlayClipAtPoint(GetSoundClip(soundName), Vector3.zero); 
    }
    //获取音效名字
    public static AudioClip GetSoundClip(string soundName)
    {
        return Resources.Load("Sound/" + soundName, typeof(AudioClip)) as AudioClip;
    }
    public static void PlayBallEnter() { Play("BallEnter"); }
    public static void PlayBomb() { Play("Bomb"); }
    public static void PlayEliminate() { Play("Eliminate"); }
    public static void PlayFail() { Play("Fail"); }
    public static void PlayFastMove() { Play("FastMove"); }
    public static void PlayShoot() { Play("Shoot"); }
}
