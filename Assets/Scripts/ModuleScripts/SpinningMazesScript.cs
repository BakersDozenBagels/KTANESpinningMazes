using KeepCoding;
using System;
using System.Collections;
using UnityEngine;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;

public class SpinningMazesScript : ModuleScript
{
    [SerializeField]
    private KMSelectable[] Buttons;
    [SerializeField]
    private MeshRenderer[] LEDs;

    private enum Color
    {
        Red = 0,
        Blue = 1,
        Yellow = 2,
        Green = 3
    }

    [Flags]
    private enum Direction
    {
        None = 0,
        Up = 1,
        Down = 2,
        Left = 4,
        Right = 8
    }

    private int ColoredButton;
    private Color ColoredButtonColor;
    private Direction[] ButtonDirections, InitialButtonDirections = new Direction[] { Direction.Up, Direction.Down, Direction.Left, Direction.Right };
    private int X = 4, Y = 4;

    private readonly Direction[][] Maze = new Direction[][] {
        new int[] { 0, 12, 14, 12, 12, 5, 10, 12, 0 }.Cast<Direction>().ToArray(),
        new int[] { 3, 0, 9, 14, 12, 14, 5, 0, 3 }.Cast<Direction>().ToArray(),
        new int[] { 9, 12, 14, 15, 14, 15, 12, 6, 3 }.Cast<Direction>().ToArray(),
        new int[] { 12, 6, 6, 3, 11, 5, 8, 15, 7 }.Cast<Direction>().ToArray(),
        new int[] { 10, 5, 3, 11, 15, 12, 14, 7, 3 }.Cast<Direction>().ToArray(),
        new int[] { 9, 14, 5, 3, 11, 14, 13, 7, 9 }.Cast<Direction>().ToArray(),
        new int[] { 10, 13, 14, 15, 13, 7, 10, 13, 6 }.Cast<Direction>().ToArray(),
        new int[] { 3, 0, 3, 3, 10, 13, 5, 0, 3 }.Cast<Direction>().ToArray(),
        new int[] { 0, 12, 5, 3, 9, 12, 12, 12, 0 }.Cast<Direction>().ToArray(),
    };

    private void Start()
    {
        Buttons.Assign(onInteract: ButtonPress);
        ColoredButton = UnityEngine.Random.Range(0, 4);
        ColoredButtonColor = (Color)UnityEngine.Random.Range(0, 4);

        Color32 displayedColor = new Color32();
        Direction badDirection = Direction.None;
        switch(ColoredButtonColor)
        {
            case Color.Red:
                displayedColor = new Color32(255, 0, 0, 255);
                badDirection = Direction.Up;
                break;
            case Color.Green:
                displayedColor = new Color32(0, 255, 0, 255);
                badDirection = Direction.Right;
                break;
            case Color.Blue:
                displayedColor = new Color32(0, 0, 255, 255);
                badDirection = Direction.Down;
                break;
            case Color.Yellow:
                displayedColor = new Color32(255, 255, 0, 255);
                badDirection = Direction.Left;
                break;
        }
        Buttons[ColoredButton].GetComponent<MeshRenderer>().material.color = displayedColor;

        do
            InitialButtonDirections = InitialButtonDirections.Shuffle();
        while(InitialButtonDirections[ColoredButton] == badDirection);
        ButtonDirections = InitialButtonDirections;

        string buttonString = "";
        switch(ColoredButton)
        {
            case 0:
                buttonString = "top-left";
                break;
            case 1:
                buttonString = "top-right";
                break;
            case 2:
                buttonString = "bottom-left";
                break;
            case 3:
                buttonString = "bottom-right";
                break;
        }

        Log("The {0} button is colored {1}.", buttonString, ColoredButtonColor);
        Log("The buttons (in reading order) move you in the following directions in the maze: {0}, {1}, {2}, {3}", ButtonDirections[0], ButtonDirections[1], ButtonDirections[2], ButtonDirections[3]);
    }

