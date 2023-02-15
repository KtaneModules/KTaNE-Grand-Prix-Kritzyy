using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using KModkit;
using Random = UnityEngine.Random;
using System.Linq;
using KeepCoding;

public class GrandPrixScript : MonoBehaviour
{
	//
	// NOTE TO ANYONE WHO WANTS TO EDIT THIS FILE:
	// Grid standings are being calculated from the difference to the leader.
	// Example: driver 1 is P1 (first), driver 2 is 5 away from driver 1, driver 3 is 7 away from driver 1
	// Driver 1 is first, driver 2 is second, driver 3 is third.
	//

	public TextMesh DeltaTextMesh, DriversTextMesh, LapCountTextMesh, DriverSectorTextMesh;
	public TextMesh SectorInfo_LapTextMesh, SectorInfo_DriverTextMesh;
	public KMBombInfo BombInfo;
	public KMBombModule ThisModule;
	public KMBossModule BossModule;
	public KMAudio Sound;

	public KMSelectable UpButton, DownButton, SubmitFlag;
	public List<KMSelectable> AllNameSelectables;

	public List<Renderer> TeamRenders;
	public List<TextMesh> AllDriverNameTextMeshes;
	public Renderer FlagRender;
	public Renderer FlagDescriptionBox;

	public List<Texture> TeamTextures;
	public List<Texture> AllFlagTextures;
	public Texture ChequeredFlag;

	public GameObject DeltasGameObject;

	List<string> ListOfAllDriverAcros = new List<string>()
	{
		"HAM",
		"BOT",
		"VER",
		"PER",
		"NOR",
		"RIC",
		"RUS",
		"LAT",
		"SAI",
		"LEC",
		"MAZ",
		"MSC",
		"ALO",
		"OCO",
		"RAI",
		"GIO",
		"GAS",
		"TSU",
		"VET",
		"STR"
	};

	List<string> ListOfAllDriverLastNames = new List<string>()
    {
        "Hamilton",
        "Bottas",
        "Verstappen",
        "Perez",
        "Norris",
        "Ricciardo",
        "Russell",
        "Latifi",
        "Sainz",
        "Leclerc",
        "Mazepin",
        "Schumacher",
        "Alonso",
        "Ocon",
        "Raikkonen",
        "Giovinazzi",
        "Gasly",
        "Tsunoda",
        "Vettel",
        "Stroll"
    };


    List<string> Team_MERCEDES = new List<string>()
	{
		"HAM", "BOT"
	};
	List<string> Team_RB = new List<string>()
	{
		"VER", "PER"
	};
	List<string> Team_MCLAREN = new List<string>()
	{
		"NOR", "RIC"
	};
	List<string> Team_WILLIAMS = new List<string>()
	{
		"RUS", "LAT"
	};
	List<string> Team_FERRARI = new List<string>()
	{
		"SAI", "LEC"
	};
	List<string> Team_HAAS = new List<string>()
	{
		"MAZ", "MSC"
	};
	List<string> Team_ALPINE = new List<string>()
	{
		"ALO", "OCO"
	};
	List<string> Team_ROMEO = new List<string>()
	{
		"RAI", "GIO"
	};
	List<string> Team_TAURI = new List<string>()
	{
		"GAS", "TSU"
	};
	List<string> Team_ASTON = new List<string>()
	{
		"VET", "STR"
	};

	bool BlackFlagged;
	bool FinalLap;
	bool SelectedSomething;
	bool StrikeMode;

	int MaxLaps;
	int CurrentLap = 1;
	int LapCount = 0;
	int CurrentSolveCount = 0;
	int GreenFlagLap;
	int Eliminations = 0;
	int CurrentSectorNumber;
	int AvailibleDrivers = 20;
	public int[] DriverSectorNumbers = new int[20];
	int SelectedIndex_1, SelectedIndex_2;

	string SectorInfoDriver;
	string SelectedDriver_1, SelectedDriver_2;
	public string[] CurrentGridDrivers = new string[20];
	public string[] CurrentGridDrivers_FullLastName = new string[20];
	public string[] CurrentGridTeams = new string[20];
	public string[] StartingGridDrivers = new string[20];
	public string[] StartingGridTeams = new string[20];
	public string[] EndResultsList = new string[20];

	public float[] Deltas = new float[20];
	public float[] LeaderDifference = new float[20]; //For the difference of every driver relative to the leader

	public string[] IgnoredModules;
	public string[] DefaultListOfIgnores = new string[]
	{
		"14","+","Black Arrows","A>N<D","Brainf---","Busy Beaver","Concentration","Don't Touch Anything","Floor Lights","Forget Any Color","Forget Enigma","Forget Everything","Forget Infinity","Forget It Not","Forget Maze Not","Forget Me Later","Forget Me Not","Forget Morse Not","Forget Our Voices","Forget Perspective","Forget This","Forget Us Not","Forget The Colors","Forget Them All","Duck Konundrum","Gemory","Four-Card Monte","Iconic","ID Exchange","Cube Synchronization","Keypad Directionality","Kugelblitz","OmegaForget","Organization","Out of Time","Purgatory","RPS Judging","Security Council","Shoddy Chess","Simon Forgets","Simon's Stages","Soulscream","Soulsong","Souvenir","Tallordered Keys","Tetrahedron","The Board Walk","The Twin","The Very Annoying Button","The Troll","Twister","Ultimate Custom Night","Whiteout","Übermodule"
	};

	//History variables
	public List<string> History_FlagType;
	public List<string> History_SectorInfo;
	public List<string> History_DriverInfo;
	public List<string> History_DriverSectors;

	//ModuleID
	static int moduleIdCounter = 1;
	int ModuleID;

	void Awake()
	{
		ModuleID = moduleIdCounter++;
	}

	void Start ()
	{
		Sound.PlaySoundAtTransform("StartingLights", transform);

		DriversTextMesh.text = "";
		Deltas[0] = 0;
		string DeltaString = "Inter.\n";
		float DeltaGenerator;

		IgnoredModules = BossModule.GetIgnoredModules(ThisModule, DefaultListOfIgnores);

		//Generating starting deltas and grid
		for (int T = 0; T < 20; T++)
		{
			if (T < 19)
            {
				DeltaGenerator = Random.Range(50f, 5000) / 1000;
				DeltaGenerator = (float)Math.Round(DeltaGenerator, 3);

				Deltas[T + 1] = DeltaGenerator;

				//Make sure every value has 3 decimals on the module. This is only visual
				if (DeltaGenerator.ToString().Length == 1)
				{
					DeltaString += "+" + DeltaGenerator + ".000\n";
				}
				if (DeltaGenerator.ToString().Length == 3)
				{
					DeltaString += "+" + DeltaGenerator + "00\n";
				}
				else if (DeltaGenerator.ToString().Length == 4)
				{
					DeltaString += "+" + DeltaGenerator + "0\n";
				}
				else
				{
					DeltaString += "+" + DeltaGenerator + "\n";
				}

			}
			DeltaTextMesh.text = DeltaString;

			int DriverGenerator = Random.Range(0, ListOfAllDriverAcros.Count);
			CurrentGridDrivers[T] = ListOfAllDriverAcros[DriverGenerator];
			CurrentGridDrivers_FullLastName[T] = ListOfAllDriverLastNames[DriverGenerator];
			ListOfAllDriverAcros.Remove(CurrentGridDrivers[T]);
			ListOfAllDriverLastNames.Remove(CurrentGridDrivers_FullLastName[T]);
			AllDriverNameTextMeshes[T].text = CurrentGridDrivers[T];
		}

		for (int T = 0; T < 20; T++)
        {
			StartingGridDrivers[T] = CurrentGridDrivers[T];
        }

		Debug.LogFormat("[Grand Prix #{0}]: (Initial) The starting order is: {1}.", ModuleID, string.Join(", ", StartingGridDrivers));
		string[] DeltasArray = Array.ConvertAll(Deltas, x => x.ToString());
		Debug.LogFormat("[Grand Prix #{0}]: (Initial) The starting deltas are as follows: \n{1}.", ModuleID, string.Join("\n", DeltasArray));

		//Setting the leader differences
		int CurrentDriver_Leader = 0;
		foreach (int Delta in Deltas)
        {
			if (CurrentDriver_Leader == 0)
            {
				LeaderDifference[CurrentDriver_Leader] = 0;
            }
			else
			{
				LeaderDifference[CurrentDriver_Leader] = Deltas[CurrentDriver_Leader] + LeaderDifference[CurrentDriver_Leader - 1];
			}
			CurrentDriver_Leader++;
        }

		//Assigning their teams
		int CurrentDriver_Driver = 0;
		foreach (string Driver in CurrentGridDrivers)
        {
			if (Team_MERCEDES.Any(Driver.Contains))
            {
				CurrentGridTeams[CurrentDriver_Driver] = "Mercedes";
				TeamRenders[CurrentDriver_Driver].material.mainTexture = TeamTextures[0];
            }
			else if (Team_RB.Any(Driver.Contains))
			{
				CurrentGridTeams[CurrentDriver_Driver] = "Red Bull";
				TeamRenders[CurrentDriver_Driver].material.mainTexture = TeamTextures[1];
			}
			else if (Team_MCLAREN.Any(Driver.Contains))
			{
				CurrentGridTeams[CurrentDriver_Driver] = "McLaren";
				TeamRenders[CurrentDriver_Driver].material.mainTexture = TeamTextures[2];
			}
			else if (Team_WILLIAMS.Any(Driver.Contains))
			{
				CurrentGridTeams[CurrentDriver_Driver] = "Williams";
				TeamRenders[CurrentDriver_Driver].material.mainTexture = TeamTextures[3];
			}
			else if (Team_FERRARI.Any(Driver.Contains))
			{
				CurrentGridTeams[CurrentDriver_Driver] = "Ferrari";
				TeamRenders[CurrentDriver_Driver].material.mainTexture = TeamTextures[4];
			}
			else if (Team_HAAS.Any(Driver.Contains))
			{
				CurrentGridTeams[CurrentDriver_Driver] = "Haas";
				TeamRenders[CurrentDriver_Driver].material.mainTexture = TeamTextures[5];
			}
			else if (Team_ALPINE.Any(Driver.Contains))
			{
				CurrentGridTeams[CurrentDriver_Driver] = "Alpine";
				TeamRenders[CurrentDriver_Driver].material.mainTexture = TeamTextures[6];
			}
			else if (Team_ROMEO.Any(Driver.Contains))
			{
				CurrentGridTeams[CurrentDriver_Driver] = "Alfa Romeo";
				TeamRenders[CurrentDriver_Driver].material.mainTexture = TeamTextures[7];
			}
			else if (Team_TAURI.Any(Driver.Contains))
			{
				CurrentGridTeams[CurrentDriver_Driver] = "Alpha Tauri";
				TeamRenders[CurrentDriver_Driver].material.mainTexture = TeamTextures[8];
			}
			else if (Team_ASTON.Any(Driver.Contains))
			{
				CurrentGridTeams[CurrentDriver_Driver] = "Aston Martin";
				TeamRenders[CurrentDriver_Driver].material.mainTexture = TeamTextures[9];
			}
			CurrentDriver_Driver++;
        }

		ThisModule.OnActivate += delegate
		{
			//Determine maximum amount of laps
			foreach (string ModuleName in BombInfo.GetSolvableModuleNames())
			{
				if (ModuleName == "The Grand Prix")
				{
					//Ignore this module
				}
				else if (!IgnoredModules.Contains(ModuleName))
				{
					MaxLaps++;
				}
			}

			//IF THERE ARE NO NON-IGNORED MODULES, OR LESS THAN 4, AUTOSOLVE
			if (MaxLaps < 4)
			{
				ThisModule.HandlePass();
				Debug.LogFormat("[Grand Prix #{0}]: (Initial) There are not enough valid modules, autosolving...", ModuleID);
			}
			//Otherwise, the minimum is half the amount of normal modules
			else
			{
				LapCount = Random.Range(MaxLaps / 2, MaxLaps);
				Debug.LogFormat("[Grand Prix #{0}]: (Initial) Total lap count: {1}", ModuleID, LapCount);
			}

			//Generate Green flag lap
			GreenFlagLap = Random.Range(1, LapCount);
			Debug.LogFormat("[Grand Prix #{0}]: (Initial) The green flag lap is at lap {1}", ModuleID, GreenFlagLap);

			//Generate first lap info
			GenerateLapInfo();
			StartCoroutine(CheckForSolves());
		};

		foreach (KMSelectable DriverButton in AllNameSelectables)
        {
			DriverButton.OnInteract = Empty;
        }
		SubmitFlag.OnInteract = Empty;

		UpButton.OnInteract = UpButton_Press;
		DownButton.OnInteract = DownButton_Press;

		UpButton.gameObject.transform.localScale = new Vector3(0, 0, 0);
		DownButton.gameObject.transform.localScale = new Vector3(0, 0, 0);
	}


