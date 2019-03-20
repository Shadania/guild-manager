using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

// the little info card displayed in the guild desk's recruit info menu
public class RecruitCardBehaviour : MonoBehaviour
{
    public Button RecruitButton;
    public Button RefuseButton;
    public Guild ApplyingGuild;
    public Text NameText;
    public UnityEvent GotRecruited = new UnityEvent();
    
	void Start ()
    {
        RecruitButton.onClick.AddListener(RecruitButtonClicked);
        RefuseButton.onClick.AddListener(RefuseButtonClicked);
	}
	
    void RecruitButtonClicked()
    {
        ApplyingGuild.AddMember(gameObject); // Guild handles it from here
        GotRecruited.Invoke();
        Destroy(gameObject);
    }
    void RefuseButtonClicked()
    {
        Destroy(gameObject);
    }
}