    private void ButtonPress(int ix)
    {
        ButtonEffect(Buttons[ix], 0.05f, KMSoundOverride.SoundEffect.ButtonPress);
        StartCoroutine(ButtonMovement(Buttons[ix]));
        if(IsSolved)
            return;

        if((ButtonDirections[ix] & Maze[Y][X]) == Direction.None)
        {
            Strike("You ran into a wall by trying to go {0} from ({1},{2})! Resetting to (5,5).".Form(ButtonDirections[ix], X + 1, Y + 1));
            X = 4;
            Y = 4;
            LEDs.ForEach(r => r.material.color = new Color32(0, 0, 0, 255));
            ButtonDirections = InitialButtonDirections;
            return;
        }

        switch(ButtonDirections[ix])
        {
            case Direction.Up:
                Y--;
                break;
            case Direction.Down:
                Y++;
                break;
            case Direction.Right:
                X++;
                break;
            case Direction.Left:
                X--;
                break;
        }


        if(X == 0 && Y == 0)
        {
            if(ButtonDirections[ix] == Direction.Up)
            {
                X = 2;
                RotateClockwise();
            }
            else
            {
                Y = 2;
                RotateCounterClockwise();
            }
        }
        if(X == 0 && Y == 8)
        {
            if(ButtonDirections[ix] == Direction.Left)
            {
                Y = 6;
                RotateClockwise();
            }
            else
            {
                X = 2;
                RotateCounterClockwise();
            }
        }
        if(X == 8 && Y == 8)
        {
            if(ButtonDirections[ix] == Direction.Down)
            {
                X = 6;
                RotateClockwise();
            }
            else
            {
                Y = 6;
                RotateCounterClockwise();
            }
        }
        if(X == 8 && Y == 0)
        {
            if(ButtonDirections[ix] == Direction.Right)
            {
                Y = 2;
                RotateClockwise();
            }
            else
            {
                X = 6;
                RotateCounterClockwise();
            }
        }

        Log("Moved to ({0},{1}).", X + 1, Y + 1);

        if(X < 0)
        {
            if(ColoredButtonColor == Color.Yellow && ix == ColoredButton)
            {
                Solve("Congratulations! You got it correct.");
                PlaySound(KMSoundOverride.SoundEffect.CorrectChime);
            }
            else
            {
                Strike("You tried to exit the maze incorrectly! Resetting to (5,5).");
                X = 4;
                Y = 4;
                ButtonDirections = InitialButtonDirections;
            }
        }
        if(Y < 0)
        {
            if(ColoredButtonColor == Color.Red && ix == ColoredButton)
            {
                Solve("Congratulations! You got it correct.");
                PlaySound(KMSoundOverride.SoundEffect.CorrectChime);
            }
            else
            {
                Strike("You tried to exit the maze incorrectly! Resetting to (5,5).");
                X = 4;
                Y = 4;
                ButtonDirections = InitialButtonDirections;
            }
        }
        if(X > 8)
        {
            if(ColoredButtonColor == Color.Green && ix == ColoredButton)
            {
                Solve("Congratulations! You got it correct.");
                PlaySound(KMSoundOverride.SoundEffect.CorrectChime);
            }
            else
            {
                Strike("You tried to exit the maze incorrectly! Resetting to (5,5).");
                X = 4;
                Y = 4;
                ButtonDirections = InitialButtonDirections;
            }
        }
        if(Y > 8)
        {
            if(ColoredButtonColor == Color.Blue && ix == ColoredButton)
            {
                Solve("Congratulations! You got it correct.");
                PlaySound(KMSoundOverride.SoundEffect.CorrectChime);
            }
            else
            {
                Strike("You tried to exit the maze incorrectly! Resetting to (5,5).");
                X = 4;
                Y = 4;
                ButtonDirections = InitialButtonDirections;
            }
        }

        LEDs.ForEach(r => r.material.color = new Color32(0, 0, 0, 255));
        if(X == 3)
        {
            LEDs[1].material.color = new Color32(0, 0, 255, 255);
        }
        if(X == 5)
        {
            LEDs[0].material.color = new Color32(255, 0, 0, 255);
        }
        if(Y == 3)
        {
            LEDs[2].material.color = new Color32(255, 255, 0, 255);
        }
        if(Y == 5)
        {
            LEDs[3].material.color = new Color32(0, 255, 0, 255);
        }
    }

    private void RotateCounterClockwise()
    {
        Log("You went across a curved space, so your inputs are now rotated counterclockwise.");
        ButtonDirections = ButtonDirections.Select(d =>
        {
            switch(d)
            {
                case Direction.Up:
                    return Direction.Left;
                case Direction.Down:
                    return Direction.Right;
                case Direction.Left:
                    return Direction.Down;
                case Direction.Right:
                    return Direction.Up;
                default:
                    return Direction.None;
            }
        }).ToArray();
    }

