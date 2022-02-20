using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ModuleSolver : MonoBehaviour 
{
    public KMBombModule ThisModule;
    public KMSelectable SolveButton;


    void Start () 
	{
        SolveButton.OnInteract = Solver;
    }

    
    protected bool Solver()
    {
        ThisModule.HandlePass();
        return false;
    }

    public KMSelectable[] ProcessTwitchCommand(string Command)
    {
        Command = Command.ToLowerInvariant().Trim();

        if (Command.Equals("solve"))
        {
            ThisModule.HandlePass();
        }
        return null;
    }
}
