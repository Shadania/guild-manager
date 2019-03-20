using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// register a guild! gameobject's name is the guild's name
public class Guild : MonoBehaviour
{
    #region Declarations
    public GameObject MemberPrefab;
    public List<GameObject> GuildMembers;
    public GameObject GuildLeader;
    public Transform MemberSpawnPoint;
    public float NewMemberSpawnPointRadius = 1.0f;
    public GuildManagementDesk Desk;
    public GameObject HomeLocations;
    public string GuildName = "My Guild";

    public Dictionary<GameObject, int> Influences = new Dictionary<GameObject, int>();
    public Dictionary<GameObject, int> VillCouncilPosition = new Dictionary<GameObject, int>();
    #endregion Declarations

    public void AddMember(GameObject newMemberCard)
    {
        GameObject newMember = Instantiate(MemberPrefab, MemberSpawnPoint.transform);

        GuildMemberController guildMemberContr = newMember.GetComponent<GuildMemberController>();
        RecruitCardBehaviour recruitCard = newMemberCard.GetComponent<RecruitCardBehaviour>();
            
        // add stats of the recruit card to the member object
        guildMemberContr.NameMesh.text = recruitCard.NameText.text;
        guildMemberContr.MemberMenuName.text = recruitCard.NameText.text;
        
        // adding, updating, childing
        GuildMembers.Add(newMember);
        guildMemberContr.SetGuild(this);
        
        // newMember.transform.position = MemberSpawnPoint.transform.position;
        // newMember.transform.localPosition = new Vector3(0, 0, 0);

        Vector3 SpawnOffset = new Vector3();

        SpawnOffset.x = Random.Range(-NewMemberSpawnPointRadius, NewMemberSpawnPointRadius);
        SpawnOffset.z = Random.Range(-NewMemberSpawnPointRadius, NewMemberSpawnPointRadius);
        int test = Random.Range(0, 2);
        if (test == 0)
        {
            SpawnOffset.z *= -1;
        }

        newMember.transform.localPosition += SpawnOffset;

        newMember.transform.parent = /*MemberSpawnPoint.*/transform;

        newMember.transform.localScale = new Vector3(1, 1, 1);
    }
    public void AddMemberFromVillager(VillagerBehaviour vill)
    {
        GameObject newMember = Instantiate(MemberPrefab, MemberSpawnPoint.transform);

        GuildMemberController gmContr = newMember.GetComponent<GuildMemberController>();
        gmContr.NameMesh.text = vill.VillagerName;
        gmContr.MemberMenuName.text = vill.VillagerName;

        GuildMembers.Add(newMember);
        gmContr.SetGuild(this);
        
        // newMember.transform.position = MemberSpawnPoint.transform.position;
        // newMember.transform.localPosition = new Vector3(0, 0, 0);

        Vector3 SpawnOffset = new Vector3();

        SpawnOffset.x = Random.Range(-NewMemberSpawnPointRadius, NewMemberSpawnPointRadius);
        SpawnOffset.z = Random.Range(-NewMemberSpawnPointRadius, NewMemberSpawnPointRadius);
        int test = Random.Range(0, 2);
        if (test == 0)
        {
            SpawnOffset.z *= -1;
        }

        newMember.transform.localPosition += SpawnOffset;

        newMember.transform.parent =  /*MemberSpawnPoint.*/transform;

        newMember.transform.localScale = new Vector3(1, 1, 1);
    }

    public int GetInfluenceFor(GameObject village)
    {
        int temp;
        if (Influences.TryGetValue(village, out temp))
            return temp;

        Influences.Add(village, 0);
        return Influences[village];
    }
    public void RemoveInfluenceFor(GameObject village, int amt)
    {
        int temp;
        if (Influences.TryGetValue(village, out temp))
            Influences[village] -= amt;
        else
            Debug.Log("Could not find village with that ID in RemoveInfluence");

        Desk.UpdateInfluenceMenu();
    }
    public void AddInfluenceFor(GameObject village, int amt)
    {
        int temp;
        if (Influences.TryGetValue(village, out temp))
            Influences[village] += amt;
        else
            Influences.Add(village, amt);

        Desk.UpdateInfluenceMenu();
        village.GetComponent<Village>().TownHall.GetComponent<Townhall>().HandleMyInfluenceChanged();
    }

    public int GetAmtCouncilPositionsFor(GameObject village)
    {
        int temp = 0;
        if (VillCouncilPosition.TryGetValue(village, out temp))
        {
            return temp;
        }
        VillCouncilPosition.Add(village, 0);
        return 0;
    }
    public void RemoveCouncilPositionFor(GameObject village)
    {
        int temp;
        if (VillCouncilPosition.TryGetValue(village, out temp))
        {
            if (VillCouncilPosition[village] > 0)
                VillCouncilPosition[village]--;
            else
                Debug.LogWarning("Tried to remove council seat that didn't exist");
        }
        else Debug.LogWarning("Tried to remove council seat that didn't exist");
    }
    public void AddCouncilPositionFor(GameObject village)
    {
        int temp;
        if (VillCouncilPosition.TryGetValue(village, out temp))
            VillCouncilPosition[village]++;
        else
            VillCouncilPosition.Add(village, 1);
    }

    public List<GuildMemberController> GetGuildMembersInRange(Vector3 pos, float range)
    {
        List<GuildMemberController> result = new List<GuildMemberController>();

        for (int i = 0; i < GuildMembers.Count; ++i)
        {
            if (!GuildMembers[i])
            {
                GuildMembers.Remove(GuildMembers[i]);
                continue;
            }

            if ((GuildMembers[i].transform.position - pos).sqrMagnitude < (range * range))
            {
                result.Add(GuildMembers[i].GetComponent<GuildMemberController>());
            }
        }

        return result;
    }
}
