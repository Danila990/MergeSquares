using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Repositories;
using GoogleSheetsToUnity;
using Rewards;
using Rewards.Models;
using UnityEngine;
using Utils;

[Serializable]
public class WheelRollData
{
    public string id;
    public bool isCallback;
    public int fixedSector;
    // public List<WheelSectorData> sectors = new();
    public List<RewardModel> rewards;
}

[CreateAssetMenu(fileName = "WheelRollDataRepository", menuName = "Repositories/WheelRollDataRepository")]
public class WheelRollDataRepository : AbstractRepository<List<WheelRollData>>, ISerializationCallbackReceiver
{
    [SerializeField] private RewardRepository rewardRepository;
    [SerializeField] public override string AssociatedWorksheet => "2248RollData";
    
    public WheelRollData GetResults(string id)
    {
        if (!ContainsField(id))
        {
            Debug.LogWarning($"WheelRollDataRepository is not contains data for ({id})");
            return null;
        }

        return data.GetBy(value => value.id == id);
    }

    public bool ContainsField(string id)
    {
        var fields = data.Where(value => value.id == id).ToList();
        return fields.Count > 0;
    }

    public override void UpdateRepository(GstuSpreadSheet spreadSheet)
    {
        data = new List<WheelRollData>();
        foreach (var cell in spreadSheet.columns["Id"])
        {
            if (cell.rowId == "Id")
            {
                continue;
            }
            var rolldata = new WheelRollData();
            data.Add(rolldata);
            
            var row = spreadSheet.rows[cell.value];
            
            foreach (var rowCell in row)
            {
                switch (rowCell.columnId)
                {
                    case "Name":
                        rolldata.id = rowCell.value;
                        break;
                    // case "IsNegative":
                    //     SetNegatives(rolldata, rowCell.value);
                    //     break;
                    // case "IsAdv":
                    //     SetAdv(rolldata, rowCell.value);
                    //     break;
                    // case "Cost":
                    //     SetCosts(rolldata, rowCell.value);
                    //     break;
                    case "FixedSector":
                        SetFixed(rolldata, rowCell.value);
                        break;
                    case "Currencies":
                        SetCurrencies(rolldata, rowCell.value);
                        break;
                    case "IsCallback":
                        SetCallback(rolldata, rowCell.value);
                        break;
                    default:
                        break;
                }
            }
        }
    }
    
    public void OnBeforeSerialize()
    {

    }

    public void OnAfterDeserialize()
    {

    }

    // private void SetNegatives(WheelRollData data, string line)
    // {
    //     foreach (var b in ParseLine(line, Boolean.Parse))
    //     {
    //         var sector = new WheelSectorData();
    //         sector.isNegative = b;
    //         data.sectors.Add(sector);
    //     }
    // } 
    
    // private void SetAdv(WheelRollData data, string line)
    // {
    //     var parseParams = ParseLine(line, Boolean.Parse);
    //     for (int i = 0; i < Math.Min(data.sectors.Count, parseParams.Count); i++)
    //     {
    //         data.sectors[i].isAdv = parseParams[i];
    //     }
    // } 
    //
    // private void SetCosts(WheelRollData data, string line)
    // {
    //     var parseParams = ParseLine(line, Convert.ToInt32);
    //     for (int i = 0; i < Math.Min(data.sectors.Count, parseParams.Count); i++)
    //     {
    //         data.sectors[i].cost = parseParams[i];
    //     }
    // }

    private void SetFixed(WheelRollData data, string line)
    {
        data.fixedSector = Convert.ToInt32(line);
    }

    private void SetCallback(WheelRollData data, string line)
    {
        data.isCallback = line == "TRUE";
    }
    
    private List<T> ParseLine<T>(string line, Func<string, T> cast)
    {
        var parts = TableParser.ParseStringList(line);
        var list = new List<T>();
        foreach (var e in parts)
        {
            var param = TableParser.ParseStringList(e, '-');
            list.Add(cast(param.Last()));
        }
        return list;
    }

    private void SetCurrencies(WheelRollData data, string line)
    {
        var rewards = rewardRepository.ParseCurrencies(line);
        data.rewards = rewards;
    }
}