	void GenerateLapInfo()
    {
		LapCountTextMesh.text = CurrentLap + "/" + LapCount;
		int FlagGenerator = 0;
		int BlackOrWhite = -1;
		int SafetyCarGen = 0;
		string SectorString = "";
		SectorInfoDriver = "";

		//Generate driver sectors
		for (int T = 0; T < 20; T++)
        {
			int RandomSectorGenerator = Random.Range(0, 3) + 1;
			DriverSectorNumbers[T] = RandomSectorGenerator;

			SectorString += "S" + DriverSectorNumbers[T] + Environment.NewLine;
		}
		DriverSectorTextMesh.text = SectorString;
		History_DriverSectors.Add(SectorString);

		//Generate flag
		//first, check if this is the green flag lap
		if (CurrentLap == GreenFlagLap)
        {
			DeltasGameObject.SetActive(true);

			FlagRender.material.mainTexture = AllFlagTextures[6];
			FlagDescriptionBox.material.color = Color.green;
			SectorInfo_DriverTextMesh.color = Color.black;
			SectorInfo_LapTextMesh.color = Color.black;
		}
		else //If not, new flag
		{
			DeltasGameObject.SetActive(false);

			//To avoid too many drop-outs, limit it to two
			if (Eliminations > 2)
            {
				FlagGenerator = Random.Range(0, 4);
			}
			else
            {
				FlagGenerator = Random.Range(0, 5);
			}

			switch (FlagGenerator)
			{
				case 0: //Yellow flag
					{
						SafetyCarGen = Random.Range(0, 4);
						switch (SafetyCarGen)
                        {
							default: //No safety car
                                {
									FlagRender.material.mainTexture = AllFlagTextures[0];
									FlagDescriptionBox.material.color = Color.yellow;
									SectorInfo_DriverTextMesh.color = Color.black;
									SectorInfo_LapTextMesh.color = Color.black;
									break;
                                }
							case 3: //Safety car
                                {
									FlagRender.material.mainTexture = AllFlagTextures[7];
									FlagDescriptionBox.material.color = Color.yellow;
									SectorInfo_DriverTextMesh.color = Color.black;
									SectorInfo_LapTextMesh.color = Color.black;
									break;
                                }
                        }
						break;
					}
				case 1: //Blue flag
					{
						FlagRender.material.mainTexture = AllFlagTextures[1];
						FlagDescriptionBox.material.color = Color.blue;
						SectorInfo_DriverTextMesh.color = Color.black;
						SectorInfo_LapTextMesh.color = Color.black;
						break;
					}
				case 2: //Red flag
					{
						FlagRender.material.mainTexture = AllFlagTextures[2];
						FlagDescriptionBox.material.color = Color.red;
						SectorInfo_DriverTextMesh.color = Color.black;
						SectorInfo_LapTextMesh.color = Color.black;
						break;
					}
				case 3: //Penalty flag
					{
						FlagRender.material.mainTexture = AllFlagTextures[3];
						FlagDescriptionBox.material.color = Color.white;
						SectorInfo_DriverTextMesh.color = Color.black;
						SectorInfo_LapTextMesh.color = Color.black;
						break;
					}
				case 4: //Black or white flag
					{
						//lower odds of happening
						int RNG = Random.Range(0,3);
						switch (RNG)
						{
							case 0: //Will happen
                                {
									// Decide what type of flag. 1 = black, 0 = white
									Eliminations++;
                                    if (BlackFlagged) // ... Unless if a black flag was already called before. There can only be one black flag
                                    {
                                        BlackOrWhite = 0;
                                    }
                                    else
                                    {
                                        BlackOrWhite = Random.Range(0, 2);
                                    }

                                    switch (BlackOrWhite)
									{
										default: //White flag
											{
												FlagRender.material.mainTexture = AllFlagTextures[4];
												FlagDescriptionBox.material.color = Color.white;
												SectorInfo_DriverTextMesh.color = Color.black;
												SectorInfo_LapTextMesh.color = Color.black;
												break;
											}
										case 1: //Black flag
											{
												BlackFlagged = true;
												FlagRender.material.mainTexture = AllFlagTextures[5];
												FlagDescriptionBox.material.color = Color.black;
												SectorInfo_DriverTextMesh.color = Color.white;
												SectorInfo_LapTextMesh.color = Color.white;
												break;
											}
									}
									break;
                                }
							default: //Will NOT happen
                                {
									GenerateLapInfo();
									break;
                                }
                        }
						break;
					}
			}
		}

		//Generate sector info
		//If not, check if this is the green flag lap
		if (CurrentLap == GreenFlagLap)
		{
			Debug.LogFormat("[Grand Prix #{0}]: (Lap {1}) Arrived at green flag lap.", ModuleID, CurrentLap);
			SectorInfo_LapTextMesh.text = "Track clear";
			SectorInfo_DriverTextMesh.text = "";
			SectorInfo_LapTextMesh.gameObject.transform.localScale = new Vector3(0.001544802f, 0.006553107f, 0.1229215f);

			History_FlagType.Add("Green");
			History_DriverInfo.Add("");
			History_SectorInfo.Add("");
		}
		//Else, follow normal procedures
		else
		{
			SectorInfo_DriverTextMesh.text = "";

			CurrentSectorNumber = Random.Range(1, 4);
			SectorInfo_LapTextMesh.text = "Sector " + CurrentSectorNumber;
			SectorInfo_LapTextMesh.gameObject.transform.localScale = new Vector3(0.002060054f, 0.006553107f, 0.1229215f);

			History_SectorInfo.Add(CurrentSectorNumber.ToString());

			UpButton.gameObject.transform.localScale = new Vector3(0, 0, 0);
			DownButton.gameObject.transform.localScale = new Vector3(0, 0, 0);

			switch (FlagGenerator)
			{
				case 0: //Yellow flag
					{
						if (SafetyCarGen != 3)
                        {
							History_FlagType.Add("Yellow");
							History_DriverInfo.Add("");

							HandleSectorInfo("Yellow");
						}
						else
                        {
							History_FlagType.Add("YellowSC");
							History_DriverInfo.Add("");

							HandleSectorInfo("YellowSC");
						}
						break;
					}
				case 1: //Blue flag
					{
						int BackmarkerGenerator = Random.Range(0, AvailibleDrivers);
						SectorInfoDriver = CurrentGridDrivers[BackmarkerGenerator];
						SectorInfo_DriverTextMesh.text = "- " + SectorInfoDriver;

						History_FlagType.Add("Blue");
						History_DriverInfo.Add("- " + SectorInfoDriver);

						HandleSectorInfo("Blue");
						break;
					}
				case 2: //Red flag
					{
						History_FlagType.Add("Red");

						if (Eliminations > 2) //Limit eliminations
						{
							HandleSectorInfo("Red", "NoDrop");
							History_DriverInfo.Add("");
						}
						else
						{
							int DropoutChance = Random.Range(0, 5);
							switch (DropoutChance)
							{
								default: //No dropouts
									{
										HandleSectorInfo("Red", "NoDrop");
										History_DriverInfo.Add("");
										break;
									}
								case 4: //Dropout
									{
										int EliminatedDriverGenerator = Random.Range(0, AvailibleDrivers);
										SectorInfoDriver = CurrentGridDrivers[EliminatedDriverGenerator];
										SectorInfo_DriverTextMesh.text = "- " + SectorInfoDriver;

										History_DriverInfo.Add("- " + SectorInfoDriver);

										HandleSectorInfo("Red", CurrentGridDrivers[EliminatedDriverGenerator]);
										break;
									}
							}

						}
						break;
					}
				case 3: //Penalty flag
					{
						int PenaltyGenerator = Random.Range(0, AvailibleDrivers);
						SectorInfoDriver = CurrentGridDrivers[PenaltyGenerator];
						SectorInfo_DriverTextMesh.text = "- " + SectorInfoDriver;

						History_FlagType.Add("Penalty");
						History_DriverInfo.Add("- " + SectorInfoDriver);

						HandleSectorInfo("Penalty", CurrentGridDrivers[PenaltyGenerator]);
						break;
					}
				case 4: //Black or white flag
					{
						switch (BlackOrWhite)
                        {
							default: //White flag
                                {
									int EliminatedDriverGenerator = Random.Range(0, AvailibleDrivers);
									SectorInfoDriver = CurrentGridDrivers[EliminatedDriverGenerator];
									SectorInfo_DriverTextMesh.text = "- " + SectorInfoDriver;

									History_FlagType.Add("White");
									History_DriverInfo.Add("- " + SectorInfoDriver);

									HandleSectorInfo("White", CurrentGridDrivers[EliminatedDriverGenerator]);
									break;
                                }
							case 1: //Black flag
                                {
									int EliminatedDriverGenerator = Random.Range(0, AvailibleDrivers);
									SectorInfoDriver = CurrentGridDrivers[EliminatedDriverGenerator];
									SectorInfo_DriverTextMesh.text = "- " + SectorInfoDriver;

									History_FlagType.Add("Black");
									History_DriverInfo.Add("- " + SectorInfoDriver);

									HandleSectorInfo("Black", CurrentGridDrivers[EliminatedDriverGenerator]);
									break;
								}
                        }
						break;
					}
			}
		}

	}

