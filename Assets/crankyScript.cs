using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using Random = UnityEngine.Random;

public class crankyScript : MonoBehaviour
{
	
	static int ModuleIdCounter = 1;
	int ModuleId;

	public KMBossModule Boss;
	public KMBombInfo Info;
	private string[] ignoredModules;
	
	public GameObject[] states;
	public SpriteRenderer face;
	public Sprite[] emotions;
	public KMAudio Audio;
	public AudioClip[] clips;
	
	public GameObject statusLight;
	
	//module
	private int solved = 0;
    private int solvables = -1;
    private bool solvedBusy = false;
    private int[] all, barelies, misses;
    private int currentState = 0;
    
	//mainState
	public MeshRenderer progressBar;
	public TextMesh accuracy;
	public TextMesh bareliesMisses;
	public TextMesh sectionName;

	//submitState
	public TextMesh submittion;
	public KMSelectable[] buttons;
	private string answer;
	private bool failsafeActivated;

	//failsafeState
	public TextMesh pageCounter;
	public KMSelectable[] pageScrollers;
	public TextMesh[] listIds;
	public TextMesh[] listPercentages;
	public TextMesh[] listBareliesMisses;
	private int page = 0;


	void setActiveState(int newState)
	{
		states[0].SetActive(false);
		states[1].SetActive(false);
		states[2].SetActive(false);
		currentState = newState;
		states[currentState].SetActive(true);
	}

	void refreshPage()
	{
		pageCounter.text = "Page " + (page+1) + "/" + Mathf.CeilToInt(solvables / 8f);
		for (int i = 8 * page; i < 8 * (page + 1); i++)
		{
			if (i < solvables)
			{
				listIds[i%8].text = i + 1 + ".";
				listPercentages[i%8].text = percentage(all[i], misses[i], barelies[i]);
				listBareliesMisses[i % 8].text = bareliesMissesText(barelies[i], misses[i]);
			}
			else
			{
				listIds[i%8].text = "";
				listPercentages[i%8].text = "";
				listBareliesMisses[i % 8].text = "";
			}
		}
	}

	void checkSolution()
	{
		if (submittion.text + "%" == answer)
		{
			Audio.HandlePlaySoundAtTransform(clips[2].name, transform);
			face.sprite = emotions[1];
			GetComponent<KMBombModule>().HandlePass();
			accuracy.text = answer;
			bareliesMisses.text = bareliesMissesText(barelies.Sum(),misses.Sum());
			sectionName.text = "Section Name:\nSolved! :D";
			progressBar.transform.localScale = new Vector3(0f, 1f, 1f);
			setActiveState(0);
		}
		else
		{
			GetComponent<KMBombModule>().HandleStrike();
			submittion.text = "";
			failsafeActivated = true;
		}
	}
	
	void updateStage()
	{
		if (solved == solvables) {transferToSubmitState(); return;}
		sectionName.text = "Section Name:\n" + solved + "/" + solvables;
		progressBar.transform.localScale = new Vector3(1f-(float)solved/solvables, 1f, 1f);
		progressBar.transform.localPosition = new Vector3((float)solved/solvables/2, 0f, 0f);
		accuracy.text = percentage(all[solved], misses[solved], barelies[solved]);
		bareliesMisses.text = bareliesMissesText(barelies[solved], misses[solved]);
	}
	void transferToSubmitState()
	{
		setActiveState(1);
		solvedBusy = true;
	}
	void initBoss()
	{
		if (ignoredModules == null) ignoredModules = Boss.GetIgnoredModules("Cranky's Sections", new[]
            {
                "14",
                "42",
                "501",
                "A>N<D",
                "AMM-041-292",
                "Bamboozling Time Keeper",
                "Black Arrows",
                "Brainf---",
                "The Board Walk",
                "Busy Beaver",
                "Cranky's Sections",
                "Don't Touch Anything",
                "Floor Lights",
                "Forget Any Color",
                "Forget Enigma",
                "Forget Ligma",
                "Forget Everything",
                "Forget Infinity",
                "Forget It Not",
                "Forget Maze Not",
                "Forget Me Later",
                "Forget Me Not",
                "Forget Perspective",
                "Forget The Colors",
                "Forget Them All",
                "Forget This",
                "Forget Us Not",
                "Iconic",
                "Keypad Directionality",
                "Kugelblitz",
                "Multitask",
                "OmegaDestroyer",
                "OmegaForest",
                "Organization",
                "Password Destroyer",
                "Purgatory",
                "Reporting Anomalies",
                "RPS Judging",
                "Security Council",
                "Shoddy Chess",
                "Simon Forgets",
                "Simon's Stages",
                "Souvenir",
                "Speech Jammer",
                "Tallordered Keys",
                "The Time Keeper",
                "Timing is Everything",
                "The Troll",
                "Turn The Key",
                "The Twin",
                "Übermodule",
                "Ultimate Custom Night",
                "The Very Annoying Button",
                "WAR",
                "Whiteout"
            });
        if (!ignoredModules.Contains("Cranky's Sections")) ignoredModules = ignoredModules.ToList().Concat(new List<string> { "Cranky's Sections" }).ToArray();

        GetComponent<KMBombModule>().OnActivate += delegate
        {
	        solvables = Info.GetSolvableModuleNames().Count(x => !ignoredModules.Contains(x));
	        init();
	        statusLight.SetActive(false);
        };
	}
	string percentage(int all, int misses, int barelies)
	{
		float percentage = (all - misses - .25f * barelies) / all;
		percentage = (float)Math.Floor(percentage * 10000) / 100;
		return percentage.ToString("F2")+"%";
	}
	string bareliesMissesText(int barelies, int misses)
    	{
    		string ans = "";
    		if (barelies > 0)
    		{
    			if (barelies % 10 == 1 && (barelies > 20 || barelies < 10)) ans += barelies.ToString() + " barely";
    			else ans += barelies.ToString() + " barelies";
    		}
    
    		if (misses > 0)
    		{
    			if (ans != "") ans += "\n";
    			if (misses % 10 == 1 && (misses > 20 || misses < 10)) ans += misses.ToString() + " miss";
    			else ans += misses.ToString() + " misses";
    		}
    		return ans;
    	}
	void init()
	{
		face.sprite = emotions[0];
		all = new int[solvables];
		barelies = new int[solvables];
		misses = new int[solvables];
		for (int i = 0; i < solvables; i++)
		{
			all[i] = Random.Range(200, 500);
			barelies[i] = (int)(Math.Pow(Random.value, 2) * 50);
			misses[i] = (int)(Math.Pow(Random.value, 4) * 50);
			if (barelies[i] + misses[i] == 0) all[i] = 250;
			Debug.LogFormat("[Cranky's Sections #{0}] Section {1}: {2} / {3} @ {4}. Percentage: {5}", ModuleId, i,barelies[i],misses[i],all[i],percentage(all[i], misses[i], barelies[i]));
		}
		answer = solvables==0?"100.00%":percentage(all.Sum(), misses.Sum(), barelies.Sum());
		Debug.LogFormat("[Cranky's Sections #{0}] Answer: {1}", ModuleId, answer);
		updateStage();
		initSubmission();
		pageScrollers[0].OnInteract += delegate
		{
			Audio.HandlePlaySoundAtTransform(clips[1].name, transform);
			if (page > 0)
			{
				page--;
				refreshPage();
			}
			return false;
		};
		pageScrollers[1].OnInteract += delegate
		{
			Audio.HandlePlaySoundAtTransform(clips[1].name, transform);
			if (page < Mathf.CeilToInt(solvables /8f)-1)
			{
				page++;
				refreshPage();
			}
			return false;
		};
		pageScrollers[2].OnInteract += delegate
		{
			Audio.HandlePlaySoundAtTransform(clips[1].name, transform);
			setActiveState(1);
			return false;
		};
	}

