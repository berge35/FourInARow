using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour {

    public Material p1;
    public Material p2;
    Color team;     //team color

    public Text game_over_text;     //game over text

    public GameObject prefab;       //prefab for pieces

    public GameObject[] spawns;     //array for spawn points of pieces

    public int turn = 1;                //current turn

    int spawn_slot = 3;

    public struct slot
    {
        public int team;
    }

    public slot[] positions = new slot[42];

	// Use this for initialization
	void Start () {
        //initialize text
        game_over_text.text = " ";

        //initialize positions
        for (int i = 0; i < 42; i++)
        {
            positions[i].team = -1;
        }

        spawns = GameObject.FindGameObjectsWithTag("spawn");    //get spawns
        reset_mat();    //clear spawns
        //get correct color
        if (turn == 1)
            team = Color.red;
        else
            team = Color.blue;
	}
	
	// Update is called once per frame
	void Update () {
        spawns[spawn_slot].GetComponent<Renderer>().enabled = true;
        spawns[spawn_slot].GetComponent<Renderer>().material.color = team;
        if (Input.GetKeyDown("left"))
        {
            placePiece(2);
        }
        if (Input.GetKeyDown("right"))
        {
            placePiece(1);
        }
        if (Input.GetKeyDown("down"))
        {
            spawns[spawn_slot].GetComponent<Renderer>().enabled = false;
            placePiece(3);
            //check for a winning combonation
            List<int> win = global_win();   //get possible winning combonation
            Debug.Log(win.Count);
            if (win.Count >= 4)
                game_over(win);

        }
	}


    void reset_mat()
    {
        foreach (GameObject spawn in spawns){
            spawn.GetComponent<Renderer>().enabled = false;
        }
    }

    void placePiece(int key)       //
    {
        
        //let user pick slot to drop
        
        if (key == 1)
        {
            if (spawn_slot != 6)
            {
                reset_mat();
                spawn_slot++;
                
            }
        }
        if (key == 2)
        {
            if (spawn_slot != 0)
            {
                reset_mat();
                spawn_slot--;

            }
        }
        if (key == 3)      //user chooses a slot
        {
            //instantiate new peace with team color
            GameObject go = (GameObject)Instantiate(prefab, spawns[spawn_slot].gameObject.transform.position, transform.rotation * Quaternion.Euler(90, 0, 0));      //spawn piece in middle slot
            go.GetComponent<Renderer>().material.color = team;

            logical_place(spawn_slot);  //logically place the piece

            //change turn
            if (turn == 1)
                turn = 2;
            else
                turn = 1;

            //get correct color
            if (turn == 1)
                team = Color.red;
            else
                team = Color.blue;
        }
    }

    void logical_place(int slot)    //this will place the piece in terms of the positions array in the code
    {
        if (slot < 42)
        {
            if (positions[slot].team == -1)     //if first slot is available, proceed
            {
                if (slot > 6)   //if we are past the first row
                {
                    positions[slot - 7].team = -1;  //reset the slot above as we fall down one to keep it open
                }
                positions[slot].team = turn;    //set this slot as taken by current team
                logical_place(slot + 7);        //recur for next row down
            }
        }
    }


    //Search for a winning combination
    //4 in a row: horizontal, vertical, two diagonals
    List<int> global_win()     //search all positions and check for wins at each one
    {
        List<int> win = new List<int>();   //positions that encompass a win

        for (int j = 0; j < 42; j++)
        {
            for (int i = 0; i < 4; i++)
            {     //search all four directions for each point
                win = local_win(win, j, positions[j].team, i);
                Debug.Log("FOUND " + win.Count);
                if (win.Count >= 4)    //check if combonation of 4 was found
                    return win;
                else
                    win.Clear();   //if not found, reset the list
            }
        }
        return win;
    }

    List<int> local_win(List<int> win, int s, int team, int dir)     //return a winning combination, if win >= 4 we have a win
    {
        if (team != -1)     //if the selected slot is not empty, start search
        {
            if (!win.Contains(s))
                win.Add(s);
            if (dir == 0)   //horizontal
            {
                if (s % 7 > 0 && positions[s - 1].team == team)  //check that the slot is not on the edge
                {
                    win.Add(s - 1);        //put index into win array
                    Debug.Log("Searching: " + s + "|dir: " + dir + "|next: " + (s - 1) + "|length: " + win.Count);
                    local_win(win, s - 1, team, dir);   //run again one to the left
                }
            }
            if (dir == 1)   //vertical
            {
                if (s - 7 >= 0 && positions[s - 7].team == team)    //check that slot above exists
                {
                    win.Add(s - 7);
                    Debug.Log("Searching: " + s + "|dir: " + dir + "|next: " + (s - 7) + "|length: " + win.Count);
                    local_win(win, s - 7, team, dir);
                }
            }
            if (dir == 2)   //diagonal-left
            {
                if (((s - 8) % 7 < s % 7) && s-8 >= 0 && positions[s - 8].team == team)    //check that slot above and to the left exists
                {
                    win.Add(s - 8);
                    Debug.Log("Searching: " + s + "|dir: " + dir + "|next: " + (s - 8) + "|length: " + win.Count);
                    local_win(win, s - 8, team, dir);
                }
            }
            if (dir == 3)   //diagonal-right
            {
                if (((s - 6) % 7 > s % 7) && s - 6 >= 0 && positions[s - 6].team == team)    //check that slot above and to the right exists
                {
                    win.Add(s - 6);
                    Debug.Log("Searching: " + s + "|dir: " + dir + "|next: " + (s - 6) + "|length: " + win.Count);
                    local_win(win, s - 6, team, dir);
                }
            }
        }

        return win;
    }

    void game_over(List<int> win)   //winning combination found, show graphical display and reset board
    {
        Debug.Log("WE HAVE A WIN!!");
        string steam;
        switch (positions[win[0]].team)
        {
            case 1:
                steam = "One";
                break;
            case 2:
                steam = "Two";
                break;
            default:
                steam = "Error";
                break;
        }
        //get correct color
        if (positions[win[0]].team == 1)
            team = Color.red;
        else
            team = Color.blue;

        game_over_text.text = ("Player " + steam + " Wins!");
        game_over_text.color = team;
    }

}