	void HandleSectorInfo(string FlagType, string Eliminated = "")
    {
		//The fun part: Change deltas according to flag/sector info
		int CurrentDriver_Sector = 0;

		//This stuff is for logging later on:
		List<string> LOG_DriversInCurrentSector = new List<string>();
		string PerformedAction = "";

		switch (FlagType)
        {
			case "Yellow": //Yellow flag
                {
					foreach (int Sector in DriverSectorNumbers)
                    {
						if (Sector == CurrentSectorNumber)
                        {
							LeaderDifference[CurrentDriver_Sector] += 1;
							LOG_DriversInCurrentSector.Add(CurrentGridDrivers[CurrentDriver_Sector]);
						}
						CurrentDriver_Sector++;
                    }
					PerformedAction = "Drivers in sector " + CurrentSectorNumber + " have their delta increased by 1";
					break;
                }
			case "YellowSC":
				{
					PerformedAction = "A safety car was deployed. Everyone's delta is reset to 2.";
					int DifferenceToAssign = 0;
					for (int i = 0; i < LeaderDifference.Length; i++)
					{
						if (LeaderDifference[i] <= 100)
						{
							DifferenceToAssign += 2;
							LeaderDifference[i] = DifferenceToAssign;
						}
					}

					int RandomSectorGenerator = Random.Range(1, 4);
					DriverSectorNumbers[0] = RandomSectorGenerator;
					DriverSectorNumbers[1] = RandomSectorGenerator;
					DriverSectorNumbers[2] = RandomSectorGenerator;
					DriverSectorNumbers[3] = RandomSectorGenerator;
					DriverSectorNumbers[4] = RandomSectorGenerator;
					DriverSectorNumbers[5] = RandomSectorGenerator;
					DriverSectorNumbers[6] = RandomSectorGenerator;
					DriverSectorNumbers[7] = RandomSectorGenerator;
					DriverSectorNumbers[8] = RandomSectorGenerator;
					DriverSectorNumbers[9] = RandomSectorGenerator;
					DriverSectorNumbers[10] = RandomSectorGenerator;
					DriverSectorNumbers[11] = RandomSectorGenerator;
					DriverSectorNumbers[12] = RandomSectorGenerator;
					DriverSectorNumbers[13] = RandomSectorGenerator;
					DriverSectorNumbers[14] = RandomSectorGenerator;
					DriverSectorNumbers[15] = RandomSectorGenerator;
					DriverSectorNumbers[16] = RandomSectorGenerator;
					DriverSectorNumbers[17] = RandomSectorGenerator;
					DriverSectorNumbers[18] = RandomSectorGenerator;
					DriverSectorNumbers[19] = RandomSectorGenerator;

					string NewSectorString = "";
					foreach (int sector in DriverSectorNumbers)
                    {
						NewSectorString += "S" + sector + Environment.NewLine;
                    }
					DriverSectorTextMesh.text = NewSectorString;


					break;
				}
			case "Blue":
				{
					int index = StartingGridDrivers.IndexOf(SectorInfoDriver);
					if (index <= 3 && DriverSectorNumbers[index] == 1)
                    {
						PerformedAction = "The associated driver started top 3 and is now in sector 1. His delta increases by 1 second.";
						LeaderDifference[index] -= 0.5f;
					}
					else if (CurrentLap > 5)
                    {
						PerformedAction = "5 laps have passed, so the associated driver's delta increases by 1 second.";
						LeaderDifference[index] += 1;
					}
					else
                    {
						PerformedAction = "Blue flags are out, so the associated driver's delta increases by 0.75 seconds.";
						LeaderDifference[index] += 0.75f;
					}
					break;
				}
			case "Red":
				{
					if (Eliminated != "NoDrop")
					{
						PerformedAction = Eliminated + " is out of the race. ";
						int Difference = 0;
						for (int i = 0; i < LeaderDifference.Length; i++)
						{
							if (LeaderDifference[i] <= 89999999)
							{
								Difference++;
								LeaderDifference[i] = Difference;
							}
						}
						int Place = 0;
						foreach (string Driver in CurrentGridDrivers)
						{
							if (Driver != Eliminated)
							{
								Place++;
							}
							else
							{
								//Stop checking. Found the position of the dropout.
								break;
							}
						}
						LeaderDifference[Place] = 90000000; //9000000X are dropouts.
					}

					int DifferenceToAssign = 0;
					if (BombInfo.IsIndicatorOn(Indicator.BOB))
                    {
						// Logging
						PerformedAction += "Lit BOB is present, so deltas are reset to 2";
						for (int i = 0; i < LeaderDifference.Length; i++)
						{
							if (LeaderDifference[i] <= 100)
							{
								DifferenceToAssign += 2;
								LeaderDifference[i] = DifferenceToAssign;
							}
						}
					}
					else if (LapCount >= BombInfo.GetBatteryCount())
                    {
						PerformedAction += "Lap count is equal or greater than battery count, so deltas are decreased by 5 for first 10 drivers";
						for (int i = 9; i >= 0; i--)
						{
							if (LeaderDifference[i] <= 100)
							{
								// Since the times of all the drivers are being lowered,
								// we also have to keep the decrease from each prior driver in mind
								// while decreasing the current driver.
								DifferenceToAssign += 5;
								LeaderDifference[i] += DifferenceToAssign;
							}
						}
					}
					else if (BombInfo.GetSerialNumberNumbers().First() == CurrentSectorNumber || BombInfo.GetSerialNumberNumbers().Last() == CurrentSectorNumber)
					{
                        PerformedAction += "Serial Number matches sector, so un even driver decreases delta by 2";
                        bool Even = false;
						for (int i = 0; i < LeaderDifference.Length; i++)
						{
							if (!Even)
                            {
								if (LeaderDifference[i] <= 100)
								{
									DifferenceToAssign += 2;
									LeaderDifference[i] = DifferenceToAssign;
								}
							}
							Even = !Even;
						}
					}
					else
					{
                        PerformedAction += "None apply. Restarting as usual";
                    }

					break;
				}
			case "Penalty":
				{
					int Place = 0;
					foreach (string Driver in CurrentGridDrivers)
					{
						if (Driver != Eliminated)
						{
							Place++;
						}
						else
						{
							//Stop checking. Found the position of the penalty receiver.
							break;
						}
					}

                    List<string> PenaltyTeamNames = new List<string>()
					{
						"Alfa Romeo",
						"Williams",
						"Haas"
					};

					// Gain info on serial letters and receiver of the penalty, for more clear reading
					string SerialNumberLetters = "";
					foreach (char letter in BombInfo.GetSerialNumberLetters())
					{
						SerialNumberLetters += letter;
					}
                    string Receiver = CurrentGridDrivers_FullLastName[Place];
                    Debug.LogFormat("(Grand Prix #{0}): (SYSTEM) Penalty receiver's full last name: {1}", ModuleID, Receiver);

                    if (PenaltyTeamNames.Contains(CurrentGridDrivers[Place]) && Place >= 15)
					{
                        Debug.LogFormat("(Grand Prix #{0}): (SYSTEM) Receiver is part of Williams, Haas or Alfa, and is lower than 14th", ModuleID);
                        LeaderDifference[Place] += 3;
						PerformedAction = SectorInfoDriver + " has received a 3 second penalty.";
					}
					else if (CurrentGridDrivers[Place] == History_DriverInfo[CurrentLap - 1])
					{
                        Debug.LogFormat("(Grand Prix #{0}): (SYSTEM) Receiver was shown a flag prior in the previous lap", ModuleID);
                        LeaderDifference[Place] += 10;
						PerformedAction = SectorInfoDriver + " has received a 10 second penalty.";
					}
					else if (DriverSectorNumbers[Place] == 1 && CurrentLap >= 3)
					{
                        Debug.LogFormat("(Grand Prix #{0}): (SYSTEM) receiver is in S1 on lap {1}", ModuleID, LapCount);
                        LeaderDifference[Place] += 5;
						PerformedAction = SectorInfoDriver + " has received a 5 second penalty.";
					}
					else if (CurrentLap == 1 && SerialNumberLetters.Any(CurrentGridTeams[Place].Contains))
					{
                        Debug.LogFormat("(Grand Prix #{0}): (SYSTEM) Lap 1, and serial matches team name", ModuleID);
                        LeaderDifference[Place] += 5;
						PerformedAction = SectorInfoDriver + " has received a 5 second penalty.";
					}
                    else if (CurrentLap == 1)
                    {
                        Debug.LogFormat("(Grand Prix #{0}): (SYSTEM) Lap 1, but no match", ModuleID);
                        LeaderDifference[Place] += 10;
						PerformedAction = SectorInfoDriver + " has received a 10 second penalty.";
					}
                    else if (SerialNumberLetters.ToLower().Any(Receiver.ToLower().Contains))
                    {
                        Debug.LogFormat("(Grand Prix #{0}): (SYSTEM) Serial contains letter of full last name", ModuleID);
                        LeaderDifference[Place] += Receiver.Length;
                        PerformedAction = SectorInfoDriver + " has received a time penalty of " + Receiver.Length + ".";
                    }
                    else
                    {
                        Debug.LogFormat("(Grand Prix #{0}): (SYSTEM) NONE APPLY", ModuleID);
                        LeaderDifference[Place] += 3;
                        PerformedAction = SectorInfoDriver + " has received a 3 second penalty.";
                    }
                    break;
				}
			case "White":
				{
					PerformedAction = Eliminated + " is out of the race.";
					int Place = 0;
					foreach (string Driver in CurrentGridDrivers)
					{
						if (Driver != Eliminated)
						{
							Place++;
						}
						else
						{
							//Stop checking. Found the position of the dropout.
							break;
						}
						Eliminations++;
					}
					LeaderDifference[Place] = 100 + Eliminations; //10X is for dropouts.
					break;
				}
			case "Black":
				{
					PerformedAction = Eliminated + " is DISQUALIFIED from the race.";
					int Place = 0;
					foreach (string Driver in CurrentGridDrivers)
					{
						if (Driver != Eliminated)
						{
							Place++;
						}
						else
						{
							//Stop checking. Found the position of the dropout.
							break;
						}
						Eliminations++;
					}
					LeaderDifference[Place] = 121; //121 is the spot for the one eliminated.
					break;
				}
		}

		while (LeaderDifference[0] > 0)
        {
			LeaderDifference[0] -= 1;
			LeaderDifference[1] -= 1;
			LeaderDifference[2] -= 1;
			LeaderDifference[3] -= 1;
			LeaderDifference[4] -= 1;
			LeaderDifference[5] -= 1;
			LeaderDifference[6] -= 1;
			LeaderDifference[7] -= 1;
			LeaderDifference[8] -= 1;
			LeaderDifference[9] -= 1;
			LeaderDifference[10] -= 1;
			LeaderDifference[11] -= 1;
			LeaderDifference[12] -= 1;
			LeaderDifference[13] -= 1;
			LeaderDifference[14] -= 1;
			LeaderDifference[15] -= 1;
			LeaderDifference[16] -= 1;
			LeaderDifference[17] -= 1;
			LeaderDifference[18] -= 1;
			LeaderDifference[19] -= 1;
		}
		while (LeaderDifference[0] < 0)
		{
			LeaderDifference[0] += 1;
			LeaderDifference[1] += 1;
			LeaderDifference[2] += 1;
			LeaderDifference[3] += 1;
			LeaderDifference[4] += 1;
			LeaderDifference[5] += 1;
			LeaderDifference[6] += 1;
			LeaderDifference[7] += 1;
			LeaderDifference[8] += 1;
			LeaderDifference[9] += 1;
			LeaderDifference[10] += 1;
			LeaderDifference[11] += 1;
			LeaderDifference[12] += 1;
			LeaderDifference[13] += 1;
			LeaderDifference[14] += 1;
			LeaderDifference[15] += 1;
			LeaderDifference[16] += 1;
			LeaderDifference[17] += 1;
			LeaderDifference[18] += 1;
			LeaderDifference[19] += 1;
		}

		Logging(FlagType, LOG_DriversInCurrentSector.ToArray(), PerformedAction);
	}

