using System.Collections.Generic;
using UnityEngine;
using UnityUtils.Extensions;

namespace Data
{
    public class PlayList
    {
        public Queue<AudioClip> clips;
        public bool shuffle;
        
        private int _hitsShuffleRemain;
        
        public PlayList(Queue<AudioClip> list, bool shuffle = true)
        {
            clips = list;
            _hitsShuffleRemain = list.Count;
            this.shuffle = shuffle;
        }
        
        public PlayList(IReadOnlyCollection<AudioClip> list, bool shuffle = true)
        {
            clips = new Queue<AudioClip>(list);
            _hitsShuffleRemain = list.Count;
            this.shuffle = shuffle;
        }
        
        private void TryShuffle()
        {
            _hitsShuffleRemain--;
            if (_hitsShuffleRemain > 1)
                clips = clips.Shuffle();
        }

        public AudioClip GetClip()
        {
            if (shuffle)
                TryShuffle();
            
            var clip = clips.Dequeue();
            clips.Enqueue(clip);
            return clip;
        }
    }
}