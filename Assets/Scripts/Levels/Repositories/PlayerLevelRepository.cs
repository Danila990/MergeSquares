using System;
using System.Collections.Generic;
using Core.Repositories;
using GoogleSheetsToUnity;
using Levels.Models;
using Rewards;
using Rewards.Models;
using UnityEngine;
using Utils;

namespace Levels.Repositories
{
    [CreateAssetMenu(fileName = "PlayerLevelRepository", menuName = "Repositories/PlayerLevelRepository")]
    public class PlayerLevelRepository : AbstractRepository<List<PlayerLevelModel>>
    {
        [SerializeField] private RewardRepository rewardRepository;
        public override string AssociatedWorksheet => "PlayerLevels";

        public PlayerLevelModel GetById(int id) => data.GetBy(value => value.id == id);
        
        public override void UpdateRepository(GstuSpreadSheet spreadSheet)
        {
            data = ParseTable(spreadSheet);
        }

        private List<PlayerLevelModel> ParseTable(GstuSpreadSheet spreadSheet)
        {
            var context = new TableParseContext<List<PlayerLevelModel>> {ss = spreadSheet};
            context.name = nameof(PlayerLevelRepository);
            context.init = c => c.value = new List<PlayerLevelModel>();
            context.parseRow = c =>
            {
                var model = new PlayerLevelModel { id = Convert.ToInt32(c.cell.value) };
                model.rewards = new List<RewardModel>();
                foreach (var rowCell in c.row)
                {
                    if (rowCell.value.Trim() == string.Empty)
                        continue;

                    if (rowCell.value.Trim() == "-")
                        continue;

                    switch (rowCell.columnId)
                    {
                        case "Experience":
                            model.experience = Convert.ToInt32(rowCell.value);
                            break;
                        case "Currencies":
                            model.rewards.AddRange(rewardRepository.ParseCurrencies(rowCell.value));
                            break;
                        case "Units":
                            model.rewards.AddRange(rewardRepository.ParseUnits(rowCell.value));
                            break;
                        case "RvCurrencies":
                            model.rewards.AddRange(rewardRepository.ParseCurrencies(rowCell.value, true));
                            break;
                        case "RvUnits":
                            model.rewards.AddRange(rewardRepository.ParseUnits(rowCell.value, true));
                            break;
                    }   
                }

                c.value.Add(model);
            };

            return TableParser.ParseTable(context);
        }
    }
}