	void Logging(string FlagType, string[] LOG_DriversInCurrentSector, string PerformedAction)
    {
		Debug.LogFormat("[Grand Prix #{0}]: (Lap {1}) Lap info:", ModuleID, CurrentLap);
		if (FlagType == "YellowSC")
        {
			Debug.LogFormat("[Grand Prix #{0}]: (Lap {1}) The flag shown this lap is Yellow (Safety Car)", ModuleID, CurrentLap);
		}
		else
        {
			Debug.LogFormat("[Grand Prix #{0}]: (Lap {1}) The flag shown this lap is {2}", ModuleID, CurrentLap, FlagType);
		}
		Debug.LogFormat("[Grand Prix #{0}]: (Lap {1}) The associated sector is sector {2}", ModuleID, CurrentLap, CurrentSectorNumber);
		if (SectorInfoDriver != "")
        {
			Debug.LogFormat("[Grand Prix #{0}]: (Lap {1}) The driver associated with the flag is {2}.", ModuleID, CurrentLap, SectorInfoDriver);
		}
		else
        {
			Debug.LogFormat("[Grand Prix #{0}]: (Lap {1}) There is no driver associated with this flag.", ModuleID, CurrentLap);
		}


		//Now for the action performed...
		Debug.LogFormat("[Grand Prix #{0}]: (Lap {1}) {2}", ModuleID, CurrentLap, PerformedAction);
		if (FlagType == "Yellow")
		{
			Debug.LogFormat("[Grand Prix #{0}]: (Lap {1}) The drivers involved with this are: {2}", ModuleID, CurrentLap, string.Join(", ", LOG_DriversInCurrentSector));
		}

	}

	void HandleFinalLap()
    {
		Debug.LogFormat("[Grand Prix #{0}]: (Final lap) Reached the final lap.", ModuleID);
		FinalLap = true;

		AllNameSelectables[0].OnInteract = Name1_Press;
		AllNameSelectables[1].OnInteract = Name2_Press;
		AllNameSelectables[2].OnInteract = Name3_Press;
		AllNameSelectables[3].OnInteract = Name4_Press;
		AllNameSelectables[4].OnInteract = Name5_Press;
		AllNameSelectables[5].OnInteract = Name6_Press;
		AllNameSelectables[6].OnInteract = Name7_Press;
		AllNameSelectables[7].OnInteract = Name8_Press;
		AllNameSelectables[8].OnInteract = Name9_Press;
		AllNameSelectables[9].OnInteract = Name10_Press;
		AllNameSelectables[10].OnInteract = Name11_Press;
		AllNameSelectables[11].OnInteract = Name12_Press;
		AllNameSelectables[12].OnInteract = Name13_Press;
		AllNameSelectables[13].OnInteract = Name14_Press;
		AllNameSelectables[14].OnInteract = Name15_Press;
		AllNameSelectables[15].OnInteract = Name16_Press;
		AllNameSelectables[16].OnInteract = Name17_Press;
		AllNameSelectables[17].OnInteract = Name18_Press;
		AllNameSelectables[18].OnInteract = Name19_Press;
		AllNameSelectables[19].OnInteract = Name20_Press;

		SubmitFlag.OnInteract = HandleSubmit;

		LapCountTextMesh.text = "Final lap";
		SectorInfo_LapTextMesh.text = "Final lap";

		FlagDescriptionBox.material.color = Color.green;
		SectorInfo_DriverTextMesh.color = Color.black;
		SectorInfo_LapTextMesh.color = Color.black;

		SectorInfo_DriverTextMesh.gameObject.SetActive(false);
		DriverSectorTextMesh.gameObject.SetActive(false);
		DeltasGameObject.gameObject.SetActive(false);

		FlagRender.gameObject.SetActive(false);
		SubmitFlag.gameObject.transform.localPosition = new Vector3(0, -0.055f, -0.01f);
		SubmitFlag.gameObject.transform.localEulerAngles = new Vector3(0, 0, 180);
		SubmitFlag.gameObject.transform.localScale = new Vector3(0.8496743f, 0.2983174f, 0.8496742f);

		for (int T = 0; T < 20; T++)
        {
			int LowestValue = Array.IndexOf(LeaderDifference, LeaderDifference.Min());
			EndResultsList[T] = CurrentGridDrivers[LowestValue];
			LeaderDifference[LowestValue] = 100000000;
		}

		Debug.LogFormat("[Grand Prix #{0}]: (Final lap) The exptected result is {1}", ModuleID, string.Join(", ", EndResultsList));
	}

	IEnumerator CheckForSolves()
	{
		while (true)
        {
			if (BombInfo.GetSolvedModuleNames().Count() != CurrentSolveCount)
            {
				CurrentSolveCount = BombInfo.GetSolvedModuleNames().Count();
				CurrentLap++;
				if (!FinalLap)
                {
					if (CurrentLap == LapCount)
					{
						HandleFinalLap();
					}
					else
					{
						Debug.LogFormat("(Grand Prix #{0}): (SYSTEM) New solve detected", ModuleID, CurrentLap);
						int SolveSoundGenerator = Random.Range(0, 5);
						switch (SolveSoundGenerator)
                        {
							default:
                                {
									Sound.PlaySoundAtTransform("NewSolve_1", transform);
									break;
                                }
							case 1:
                                {
									Sound.PlaySoundAtTransform("NewSolve_2", transform);
									break;
                                }
							case 2:
                                {
									Sound.PlaySoundAtTransform("NewSolve_3", transform);
									break;
                                }
							case 3:
								{
									Sound.PlaySoundAtTransform("NewSolve_4", transform);
									break;
								}
							case 4:
								{
									Sound.PlaySoundAtTransform("NewSolve_5", transform);
									break;
								}
						}
						GenerateLapInfo();
					}
				}
				//New solve
			}
			yield return new WaitForSecondsRealtime(0.1f);
		}
	}

