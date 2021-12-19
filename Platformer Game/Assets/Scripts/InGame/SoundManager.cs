using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour {
    private AudioSource audioSource;

    public AudioClip background;
    public AudioClip hitSound;
    public AudioClip attackSound;

    // Start is called before the first frame update
    private void Start() {
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = background;

        audioSource.volume = 1.0f;
        audioSource.loop = true;
        audioSource.mute = false;

        audioSource.Play();
    }

    public void PlaySwordSwingSound() {
        audioSource.PlayOneShot(attackSound);
    }

    public void PlayHitSound() {
        audioSource.PlayOneShot(hitSound);
    }
}