	void initSubmission()
	{
		submittion.text = "";
		for (int i = 0; i < 11; i++)
		{
			int i1 = i;
			buttons[i1].OnInteract += delegate
			{
				Audio.HandlePlaySoundAtTransform(clips[1].name, transform);
				if (submittion.text.Length < 6) submittion.text += "1234567890."[i1].ToString();
				failsafeActivated = false;
				return false;
			};
		}
		buttons[11].OnInteract += delegate
		{
			Audio.HandlePlaySoundAtTransform(clips[1].name, transform);
			if (submittion.text.Length > 0) submittion.text = submittion.text.Substring(0, submittion.text.Length - 1);
			else if (failsafeActivated) transferToFailsafe();
			return false;
		};
		buttons[12].OnInteract += delegate
		{
			checkSolution();
			return false;
		};
	}

	void transferToFailsafe()
	{
		failsafeActivated = false;
		refreshPage();
		setActiveState(2);
	}
	
	void Start ()
	{
		setActiveState(0);
		initBoss();
	}

	IEnumerator wait()
	{
		yield return new WaitForSeconds(4f);
		solvedBusy = false;
	}
	void Awake() { ModuleId = ModuleIdCounter++; }
	void Update ()
	{
		if (solvables == -1 || solvedBusy) return;
		if (Info.GetSolvedModuleNames().Count(x => !ignoredModules.Contains(x)) > solved)
		{
			Audio.HandlePlaySoundAtTransform(clips[0].name, transform);
			solvedBusy = true;
			solved++;
			updateStage();
			StartCoroutine(wait());
		}
	}
	
	#pragma warning disable 414
	private readonly string TwitchHelpMessage = @"Use !{0} ###.##% to submit your answer. Use !{0} failsafe to enter Failsafe State. Use !{0} </>/ok to press left/right/center button in Failsafe State.";
	#pragma warning restore 414
	
	public IEnumerator ProcessTwitchCommand(string Command)
	{
		Command  = Command.ToUpperInvariant();
		if (Command == "FAILSAFE")
		{
			if (currentState == 1) buttons[11].OnInteract();
		}
		else if (Command == "<")
		{
			if (currentState == 2) pageScrollers[0].OnInteract();
		}
		else if (Command == ">"){if (currentState == 2) pageScrollers[1].OnInteract();}
		else if (Command == "OK"){if (currentState == 2) pageScrollers[2].OnInteract();}
		else if (!Regex.Match(Command, @"\d\d\d?\.\d\d%").Success || Command.Length > 7)
		{
			yield return "sendtochaterror Invalid command.";
			yield break;
		}
		else
		{
			if (currentState != 1) yield break;
			foreach (char c in Command)
			{
				yield return new WaitForSeconds(.3f);
				if (c == '%')
				{
					buttons[12].OnInteract();
					yield break;
				}

				buttons["1234567890.".IndexOf(c.ToString(), StringComparison.Ordinal)].OnInteract();
			}
		}

		yield return null;
	}

	public IEnumerator TwitchHandleForcedSolve()
	{
		yield return null;
		if (currentState == 2) pageScrollers[2].OnInteract();
		if (currentState == 0) transferToSubmitState();
		yield return ProcessTwitchCommand(answer);
	}
}