	//Buttons
	protected bool Empty()
	{
		return false;
    }
	protected bool Name1_Press()
	{
		GetComponent<KMSelectable>().AddInteractionPunch(0);
		GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
		Debug.LogFormat("(Grand Prix #{0}): (SYSTEM) Pressed button for name 1...", ModuleID, CurrentLap);
		if (!SelectedSomething)
        {
			SelectedSomething = true;
			SelectedIndex_1 = 0;
			SelectedDriver_1 = CurrentGridDrivers[0];
			AllDriverNameTextMeshes[0].color = Color.yellow;
			Debug.LogFormat("(Grand Prix #{0}): (SYSTEM) Selected driver {1}.", ModuleID, SelectedDriver_1);
		}
		else
        {
			if (SelectedIndex_1 == 0)
            {
				AllDriverNameTextMeshes[0].color = Color.white;
				Debug.LogFormat("(Grand Prix #{0}): (SYSTEM) Cannot swap a driver with itself!", ModuleID);
			}
			else
            {
				SelectedIndex_2 = 0;
				SelectedDriver_2 = CurrentGridDrivers[0];

				
				AllDriverNameTextMeshes[SelectedIndex_1].color = Color.white;
				AllDriverNameTextMeshes[SelectedIndex_2].color = Color.white;

				CurrentGridDrivers[SelectedIndex_1] = SelectedDriver_2;
				CurrentGridDrivers[SelectedIndex_2] = SelectedDriver_1;

				Debug.LogFormat("(Grand Prix #{0}): (SYSTEM) Swapped driver {1} with {2}", ModuleID, SelectedDriver_1, SelectedDriver_2);

				SelectedDriver_1 = "";
				SelectedDriver_2 = "";
				SelectedIndex_1 = 0;
				SelectedIndex_2 = 0;

				ResetLabels();
			}
			SelectedSomething = false;
		}
		return false;
	}
	protected bool Name2_Press()
	{
		GetComponent<KMSelectable>().AddInteractionPunch(0);
		GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
		Debug.LogFormat("(Grand Prix #{0}): (SYSTEM) Pressed button for name 2...", ModuleID, CurrentLap);
		if (!SelectedSomething)
		{
			SelectedSomething = true;
			SelectedIndex_1 = 1;
			SelectedDriver_1 = CurrentGridDrivers[1];
			AllDriverNameTextMeshes[1].color = Color.yellow;
			Debug.LogFormat("(Grand Prix #{0}): (SYSTEM) Selected driver {1}.", ModuleID, SelectedDriver_1);
		}
		else
		{
			if (SelectedIndex_1 == 1)
			{
				AllDriverNameTextMeshes[1].color = Color.white;
			}
			else
			{
				SelectedIndex_2 = 1;
				SelectedDriver_2 = CurrentGridDrivers[1];

				
				AllDriverNameTextMeshes[SelectedIndex_1].color = Color.white;
				AllDriverNameTextMeshes[SelectedIndex_2].color = Color.white;

				CurrentGridDrivers[SelectedIndex_1] = SelectedDriver_2;
				CurrentGridDrivers[SelectedIndex_2] = SelectedDriver_1;

				Debug.LogFormat("(Grand Prix #{0}): (SYSTEM) Swapped driver {1} with {2}", ModuleID, SelectedDriver_1, SelectedDriver_2);

				SelectedDriver_1 = "";
				SelectedDriver_2 = "";
				SelectedIndex_1 = 0;
				SelectedIndex_2 = 0;

				ResetLabels();
			}
			SelectedSomething = false;
		}
		return false;
	}
	protected bool Name3_Press()
	{
		GetComponent<KMSelectable>().AddInteractionPunch(0);
		GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
		Debug.LogFormat("(Grand Prix #{0}): (SYSTEM) Pressed button for name 3...", ModuleID, CurrentLap);
		if (!SelectedSomething)
		{
			SelectedSomething = true;
			SelectedIndex_1 = 2;
			SelectedDriver_1 = CurrentGridDrivers[2];
			AllDriverNameTextMeshes[2].color = Color.yellow;
			Debug.LogFormat("(Grand Prix #{0}): (SYSTEM) Selected driver {1}.", ModuleID, SelectedDriver_1);
		}
		else
		{
			if (SelectedIndex_1 == 2)
			{
				AllDriverNameTextMeshes[2].color = Color.white;
			}
			else
			{
				SelectedIndex_2 = 2;
				SelectedDriver_2 = CurrentGridDrivers[2];

				
				AllDriverNameTextMeshes[SelectedIndex_1].color = Color.white;
				AllDriverNameTextMeshes[SelectedIndex_2].color = Color.white;

				CurrentGridDrivers[SelectedIndex_1] = SelectedDriver_2;
				CurrentGridDrivers[SelectedIndex_2] = SelectedDriver_1;

				Debug.LogFormat("(Grand Prix #{0}): (SYSTEM) Swapped driver {1} with {2}", ModuleID, SelectedDriver_1, SelectedDriver_2);

				SelectedDriver_1 = "";
				SelectedDriver_2 = "";
				SelectedIndex_1 = 0;
				SelectedIndex_2 = 0;

				ResetLabels();
			}
			SelectedSomething = false;
		}
		return false;
	}
	protected bool Name4_Press()
	{
		GetComponent<KMSelectable>().AddInteractionPunch(0);
		GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
		Debug.LogFormat("(Grand Prix #{0}): (SYSTEM) Pressed button for name 4...", ModuleID, CurrentLap);
		if (!SelectedSomething)
		{
			SelectedSomething = true;
			SelectedIndex_1 = 3;
			SelectedDriver_1 = CurrentGridDrivers[3];
			AllDriverNameTextMeshes[3].color = Color.yellow;
			Debug.LogFormat("(Grand Prix #{0}): (SYSTEM) Selected driver {1}.", ModuleID, SelectedDriver_1);
		}
		else
		{
			if (SelectedIndex_1 == 3)
			{
				AllDriverNameTextMeshes[3].color = Color.white;
			}
			else
			{
				SelectedIndex_2 = 3;
				SelectedDriver_2 = CurrentGridDrivers[3];

				
				AllDriverNameTextMeshes[SelectedIndex_1].color = Color.white;
				AllDriverNameTextMeshes[SelectedIndex_2].color = Color.white;

				CurrentGridDrivers[SelectedIndex_1] = SelectedDriver_2;
				CurrentGridDrivers[SelectedIndex_2] = SelectedDriver_1;

				Debug.LogFormat("(Grand Prix #{0}): (SYSTEM) Swapped driver {1} with {2}", ModuleID, SelectedDriver_1, SelectedDriver_2);

				SelectedDriver_1 = "";
				SelectedDriver_2 = "";
				SelectedIndex_1 = 0;
				SelectedIndex_2 = 0;

				ResetLabels();
			}
			SelectedSomething = false;
		}
		return false;
	}
	protected bool Name5_Press()
	{
		GetComponent<KMSelectable>().AddInteractionPunch(0);
		GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
		Debug.LogFormat("(Grand Prix #{0}): (SYSTEM) Pressed button for name 5...", ModuleID, CurrentLap);
		if (!SelectedSomething)
		{
			SelectedSomething = true;
			SelectedIndex_1 = 4;
			SelectedDriver_1 = CurrentGridDrivers[4];
			AllDriverNameTextMeshes[4].color = Color.yellow;
			Debug.LogFormat("(Grand Prix #{0}): (SYSTEM) Selected driver {1}.", ModuleID, SelectedDriver_1);
		}
		else
		{
			if (SelectedIndex_1 == 4)
			{
				AllDriverNameTextMeshes[4].color = Color.white;
			}
			else
			{
				SelectedIndex_2 = 4;
				SelectedDriver_2 = CurrentGridDrivers[4];

				
				AllDriverNameTextMeshes[SelectedIndex_1].color = Color.white;
				AllDriverNameTextMeshes[SelectedIndex_2].color = Color.white;

				CurrentGridDrivers[SelectedIndex_1] = SelectedDriver_2;
				CurrentGridDrivers[SelectedIndex_2] = SelectedDriver_1;

				Debug.LogFormat("(Grand Prix #{0}): (SYSTEM) Swapped driver {1} with {2}", ModuleID, SelectedDriver_1, SelectedDriver_2);

				SelectedDriver_1 = "";
				SelectedDriver_2 = "";
				SelectedIndex_1 = 0;
				SelectedIndex_2 = 0;

				ResetLabels();
			}
			SelectedSomething = false;
		}
		return false;
	}
	protected bool Name6_Press()
	{
		GetComponent<KMSelectable>().AddInteractionPunch(0);
		GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
		Debug.LogFormat("(Grand Prix #{0}): (SYSTEM) Pressed button for name 6...", ModuleID, CurrentLap);
		if (!SelectedSomething)
		{
			SelectedSomething = true;
			SelectedIndex_1 = 5;
			SelectedDriver_1 = CurrentGridDrivers[5];
			AllDriverNameTextMeshes[5].color = Color.yellow;
			Debug.LogFormat("(Grand Prix #{0}): (SYSTEM) Selected driver {1}.", ModuleID, SelectedDriver_1);
		}
		else
		{
			if (SelectedIndex_1 == 5)
			{
				AllDriverNameTextMeshes[5].color = Color.white;
			}
			else
			{
				SelectedIndex_2 = 5;
				SelectedDriver_2 = CurrentGridDrivers[5];

				
				AllDriverNameTextMeshes[SelectedIndex_1].color = Color.white;
				AllDriverNameTextMeshes[SelectedIndex_2].color = Color.white;

				CurrentGridDrivers[SelectedIndex_1] = SelectedDriver_2;
				CurrentGridDrivers[SelectedIndex_2] = SelectedDriver_1;

				Debug.LogFormat("(Grand Prix #{0}): (SYSTEM) Swapped driver {1} with {2}", ModuleID, SelectedDriver_1, SelectedDriver_2);

				SelectedDriver_1 = "";
				SelectedDriver_2 = "";
				SelectedIndex_1 = 0;
				SelectedIndex_2 = 0;

				ResetLabels();
			}
			SelectedSomething = false;
		}
		return false;
	}
	protected bool Name7_Press()
	{
		GetComponent<KMSelectable>().AddInteractionPunch(0);
		GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
		Debug.LogFormat("(Grand Prix #{0}): (SYSTEM) Pressed button for name 7...", ModuleID, CurrentLap);
		if (!SelectedSomething)
		{
			SelectedSomething = true;
			SelectedIndex_1 = 6;
			SelectedDriver_1 = CurrentGridDrivers[6];
			AllDriverNameTextMeshes[6].color = Color.yellow;
			Debug.LogFormat("(Grand Prix #{0}): (SYSTEM) Selected driver {1}.", ModuleID, SelectedDriver_1);
		}
		else
		{
			if (SelectedIndex_1 == 6)
			{
				AllDriverNameTextMeshes[6].color = Color.white;
			}
			else
			{
				SelectedIndex_2 = 6;
				SelectedDriver_2 = CurrentGridDrivers[6];

				
				AllDriverNameTextMeshes[SelectedIndex_1].color = Color.white;
				AllDriverNameTextMeshes[SelectedIndex_2].color = Color.white;

				CurrentGridDrivers[SelectedIndex_1] = SelectedDriver_2;
				CurrentGridDrivers[SelectedIndex_2] = SelectedDriver_1;

				Debug.LogFormat("(Grand Prix #{0}): (SYSTEM) Swapped driver {1} with {2}", ModuleID, SelectedDriver_1, SelectedDriver_2);

				SelectedDriver_1 = "";
				SelectedDriver_2 = "";
				SelectedIndex_1 = 0;
				SelectedIndex_2 = 0;

				ResetLabels();
			}
			SelectedSomething = false;
		}
		return false;
	}
	protected bool Name8_Press()
	{
		GetComponent<KMSelectable>().AddInteractionPunch(0);
		GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
		Debug.LogFormat("(Grand Prix #{0}): (SYSTEM) Pressed button for name 8...", ModuleID, CurrentLap);
		if (!SelectedSomething)
		{
			SelectedSomething = true;
			SelectedIndex_1 = 7;
			SelectedDriver_1 = CurrentGridDrivers[7];
			AllDriverNameTextMeshes[7].color = Color.yellow;
			Debug.LogFormat("(Grand Prix #{0}): (SYSTEM) Selected driver {1}.", ModuleID, SelectedDriver_1);
		}
		else
		{
			if (SelectedIndex_1 == 7)
			{
				AllDriverNameTextMeshes[7].color = Color.white;
			}
			else
			{
				SelectedIndex_2 = 7;
				SelectedDriver_2 = CurrentGridDrivers[7];

				
				AllDriverNameTextMeshes[SelectedIndex_1].color = Color.white;
				AllDriverNameTextMeshes[SelectedIndex_2].color = Color.white;

				CurrentGridDrivers[SelectedIndex_1] = SelectedDriver_2;
				CurrentGridDrivers[SelectedIndex_2] = SelectedDriver_1;

				Debug.LogFormat("(Grand Prix #{0}): (SYSTEM) Swapped driver {1} with {2}", ModuleID, SelectedDriver_1, SelectedDriver_2);

				SelectedDriver_1 = "";
				SelectedDriver_2 = "";
				SelectedIndex_1 = 0;
				SelectedIndex_2 = 0;

				ResetLabels();
			}
			SelectedSomething = false;
		}
		return false;
	}
	protected bool Name9_Press()
	{
		GetComponent<KMSelectable>().AddInteractionPunch(0);
		GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
		Debug.LogFormat("(Grand Prix #{0}): (SYSTEM) Pressed button for name 9...", ModuleID, CurrentLap);
		if (!SelectedSomething)
		{
			SelectedSomething = true;
			SelectedIndex_1 = 8;
			SelectedDriver_1 = CurrentGridDrivers[8];
			AllDriverNameTextMeshes[8].color = Color.yellow;
			Debug.LogFormat("(Grand Prix #{0}): (SYSTEM) Selected driver {1}.", ModuleID, SelectedDriver_1);
		}
		else
		{
			if (SelectedIndex_1 == 8)
			{
				AllDriverNameTextMeshes[8].color = Color.white;
			}
			else
			{
				SelectedIndex_2 = 8;
				SelectedDriver_2 = CurrentGridDrivers[8];

				
				AllDriverNameTextMeshes[SelectedIndex_1].color = Color.white;
				AllDriverNameTextMeshes[SelectedIndex_2].color = Color.white;

				CurrentGridDrivers[SelectedIndex_1] = SelectedDriver_2;
				CurrentGridDrivers[SelectedIndex_2] = SelectedDriver_1;

				Debug.LogFormat("(Grand Prix #{0}): (SYSTEM) Swapped driver {1} with {2}", ModuleID, SelectedDriver_1, SelectedDriver_2);

				SelectedDriver_1 = "";
				SelectedDriver_2 = "";
				SelectedIndex_1 = 0;
				SelectedIndex_2 = 0;

				ResetLabels();
			}
			SelectedSomething = false;
		}
		return false;
	}
	protected bool Name10_Press()
	{
		GetComponent<KMSelectable>().AddInteractionPunch(0);
		GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
		Debug.LogFormat("(Grand Prix #{0}): (SYSTEM) Pressed button for name 10...", ModuleID, CurrentLap);
		if (!SelectedSomething)
		{
			SelectedSomething = true;
			SelectedIndex_1 = 9;
			SelectedDriver_1 = CurrentGridDrivers[9];
			AllDriverNameTextMeshes[9].color = Color.yellow;
			Debug.LogFormat("(Grand Prix #{0}): (SYSTEM) Selected driver {1}.", ModuleID, SelectedDriver_1);
		}
		else
		{
			if (SelectedIndex_1 == 9)
			{
				AllDriverNameTextMeshes[9].color = Color.white;
			}
			else
			{
				SelectedIndex_2 = 9;
				SelectedDriver_2 = CurrentGridDrivers[9];

				
				AllDriverNameTextMeshes[SelectedIndex_1].color = Color.white;
				AllDriverNameTextMeshes[SelectedIndex_2].color = Color.white;

				CurrentGridDrivers[SelectedIndex_1] = SelectedDriver_2;
				CurrentGridDrivers[SelectedIndex_2] = SelectedDriver_1;

				Debug.LogFormat("(Grand Prix #{0}): (SYSTEM) Swapped driver {1} with {2}", ModuleID, SelectedDriver_1, SelectedDriver_2);

				SelectedDriver_1 = "";
				SelectedDriver_2 = "";
				SelectedIndex_1 = 0;
				SelectedIndex_2 = 0;

				ResetLabels();
			}
			SelectedSomething = false;
		}
		return false;
	}
	protected bool Name11_Press()
	{
		GetComponent<KMSelectable>().AddInteractionPunch(0);
		GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
		Debug.LogFormat("(Grand Prix #{0}): (SYSTEM) Pressed button for name 11...", ModuleID, CurrentLap);
		if (!SelectedSomething)
		{
			SelectedSomething = true;
			SelectedIndex_1 = 10;
			SelectedDriver_1 = CurrentGridDrivers[10];
			AllDriverNameTextMeshes[10].color = Color.yellow;
			Debug.LogFormat("(Grand Prix #{0}): (SYSTEM) Selected driver {1}.", ModuleID, SelectedDriver_1);
		}
		else
		{
			if (SelectedIndex_1 == 10)
			{
				AllDriverNameTextMeshes[10].color = Color.white;
			}
			else
			{
				SelectedIndex_2 = 10;
				SelectedDriver_2 = CurrentGridDrivers[10];

				
				AllDriverNameTextMeshes[SelectedIndex_1].color = Color.white;
				AllDriverNameTextMeshes[SelectedIndex_2].color = Color.white;

				CurrentGridDrivers[SelectedIndex_1] = SelectedDriver_2;
				CurrentGridDrivers[SelectedIndex_2] = SelectedDriver_1;

				Debug.LogFormat("(Grand Prix #{0}): (SYSTEM) Swapped driver {1} with {2}", ModuleID, SelectedDriver_1, SelectedDriver_2);

				SelectedDriver_1 = "";
				SelectedDriver_2 = "";
				SelectedIndex_1 = 0;
				SelectedIndex_2 = 0;

				ResetLabels();
			}
			SelectedSomething = false;
		}
		return false;
	}
	protected bool Name12_Press()
	{
		GetComponent<KMSelectable>().AddInteractionPunch(0);
		GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
		Debug.LogFormat("(Grand Prix #{0}): (SYSTEM) Pressed button for name 12...", ModuleID, CurrentLap);
		if (!SelectedSomething)
		{
			SelectedSomething = true;
			SelectedIndex_1 = 11;
			SelectedDriver_1 = CurrentGridDrivers[11];
			AllDriverNameTextMeshes[11].color = Color.yellow;
			Debug.LogFormat("(Grand Prix #{0}): (SYSTEM) Selected driver {1}.", ModuleID, SelectedDriver_1);
		}
		else
		{
			if (SelectedIndex_1 == 11)
			{
				AllDriverNameTextMeshes[11].color = Color.white;
			}
			else
			{
				SelectedIndex_2 = 11;
				SelectedDriver_2 = CurrentGridDrivers[11];

				
				AllDriverNameTextMeshes[SelectedIndex_1].color = Color.white;
				AllDriverNameTextMeshes[SelectedIndex_2].color = Color.white;

				CurrentGridDrivers[SelectedIndex_1] = SelectedDriver_2;
				CurrentGridDrivers[SelectedIndex_2] = SelectedDriver_1;

				Debug.LogFormat("(Grand Prix #{0}): (SYSTEM) Swapped driver {1} with {2}", ModuleID, SelectedDriver_1, SelectedDriver_2);

				SelectedDriver_1 = "";
				SelectedDriver_2 = "";
				SelectedIndex_1 = 0;
				SelectedIndex_2 = 0;

				ResetLabels();
			}
			SelectedSomething = false;
		}
		return false;
	}
	protected bool Name13_Press()
	{
		GetComponent<KMSelectable>().AddInteractionPunch(0);
		GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
		Debug.LogFormat("(Grand Prix #{0}): (SYSTEM) Pressed button for name 13...", ModuleID, CurrentLap);
		if (!SelectedSomething)
		{
			SelectedSomething = true;
			SelectedIndex_1 = 12;
			SelectedDriver_1 = CurrentGridDrivers[12];
			AllDriverNameTextMeshes[12].color = Color.yellow;
			Debug.LogFormat("(Grand Prix #{0}): (SYSTEM) Selected driver {1}.", ModuleID, SelectedDriver_1);
		}
		else
		{
			if (SelectedIndex_1 == 12)
			{
				AllDriverNameTextMeshes[12].color = Color.white;
			}
			else
			{
				SelectedIndex_2 = 12;
				SelectedDriver_2 = CurrentGridDrivers[12];

				
				AllDriverNameTextMeshes[SelectedIndex_1].color = Color.white;
				AllDriverNameTextMeshes[SelectedIndex_2].color = Color.white;

				CurrentGridDrivers[SelectedIndex_1] = SelectedDriver_2;
				CurrentGridDrivers[SelectedIndex_2] = SelectedDriver_1;

				Debug.LogFormat("(Grand Prix #{0}): (SYSTEM) Swapped driver {1} with {2}", ModuleID, SelectedDriver_1, SelectedDriver_2);

				SelectedDriver_1 = "";
				SelectedDriver_2 = "";
				SelectedIndex_1 = 0;
				SelectedIndex_2 = 0;

				ResetLabels();
			}
			SelectedSomething = false;
		}
		return false;
	}
	protected bool Name14_Press()
	{
		GetComponent<KMSelectable>().AddInteractionPunch(0);
		GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
		Debug.LogFormat("(Grand Prix #{0}): (SYSTEM) Pressed button for name 14...", ModuleID, CurrentLap);
		if (!SelectedSomething)
		{
			SelectedSomething = true;
			SelectedIndex_1 = 13;
			SelectedDriver_1 = CurrentGridDrivers[13];
			AllDriverNameTextMeshes[13].color = Color.yellow;
			Debug.LogFormat("(Grand Prix #{0}): (SYSTEM) Selected driver {1}.", ModuleID, SelectedDriver_1);
		}
		else
		{
			if (SelectedIndex_1 == 13)
			{
				AllDriverNameTextMeshes[13].color = Color.white;
			}
			else
			{
				SelectedIndex_2 = 13;
				SelectedDriver_2 = CurrentGridDrivers[13];

				
				AllDriverNameTextMeshes[SelectedIndex_1].color = Color.white;
				AllDriverNameTextMeshes[SelectedIndex_2].color = Color.white;

				CurrentGridDrivers[SelectedIndex_1] = SelectedDriver_2;
				CurrentGridDrivers[SelectedIndex_2] = SelectedDriver_1;

				Debug.LogFormat("(Grand Prix #{0}): (SYSTEM) Swapped driver {1} with {2}", ModuleID, SelectedDriver_1, SelectedDriver_2);

				SelectedDriver_1 = "";
				SelectedDriver_2 = "";
				SelectedIndex_1 = 0;
				SelectedIndex_2 = 0;

				ResetLabels();
			}
			SelectedSomething = false;
		}
		return false;
	}
	protected bool Name15_Press()
	{
		GetComponent<KMSelectable>().AddInteractionPunch(0);
		GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
		Debug.LogFormat("(Grand Prix #{0}): (SYSTEM) Pressed button for name 15...", ModuleID, CurrentLap);
		if (!SelectedSomething)
		{
			SelectedSomething = true;
			SelectedIndex_1 = 14;
			SelectedDriver_1 = CurrentGridDrivers[14];
			AllDriverNameTextMeshes[14].color = Color.yellow;
			Debug.LogFormat("(Grand Prix #{0}): (SYSTEM) Selected driver {1}.", ModuleID, SelectedDriver_1);
		}
		else
		{
			if (SelectedIndex_1 == 14)
			{
				AllDriverNameTextMeshes[14].color = Color.white;
			}
			else
			{
				SelectedIndex_2 = 14;
				SelectedDriver_2 = CurrentGridDrivers[14];

				
				AllDriverNameTextMeshes[SelectedIndex_1].color = Color.white;
				AllDriverNameTextMeshes[SelectedIndex_2].color = Color.white;

				CurrentGridDrivers[SelectedIndex_1] = SelectedDriver_2;
				CurrentGridDrivers[SelectedIndex_2] = SelectedDriver_1;

				Debug.LogFormat("(Grand Prix #{0}): (SYSTEM) Swapped driver {1} with {2}", ModuleID, SelectedDriver_1, SelectedDriver_2);

				SelectedDriver_1 = "";
				SelectedDriver_2 = "";
				SelectedIndex_1 = 0;
				SelectedIndex_2 = 0;

				ResetLabels();
			}
			SelectedSomething = false;
		}
		return false;
	}
	protected bool Name16_Press()
	{
		GetComponent<KMSelectable>().AddInteractionPunch(0);
		GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
		Debug.LogFormat("(Grand Prix #{0}): (SYSTEM) Pressed button for name 16...", ModuleID, CurrentLap);
		if (!SelectedSomething)
		{
			SelectedSomething = true;
			SelectedIndex_1 = 15;
			SelectedDriver_1 = CurrentGridDrivers[15];
			AllDriverNameTextMeshes[15].color = Color.yellow;
			Debug.LogFormat("(Grand Prix #{0}): (SYSTEM) Selected driver {1}.", ModuleID, SelectedDriver_1);
		}
		else
		{
			if (SelectedIndex_1 == 15)
			{
				AllDriverNameTextMeshes[15].color = Color.white;
			}
			else
			{
				SelectedIndex_2 = 15;
				SelectedDriver_2 = CurrentGridDrivers[15];

				
				AllDriverNameTextMeshes[SelectedIndex_1].color = Color.white;
				AllDriverNameTextMeshes[SelectedIndex_2].color = Color.white;

				CurrentGridDrivers[SelectedIndex_1] = SelectedDriver_2;
				CurrentGridDrivers[SelectedIndex_2] = SelectedDriver_1;

				Debug.LogFormat("(Grand Prix #{0}): (SYSTEM) Swapped driver {1} with {2}", ModuleID, SelectedDriver_1, SelectedDriver_2);

				SelectedDriver_1 = "";
				SelectedDriver_2 = "";
				SelectedIndex_1 = 0;
				SelectedIndex_2 = 0;

				ResetLabels();
			}
			SelectedSomething = false;
		}
		return false;
	}
	protected bool Name17_Press()
	{
		GetComponent<KMSelectable>().AddInteractionPunch(0);
		GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
		Debug.LogFormat("(Grand Prix #{0}): (SYSTEM) Pressed button for name 17...", ModuleID, CurrentLap);
		if (!SelectedSomething)
		{
			SelectedSomething = true;
			SelectedIndex_1 = 16;
			SelectedDriver_1 = CurrentGridDrivers[16];
			AllDriverNameTextMeshes[16].color = Color.yellow;
			Debug.LogFormat("(Grand Prix #{0}): (SYSTEM) Selected driver {1}.", ModuleID, SelectedDriver_1);
		}
		else
		{
			if (SelectedIndex_1 == 16)
			{
				AllDriverNameTextMeshes[16].color = Color.white;
			}
			else
			{
				SelectedIndex_2 = 16;
				SelectedDriver_2 = CurrentGridDrivers[16];

				
				AllDriverNameTextMeshes[SelectedIndex_1].color = Color.white;
				AllDriverNameTextMeshes[SelectedIndex_2].color = Color.white;

				CurrentGridDrivers[SelectedIndex_1] = SelectedDriver_2;
				CurrentGridDrivers[SelectedIndex_2] = SelectedDriver_1;

				Debug.LogFormat("(Grand Prix #{0}): (SYSTEM) Swapped driver {1} with {2}", ModuleID, SelectedDriver_1, SelectedDriver_2);

				SelectedDriver_1 = "";
				SelectedDriver_2 = "";
				SelectedIndex_1 = 0;
				SelectedIndex_2 = 0;

				ResetLabels();
			}
			SelectedSomething = false;
		}
		return false;
	}
	protected bool Name18_Press()
	{
		GetComponent<KMSelectable>().AddInteractionPunch(0);
		GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
		Debug.LogFormat("(Grand Prix #{0}): (SYSTEM) Pressed button for name 18...", ModuleID, CurrentLap);
		if (!SelectedSomething)
		{
			SelectedSomething = true;
			SelectedIndex_1 = 17;
			SelectedDriver_1 = CurrentGridDrivers[17];
			AllDriverNameTextMeshes[17].color = Color.yellow;
			Debug.LogFormat("(Grand Prix #{0}): (SYSTEM) Selected driver {1}.", ModuleID, SelectedDriver_1);
		}
		else
		{
			if (SelectedIndex_1 == 17)
			{
				AllDriverNameTextMeshes[17].color = Color.white;
			}
			else
			{
				SelectedIndex_2 = 17;
				SelectedDriver_2 = CurrentGridDrivers[17];

				
				AllDriverNameTextMeshes[SelectedIndex_1].color = Color.white;
				AllDriverNameTextMeshes[SelectedIndex_2].color = Color.white;

				CurrentGridDrivers[SelectedIndex_1] = SelectedDriver_2;
				CurrentGridDrivers[SelectedIndex_2] = SelectedDriver_1;

				Debug.LogFormat("(Grand Prix #{0}): (SYSTEM) Swapped driver {1} with {2}", ModuleID, SelectedDriver_1, SelectedDriver_2);

				SelectedDriver_1 = "";
				SelectedDriver_2 = "";
				SelectedIndex_1 = 0;
				SelectedIndex_2 = 0;

				ResetLabels();
			}
			SelectedSomething = false;
		}
		return false;
	}
	protected bool Name19_Press()
	{
		GetComponent<KMSelectable>().AddInteractionPunch(0);
		GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
		Debug.LogFormat("(Grand Prix #{0}): (SYSTEM) Pressed button for name 19...", ModuleID, CurrentLap);
		if (!SelectedSomething)
		{
			SelectedSomething = true;
			SelectedIndex_1 = 18;
			SelectedDriver_1 = CurrentGridDrivers[18];
			AllDriverNameTextMeshes[18].color = Color.yellow;
			Debug.LogFormat("(Grand Prix #{0}): (SYSTEM) Selected driver {1}.", ModuleID, SelectedDriver_1);
		}
		else
		{
			if (SelectedIndex_1 == 18)
			{
				AllDriverNameTextMeshes[18].color = Color.white;
			}
			else
			{
				SelectedIndex_2 = 18;
				SelectedDriver_2 = CurrentGridDrivers[18];

				
				AllDriverNameTextMeshes[SelectedIndex_1].color = Color.white;
				AllDriverNameTextMeshes[SelectedIndex_2].color = Color.white;

				CurrentGridDrivers[SelectedIndex_1] = SelectedDriver_2;
				CurrentGridDrivers[SelectedIndex_2] = SelectedDriver_1;

				Debug.LogFormat("(Grand Prix #{0}): (SYSTEM) Swapped driver {1} with {2}", ModuleID, SelectedDriver_1, SelectedDriver_2);

				SelectedDriver_1 = "";
				SelectedDriver_2 = "";
				SelectedIndex_1 = 0;
				SelectedIndex_2 = 0;

				ResetLabels();
			}
			SelectedSomething = false;
		}
		return false;
	}
	protected bool Name20_Press()
	{
		GetComponent<KMSelectable>().AddInteractionPunch(0);
		GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
		Debug.LogFormat("(Grand Prix #{0}): (SYSTEM) Pressed button for name 20...", ModuleID, CurrentLap);
		if (!SelectedSomething)
		{
			SelectedSomething = true;
			SelectedIndex_1 = 19;
			SelectedDriver_1 = CurrentGridDrivers[19];
			AllDriverNameTextMeshes[19].color = Color.yellow;
			Debug.LogFormat("(Grand Prix #{0}): (SYSTEM) Selected driver {1}.", ModuleID, SelectedDriver_1);
		}
		else
		{
			if (SelectedIndex_1 == 19)
			{
				AllDriverNameTextMeshes[19].color = Color.white;
			}
			else
			{
				SelectedIndex_2 = 19;
				SelectedDriver_2 = CurrentGridDrivers[19];

				
				AllDriverNameTextMeshes[SelectedIndex_1].color = Color.white;
				AllDriverNameTextMeshes[SelectedIndex_2].color = Color.white;

				CurrentGridDrivers[SelectedIndex_1] = SelectedDriver_2;
				CurrentGridDrivers[SelectedIndex_2] = SelectedDriver_1;

				Debug.LogFormat("(Grand Prix #{0}): (SYSTEM) Swapped driver {1} with {2}", ModuleID, SelectedDriver_1, SelectedDriver_2);

				SelectedDriver_1 = "";
				SelectedDriver_2 = "";
				SelectedIndex_1 = 0;
				SelectedIndex_2 = 0;

				ResetLabels();
			}
			SelectedSomething = false;
		}
		return false;
	}