    private void RotateClockwise()
    {
        Log("You went across a curved space, so your inputs are now rotated clockwise.");
        ButtonDirections = ButtonDirections.Select(d =>
        {
            switch(d)
            {
                case Direction.Up:
                    return Direction.Right;
                case Direction.Down:
                    return Direction.Left;
                case Direction.Left:
                    return Direction.Up;
                case Direction.Right:
                    return Direction.Down;
                default:
                    return Direction.None;
            }
        }).ToArray();
    }

    private IEnumerator ButtonMovement(KMSelectable button)
    {
        float time = 0;
        while(time < 0.1f)
        {
            time += Time.deltaTime;
            button.transform.parent.localPosition = Vector3.Lerp(Vector3.zero, Vector3.down * 0.03f, time);
            yield return null;
        }
        while(time < 0.2f)
        {
            time += Time.deltaTime;
            button.transform.parent.localPosition = Vector3.Lerp(Vector3.down * 0.03f, Vector3.zero, time + 0.8f);
            yield return null;
        }
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = "Use '!{0} 12 34' to press the top-left, top-right, bottom-left, and bottom-right bbuttons in that order.";
#pragma warning restore 414

    public IEnumerator ProcessTwitchCommand(string command)
    {
        Regex r = new Regex(@"^(?:press\s+)?((?:[1-4]\s*)+)$");
        Match m = r.Match(command.Trim().ToLowerInvariant());
        if(!m.Success)
            yield break;

        Regex cr = new Regex("[1-4]");
        IEnumerable<KMSelectable> bs = m.Groups[1].Value.Where(c => cr.IsMatch(c.ToString())).Select(c => Buttons[int.Parse(c.ToString()) - 1]);

        foreach(KMSelectable b in bs)
        {
            b.OnInteract();
            yield return new WaitForSeconds(0.2f);
        }
        yield return null;
    }

    public IEnumerator TwitchHandleForcedSolve()
    {
        foreach(object e in SpinToDirection())
            yield return e;

        Vector2Int exitp;
        switch(ColoredButtonColor)
        {
            case Color.Red:
                exitp = new Vector2Int(5, 0);
                break;
            case Color.Green:
                exitp = new Vector2Int(8, 5);
                break;
            case Color.Blue:
                exitp = new Vector2Int(3, 8);
                break;
            case Color.Yellow:
                exitp = new Vector2Int(0, 3);
                break;
            default:
                throw new Exception("The colored button was not RGBY. This shouldn't have happened, so please contact Bagels.");
        }

        foreach(object e in SearchAndMoveTo(new[] { exitp }))
            yield return e;

        if(X == 0)
            Buttons[ButtonDirections.IndexOf(Direction.Left)].OnInteract();
        if(X == 8)
            Buttons[ButtonDirections.IndexOf(Direction.Right)].OnInteract();
        if(Y == 0)
            Buttons[ButtonDirections.IndexOf(Direction.Up)].OnInteract();
        if(Y == 8)
            Buttons[ButtonDirections.IndexOf(Direction.Down)].OnInteract();

        yield return null;
    }

    private IEnumerable SpinToDirection()
    {
        Direction exitdir;
        switch(ColoredButtonColor)
        {
            case Color.Red:
                exitdir = Direction.Up;
                break;
            case Color.Green:
                exitdir = Direction.Right;
                break;
            case Color.Blue:
                exitdir = Direction.Down;
                break;
            case Color.Yellow:
                exitdir = Direction.Left;
                break;
            default:
                throw new Exception("The colored button was not RGBY. This shouldn't have happened, so please contact Bagels.");
        }
        Direction cdir = ButtonDirections[ColoredButton];

        string spin = GetSpin(cdir, exitdir);

        if(spin == "0")
            yield break;
        List<Vector2Int> locs = new List<Vector2Int>();
        if(spin == "90" || spin == "180")
        {
            locs.Add(new Vector2Int(0, 1));
            locs.Add(new Vector2Int(7, 0));
            locs.Add(new Vector2Int(8, 7));
            locs.Add(new Vector2Int(1, 8));
        }
        if(spin == "270" || spin == "180")
        {
            locs.Add(new Vector2Int(1, 0));
            locs.Add(new Vector2Int(8, 1));
            locs.Add(new Vector2Int(7, 8));
            locs.Add(new Vector2Int(0, 7));
        }

        foreach(object e in SearchAndMoveTo(locs))
            yield return e;

        if(X == 1)
            Buttons[ButtonDirections.IndexOf(Direction.Left)].OnInteract();
        if(X == 7)
            Buttons[ButtonDirections.IndexOf(Direction.Right)].OnInteract();
        if(Y == 1)
            Buttons[ButtonDirections.IndexOf(Direction.Up)].OnInteract();
        if(Y == 7)
            Buttons[ButtonDirections.IndexOf(Direction.Down)].OnInteract();
        yield return new WaitForSeconds(0.1f);

        if(spin == "180")
            foreach(object e in SpinToDirection())
                yield return e;
    }

    private IEnumerable SearchAndMoveTo(IEnumerable<Vector2Int> locs)
    {
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        Dictionary<Vector2Int, Vector2Int> parents = new Dictionary<Vector2Int, Vector2Int>();
        Queue<Vector2Int> q = new Queue<Vector2Int>();
        Vector2Int startLoc = new Vector2Int(X, Y);
        q.Enqueue(startLoc);
        Vector2Int exit;

        while(q.Count > 0)
        {
            Vector2Int working = q.Dequeue();
            if(!visited.Add(working))
                continue;
            if(locs.Contains(working))
            {
                exit = working;
                goto found;
            }

            List<Vector2Int> n = new List<Vector2Int>();
            if((Maze[working.y][working.x] & Direction.Left) == Direction.Left)
                n.Add(new Vector2Int(working.x - 1, working.y));
            if((Maze[working.y][working.x] & Direction.Right) == Direction.Right)
                n.Add(new Vector2Int(working.x + 1, working.y));
            if((Maze[working.y][working.x] & Direction.Down) == Direction.Down)
                n.Add(new Vector2Int(working.x, working.y + 1));
            if((Maze[working.y][working.x] & Direction.Up) == Direction.Up)
                n.Add(new Vector2Int(working.x, working.y - 1));
            n.RemoveAll(v => v.x < 0 || v.x > 8 || v.y < 0 || v.y > 8);

            foreach(Vector2Int cn in n)
            {
                if(visited.Contains(cn))
                    continue;
                q.Enqueue(cn);
                parents[cn] = working;
            }
        }

        throw new Exception("There was no valid turn found. This shouldn't have happened, so please contact Bagels.");

        found:;

        List<Vector2Int> path = new List<Vector2Int>();
        Vector2Int p = exit;
        while(p != startLoc)
        {
            path.Add(p);
            p = parents[p];
        }

        for(int i = path.Count - 1; i >= 0; i--)
        {
            Vector2Int dif = path[i] - new Vector2Int(X, Y);
            if(dif.x == 1)
                Buttons[ButtonDirections.IndexOf(Direction.Right)].OnInteract();
            if(dif.x == -1)
                Buttons[ButtonDirections.IndexOf(Direction.Left)].OnInteract();
            if(dif.y == 1)
                Buttons[ButtonDirections.IndexOf(Direction.Down)].OnInteract();
            if(dif.y == -1)
                Buttons[ButtonDirections.IndexOf(Direction.Up)].OnInteract();

            yield return new WaitForSeconds(0.1f);
        }
    }

    private string GetSpin(Direction cdir, Direction exitdir)
    {
        IEnumerable<int> dirs = new[] { cdir, exitdir }.Select(d => (int)d);

        if(dirs.SequenceEqual(new[] { 1, 2 }) ||
            dirs.SequenceEqual(new[] { 2, 1 }) ||
            dirs.SequenceEqual(new[] { 4, 8 }) ||
            dirs.SequenceEqual(new[] { 8, 4 }))
            return "180";
        if(dirs.SequenceEqual(new[] { 1, 8 }) ||
            dirs.SequenceEqual(new[] { 8, 2 }) ||
            dirs.SequenceEqual(new[] { 2, 4 }) ||
            dirs.SequenceEqual(new[] { 4, 1 }))
            return "90";
        if(dirs.SequenceEqual(new[] { 1, 4 }) ||
            dirs.SequenceEqual(new[] { 4, 2 }) ||
            dirs.SequenceEqual(new[] { 2, 8 }) ||
            dirs.SequenceEqual(new[] { 8, 1 }))
            return "270";
        return "0";
    }
}
