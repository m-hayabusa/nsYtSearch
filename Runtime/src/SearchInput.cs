
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;

public class SearchInput : UdonSharpBehaviour
{
    private InputField src;
    [SerializeField] private InputField dest;
    void Start()
    {
        src = GetComponent<InputField>();
        dest = transform.parent.Find("SearchInputResult").GetComponent<InputField>();
    }
    public void OnEndEdit()
    {
        dest.text = "https://www.youtube.com/results?search_query=" + src.text.Replace(" ", "+").Replace("?", "%3F").Replace("&", "%26");
        dest.ActivateInputField();
    }
}