	void ResetLabels()
    {
		for (int T = 0; T < 20; T++)
		{
			AllDriverNameTextMeshes[T].text = CurrentGridDrivers[T];
		}

		int CurrentDriver_Driver = 0;
		foreach (string Driver in CurrentGridDrivers)
		{
			if (Team_MERCEDES.Any(Driver.Contains))
			{
				CurrentGridTeams[CurrentDriver_Driver] = "Mercedes";
				TeamRenders[CurrentDriver_Driver].material.mainTexture = TeamTextures[0];
			}
			else if (Team_RB.Any(Driver.Contains))
			{
				CurrentGridTeams[CurrentDriver_Driver] = "Red Bull";
				TeamRenders[CurrentDriver_Driver].material.mainTexture = TeamTextures[1];
			}
			else if (Team_MCLAREN.Any(Driver.Contains))
			{
				CurrentGridTeams[CurrentDriver_Driver] = "McLaren";
				TeamRenders[CurrentDriver_Driver].material.mainTexture = TeamTextures[2];
			}
			else if (Team_WILLIAMS.Any(Driver.Contains))
			{
				CurrentGridTeams[CurrentDriver_Driver] = "Williams";
				TeamRenders[CurrentDriver_Driver].material.mainTexture = TeamTextures[3];
			}
			else if (Team_FERRARI.Any(Driver.Contains))
			{
				CurrentGridTeams[CurrentDriver_Driver] = "Ferrari";
				TeamRenders[CurrentDriver_Driver].material.mainTexture = TeamTextures[4];
			}
			else if (Team_HAAS.Any(Driver.Contains))
			{
				CurrentGridTeams[CurrentDriver_Driver] = "Haas";
				TeamRenders[CurrentDriver_Driver].material.mainTexture = TeamTextures[5];
			}
			else if (Team_ALPINE.Any(Driver.Contains))
			{
				CurrentGridTeams[CurrentDriver_Driver] = "Alpine";
				TeamRenders[CurrentDriver_Driver].material.mainTexture = TeamTextures[6];
			}
			else if (Team_ROMEO.Any(Driver.Contains))
			{
				CurrentGridTeams[CurrentDriver_Driver] = "Alfa Romeo";
				TeamRenders[CurrentDriver_Driver].material.mainTexture = TeamTextures[7];
			}
			else if (Team_TAURI.Any(Driver.Contains))
			{
				CurrentGridTeams[CurrentDriver_Driver] = "Alpha Tauri";
				TeamRenders[CurrentDriver_Driver].material.mainTexture = TeamTextures[8];
			}
			else if (Team_ASTON.Any(Driver.Contains))
			{
				CurrentGridTeams[CurrentDriver_Driver] = "Aston Martin";
				TeamRenders[CurrentDriver_Driver].material.mainTexture = TeamTextures[9];
			}
			CurrentDriver_Driver++;
		}
	}

