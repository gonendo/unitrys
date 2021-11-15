using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace unitrys{
    public class SoundManager : MonoBehaviour
    {
        private Dictionary<string, AudioSource> _audioSources;

        void Awake()
        {
            _audioSources = new Dictionary<string, AudioSource>();
            AudioSource[] sources = GetComponents<AudioSource>();
            for(int i=0; i < sources.Length; i++){
                _audioSources.Add(sources[i].clip.name, sources[i]);
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            
        }

        public void PlaySound(string soundId)
        {
            AudioSource source;
            if(_audioSources.TryGetValue(soundId, out source)){
                source.PlayOneShot(source.clip);
            }
        }

        public void StopAllSounds(){
            foreach(KeyValuePair<string,AudioSource> values in _audioSources){
                if(values.Value.isPlaying){
                    values.Value.Stop();
                }
            }
        }

        public bool IsPlaying(string soundId){
            AudioSource source;
            if(_audioSources.TryGetValue(soundId, out source)){
                return source.isPlaying;
            }
            return false;
        }
    }
}