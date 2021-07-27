using KeepCoding;
using System;
using System.Collections;
using UnityEngine;
using System.Linq;

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
}