	protected bool HandleSubmit()
    {
		GetComponent<KMSelectable>().AddInteractionPunch(1);
		Debug.LogFormat("[Grand Prix #{0}]: (Final lap) Pressed the submit flag...", ModuleID, CurrentLap);
		if (!StrikeMode)
        {
			Debug.LogFormat("[Grand Prix #{0}]: (Final lap) Submitted: {1}", ModuleID, string.Join(", ", CurrentGridDrivers));
			Debug.LogFormat("[Grand Prix #{0}]: (Final lap) Expected: {1}", ModuleID, string.Join(", ", EndResultsList));
			if (CurrentGridDrivers.SequenceEqual(EndResultsList))
			{
				Debug.LogFormat("[Grand Prix #{0}]: (Final lap) Current grid matches expected end result! Module solved.", ModuleID);
				ThisModule.HandlePass();

				SubmitFlag.gameObject.SetActive(false);
				FlagRender.gameObject.SetActive(true);
				FlagRender.material.mainTexture = AllFlagTextures[8];
				FlagDescriptionBox.material.mainTexture = ChequeredFlag;
				FlagDescriptionBox.material.color = Color.white;
				SectorInfo_LapTextMesh.gameObject.SetActive(false);

				LapCountTextMesh.text = "Finish";
			}
			else
			{
				Debug.LogFormat("[Grand Prix #{0}]: (Final lap) Current grid didn't match expected end result! Strike handed.", ModuleID);
				ThisModule.HandleStrike();
				Debug.LogFormat("(Grand Prix #{0}): (SYSTEM) Strike handed! Entering Strike Mode.", ModuleID);

				SectorInfo_LapTextMesh.text = "Strike!";
				CurrentLap--;
				if (CurrentLap == GreenFlagLap)
                {
					CurrentLap--;
                }
				else if (CurrentLap < 1)
				{
					CurrentLap++;
				}
				LapCountTextMesh.text = "Lap " + CurrentLap + "/" + LapCount;
				FlagDescriptionBox.material.color = Color.red;

				UpButton.gameObject.transform.localScale = new Vector3(0.08026525f, 0.0790887f, 0.1296025f);
				DownButton.gameObject.transform.localScale = new Vector3(0.08026525f, 0.0790887f, 0.1296025f);

				foreach (TextMesh DriverText in AllDriverNameTextMeshes)
				{
					DriverText.gameObject.SetActive(true);
				}

				foreach (KMSelectable DriverButton in AllNameSelectables)
				{
					DriverButton.OnInteract = Empty;
				}

				for (int T = 0; T < 20; T++)
				{
					CurrentGridDrivers[T] = StartingGridDrivers[T];
				}
				ResetLabels();

				DeltasGameObject.SetActive(true);

				StrikeMode = true;
			}
		}
		else
		{
			Showing = true;

			int LapToShow = CurrentLap - 1;
			Debug.LogFormat("(Grand Prix #{0}): (SYSTEM) Showing lap {1}...", ModuleID, LapToShow + 1);

			UpButton.gameObject.transform.localScale = new Vector3(0, 0, 0);

			SectorInfo_LapTextMesh.text = "Sector " + History_SectorInfo[LapToShow];
			SectorInfo_DriverTextMesh.text = History_DriverInfo[LapToShow];
			DriverSectorTextMesh.text = History_DriverSectors[LapToShow];

			FlagRender.gameObject.SetActive(true);
			SubmitFlag.gameObject.SetActive(false);
			DriverSectorTextMesh.gameObject.SetActive(true);
			SectorInfo_DriverTextMesh.gameObject.SetActive(true);

			switch (History_FlagType[LapToShow])
			{
				case "Yellow":
					{
						FlagRender.material.mainTexture = AllFlagTextures[0];
						FlagDescriptionBox.material.color = Color.yellow;
						SectorInfo_DriverTextMesh.color = Color.black;
						SectorInfo_LapTextMesh.color = Color.black;
						break;
					}
				case "YellowSC":
					{
						FlagRender.material.mainTexture = AllFlagTextures[7];
						FlagDescriptionBox.material.color = Color.yellow;
						SectorInfo_DriverTextMesh.color = Color.black;
						SectorInfo_LapTextMesh.color = Color.black;
						break;
					}
				case "Blue":
					{
						FlagRender.material.mainTexture = AllFlagTextures[1];
						FlagDescriptionBox.material.color = Color.blue;
						SectorInfo_DriverTextMesh.color = Color.black;
						SectorInfo_LapTextMesh.color = Color.black;
						break;
					}
				case "Red":
					{
						FlagRender.material.mainTexture = AllFlagTextures[2];
						FlagDescriptionBox.material.color = Color.red;
						SectorInfo_DriverTextMesh.color = Color.black;
						SectorInfo_LapTextMesh.color = Color.black;
						break;
					}
				case "Penalty":
					{
						FlagRender.material.mainTexture = AllFlagTextures[3];
						FlagDescriptionBox.material.color = Color.white;
						SectorInfo_DriverTextMesh.color = Color.black;
						SectorInfo_LapTextMesh.color = Color.black;
						break;
					}
				case "White":
					{
						FlagRender.material.mainTexture = AllFlagTextures[4];
						FlagDescriptionBox.material.color = Color.white;
						SectorInfo_DriverTextMesh.color = Color.black;
						SectorInfo_LapTextMesh.color = Color.black;
						break;
					}
				case "Black":
					{
						FlagRender.material.mainTexture = AllFlagTextures[5];
						FlagDescriptionBox.material.color = Color.black;
						SectorInfo_DriverTextMesh.color = Color.white;
						SectorInfo_LapTextMesh.color = Color.white;
						break;
					}
			}
		}
		return false;
    }

