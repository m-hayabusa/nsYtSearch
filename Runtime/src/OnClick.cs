
using UdonSharp;
using UnityEngine;

namespace nekomimiStudio.ytScraper
{
    public class OnClick : UdonSharpBehaviour
    {
        private YoutubeScraper core;
        void Start(){
            core = transform.parent.GetComponent<YoutubeScraper>();
        }

        public override void Interact()
        {
            Debug.Log(gameObject.name);
            core.clickListener(int.Parse(gameObject.name));
        }
    }
}