	protected bool UpButton_Press()
    {
		Debug.LogFormat("(Grand Prix #{0}): (SYSTEM) Pressed Up flag cycle button...", ModuleID);
		GetComponent<KMSelectable>().AddInteractionPunch(1);
		CurrentLap++;
		if (CurrentLap > LapCount - 1)
        {
			CurrentLap--;
        }
		else if (CurrentLap == GreenFlagLap)
        {
			CurrentLap++;
			if (CurrentLap > LapCount - 1)
			{
				CurrentLap -= 2;
			}
		}

		LapCountTextMesh.text = "Lap " + CurrentLap + "/" + LapCount;
		return false;
    }

	bool Showing;
	protected bool DownButton_Press()
	{
		Debug.LogFormat("(Grand Prix #{0}): (SYSTEM) Pressed Down flag cycle button...", ModuleID);
		GetComponent<KMSelectable>().AddInteractionPunch(1);
		if (StrikeMode && !Showing)
        {
			CurrentLap--;
			if (CurrentLap < 1)
            {
				CurrentLap++;
            }
			else if (CurrentLap == GreenFlagLap)
            {
				CurrentLap--;
				if (CurrentLap < 1)
				{
					CurrentLap += 2;
				}
			}

			LapCountTextMesh.text = "Lap " + CurrentLap + "/" + LapCount;
		}
		else
        {
			Debug.LogFormat("(Grand Prix #{0}): (SYSTEM) Ending Strike Mode.", ModuleID);
			AllNameSelectables[0].OnInteract = Name1_Press;
			AllNameSelectables[1].OnInteract = Name2_Press;
			AllNameSelectables[2].OnInteract = Name3_Press;
			AllNameSelectables[3].OnInteract = Name4_Press;
			AllNameSelectables[4].OnInteract = Name5_Press;
			AllNameSelectables[5].OnInteract = Name6_Press;
			AllNameSelectables[6].OnInteract = Name7_Press;
			AllNameSelectables[7].OnInteract = Name8_Press;
			AllNameSelectables[8].OnInteract = Name9_Press;
			AllNameSelectables[9].OnInteract = Name10_Press;
			AllNameSelectables[10].OnInteract = Name11_Press;
			AllNameSelectables[11].OnInteract = Name12_Press;
			AllNameSelectables[12].OnInteract = Name13_Press;
			AllNameSelectables[13].OnInteract = Name14_Press;
			AllNameSelectables[14].OnInteract = Name15_Press;
			AllNameSelectables[15].OnInteract = Name16_Press;
			AllNameSelectables[16].OnInteract = Name17_Press;
			AllNameSelectables[17].OnInteract = Name18_Press;
			AllNameSelectables[18].OnInteract = Name19_Press;
			AllNameSelectables[19].OnInteract = Name20_Press;

			SubmitFlag.OnInteract = HandleSubmit;

			LapCountTextMesh.text = "Final lap";
			SectorInfo_LapTextMesh.text = "Final lap";

			FlagDescriptionBox.material.color = Color.green;
			SectorInfo_DriverTextMesh.color = Color.black;
			SectorInfo_LapTextMesh.color = Color.black;

			FlagRender.gameObject.SetActive(false);
			SubmitFlag.gameObject.SetActive(true);
			DeltasGameObject.SetActive(false);
			DriverSectorTextMesh.gameObject.SetActive(false);
			SectorInfo_DriverTextMesh.gameObject.SetActive(false);

			DownButton.gameObject.transform.localScale = new Vector3(0, 0, 0);

			StrikeMode = false;
			Showing = false;
		}
		return false;
	}

	//Twitch Plays
	private readonly string TwitchHelpMessage = "Type !{0} swap [driver] [driver] to swap the positions of the two drivers. You can either use their acronym (HAM) or their position (#1, must include the hashtag). Type !{0} submit to submit the results. If it's incorrect, type !{0} show [#] to show the flag info for lap #, then type !{0} continue to return to the final lap submission.";
	IEnumerator ProcessTwitchCommand(string Command)
	{
		Command = Command.ToLowerInvariant().Trim();
		List<string> CommandArgs = Command.Split(" ").ToList();

		for (int T = 0; T < 3 - CommandArgs.Count; T++)
        {
			CommandArgs.Add("");
        }

		if (!FinalLap)
        {
			yield return "sendtochat This module hasn't reached the final lap yet. It is currently at lap " + CurrentLap + " of " + LapCount + ".";
		}
		else
        {
			if (CommandArgs[0] == "swap")
			{
				int DriverToSwap1,
					DriverToSwap2;
				int result;

				if (CommandArgs[1].StartsWith("#"))
				{
					CommandArgs[1] = CommandArgs[1].Replace("#", "");

					if (int.TryParse(CommandArgs[1], out result))
					{
						DriverToSwap1 = int.Parse(CommandArgs[1]);

						if (DriverToSwap1 <= 20 && DriverToSwap1 > 0)
                        {
							//Choice 1 is valid. Next choice

							if (CommandArgs[2].StartsWith("#"))
							{
								CommandArgs[2] = CommandArgs[2].Replace("#", "");
								if (int.TryParse(CommandArgs[2], out result))
								{
									DriverToSwap2 = int.Parse(CommandArgs[2]);
									if (DriverToSwap2 <= 20 && DriverToSwap2 > 0)
                                    {
										//VALID RESULT, FINAL CHECK.
										if (DriverToSwap1 != DriverToSwap2)
                                        {
											yield return null;
											yield return new[] { AllNameSelectables[DriverToSwap1 - 1] };
											yield return new[] { AllNameSelectables[DriverToSwap2 - 1] };
										}
										else
                                        {
											yield return "sendtochat Cannot swap a driver with himself!";
										}
									}
									else
                                    {
										yield return "sendtochat \"" + CommandArgs[2] + "\" is not within 1 and 20 and therefore not a valid position to swap";
									}
								}
								else
								{
									yield return "sendtochat \"" + CommandArgs[2] + "\" is not a number and therefore not a valid position to swap";
								}
							}
							else if (CurrentGridDrivers.Contains(CommandArgs[2].ToUpper()))
							{
								yield return "sendtochat You cannot mix using positions and driver initials when swapping. Use either the positions or the initials only.";
							}
							else if (CommandArgs[2] == "")
							{
								yield return "sendtochat There is no given argument for which driver to swap (2nd driver).";
							}
							else
							{
								yield return "sendtochat Driver \"" + CommandArgs[2] + "\" could not be recognized";
							}
						}
						else
                        {
							yield return "sendtochat \"" + CommandArgs[1] + "\" is not within 1 and 20 and therefore not a valid position to swap";
						}
						
					}
					else
					{
						yield return "sendtochat \"" + CommandArgs[1] + "\" is not a number and therefore not a valid position to swap";
					}
				}
				else if (CurrentGridDrivers.Contains(CommandArgs[1].ToUpper()))
				{
					DriverToSwap1 = CurrentGridDrivers.IndexOf(CommandArgs[1].ToUpper());
					//Choice 1 is valid. Next choice

					if (CurrentGridDrivers.Contains(CommandArgs[2].ToUpper()))
					{
						DriverToSwap2 = CurrentGridDrivers.IndexOf(CommandArgs[2].ToUpper());
						//VALID RESULT, SWAP POSITIONS.

						if (DriverToSwap1 != DriverToSwap2)
						{
							yield return null;
							yield return new[] { AllNameSelectables[DriverToSwap1] };
							yield return new[] { AllNameSelectables[DriverToSwap2] };
						}
						else
						{
							yield return "sendtochat Cannot swap a driver with himself!";
						}
					}
					else if (CommandArgs[2].StartsWith("#"))
					{
						yield return "sendtochat You cannot mix using positions and driver initials when swapping. Use either the positions or the initials only.";
					}
					else if (CommandArgs[2] == "")
					{
						yield return "sendtochat There is no given argument for which driver to swap (2nd driver).";
					}
					else
					{
						yield return "sendtochat Driver \"" + CommandArgs[2] + "\" could not be recognized";
					}
				}
				else if (CommandArgs[1] == "")
                {
					yield return "sendtochat There is no given argument for which driver to swap (1st driver).";
				}
				else
				{
					yield return "sendtochat Driver \"" + CommandArgs[1] + "\" could not be recognized";
				}

			}
			else if (CommandArgs[0] == "submit")
			{
				if (!StrikeMode)
                {
					//Submit order
					yield return null;
					yield return new[] { SubmitFlag };
                }
				else
                {
					yield return "sendtochat Cannot submit now since a strike was handed by this module and a flag hasn't been chosen yet to review. First, choose a flag you wish to review using !{1} show #, then after that, use !{1} continue to return to the final lap.";
				}
			}
			else if (CommandArgs[0] == "show")
			{
				if (StrikeMode)
                {
					int result;
					if (int.TryParse(CommandArgs[1], out result))
					{
						result = int.Parse(CommandArgs[1]);
						Debug.Log("Result: " + result + ", CurrentLap: " + CurrentLap);
						if (result == CurrentLap)
						{
							yield return null;
							yield return new[] { SubmitFlag };
							yield return new[] { SubmitFlag };
						}
						else if (result < 1)
                        {
							yield return "sendtochat Lap number cannot be lower than 1";
						}
						else if (result > LapCount - 1)
						{
							yield return "sendtochat Lap number cannot exceed the lap count, nor can it be the final lap.";
						}
						else if (result == GreenFlagLap)
                        {
							yield return "sendtochat You cannot select the green flag lap. If you wish to see the deltas, they are visible on the module.";
						}
						else
                        {
							//The number is valid
							if (result > CurrentLap)
                            {
								for (int T = result - CurrentLap; T > 0; T--)
                                {
									yield return null;
									yield return new[] { UpButton };
								}
							}
							else
                            {
								for (int T = CurrentLap - result; T > 0; T--)
								{
									yield return null;
									yield return new[] { DownButton };
								}
							}
							yield return null;
							yield return new[] { SubmitFlag };
						}
					}
					else
                    {
						yield return "sendtochat \"" + CommandArgs[1] + "\" is not a valid lap number";
					}
				}
				else
                {
					yield return "sendtochat Cannot review flag now since this module has yet to hand a strike. You may only review a flag after this module handed a strike.";
				}

			}
			else if (CommandArgs[0] == "continue")
			{
				if (Showing)
                {
					yield return null;
					yield return new[] { DownButton };
				}
				else
                {
					yield return "sendtochat Currently not reviewing a flag. The continue command only works when a flag is being reviewed.";
				}
			}
			else
            {
				yield return "sendtochat The command \"" + CommandArgs[0] + "\" is not recognized.";
			}
        }
		yield return null;
	}

}