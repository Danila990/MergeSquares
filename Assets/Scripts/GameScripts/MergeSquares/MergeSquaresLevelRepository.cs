using System;
using System.Collections.Generic;
using Core.Repositories;
using GameScripts.MergeSquares.Models;
using GoogleSheetsToUnity;
using UnityEngine;
using Utils;

namespace GameScripts.MergeSquares
{
    [CreateAssetMenu(fileName = "MergeSquaresLevelRepository", menuName = "Repositories/MergeSquaresLevelRepository")]
    public class MergeSquaresLevelRepository : AbstractRepository<List<GridModel>>
    {
        public override string AssociatedWorksheet => "MergeSquaresLevels";

        public GridModel GetById(int id, GridModel def) => data.GetBy(value => value.id == id) ?? def;
        
        public override void UpdateRepository(GstuSpreadSheet spreadSheet)
        {
            data = ParseTable(spreadSheet);
        }

        private List<GridModel> ParseTable(GstuSpreadSheet spreadSheet)
        {
            var context = new TableParseContext<List<GridModel>> {ss = spreadSheet};
            context.name = nameof(MergeSquaresLevelRepository);
            context.init = c => c.value = new List<GridModel>();
            context.parseRow = c =>
            {
                var model = new GridModel { id = Convert.ToInt32(c.cell.value) };
                foreach (var rowCell in c.row)
                {
                    if (rowCell.value.Trim() == string.Empty)
                        continue;

                    if (rowCell.value.Trim() == "-")
                        continue;

                    switch (rowCell.columnId)
                    {
                        case "Size":
                            var position = TableParser.ParseStringList(rowCell.value, '-');
                            model.size = new Vector2Int(
                                Convert.ToInt32(position[0]),
                                Convert.ToInt32(position[1])
                            );
                            break;
                        case "Chances":
                            var chances = TableParser.ParseStringList(rowCell.value);
                            foreach (var chance in chances)
                            {
                                var values = TableParser.ParseStringList(chance, '-');
                                model.nextValues.Add(new NextValue
                                {
                                    value = Convert.ToInt32(values[0]),
                                    chance = Convert.ToInt32(values[1])
                                });
                            }
                            break;
                        case "Reward":
                            model.reward = Convert.ToInt32(rowCell.value);
                            break;
                        case "Objects":
                            var objects = TableParser.ParseStringList(rowCell.value);
                            foreach (var obj in objects)
                            {
                                var values = TableParser.ParseStringList(obj, '-');
                                model.units.Add(new UnitModel()
                                {
                                    position = new Vector2Int(
                                        Convert.ToInt32(values[0]),
                                        Convert.ToInt32(values[1])
                                    ),
                                    value = Convert.ToInt32(values[2])
                                });
                            }
                            break;
                        case "Task":
                            var taskParams = TableParser.ParseStringList(rowCell.value, '-');
                            switch (taskParams[0])
                            {
                                case "Sum":
                                    model.taskModel = new TaskModel
                                    {
                                        type = ETaskType.CollectPoints,
                                        value = Convert.ToInt32(taskParams[1])
                                    };
                                    break;
                                case "Cell":
                                    model.taskModel = new TaskModel
                                    {
                                        type = ETaskType.GetCellWithValue,
                                        value = Convert.ToInt32(taskParams[1])
                                    };
                                    break;
                                case "Merges":
                                    model.taskModel = new TaskModel
                                    {
                                        type = ETaskType.MakeMerges,
                                        value = Convert.ToInt32(taskParams[1])
                                    };
                                    break;
                                case "Endless":
                                    model.taskModel = new TaskModel
                                    {
                                        type = ETaskType.Endless,
                                        value = 0
                                    };
                                    break;
                            }
                            break;
                    }   
                }

                c.value.Add(model);
            };

            return TableParser.ParseTable(context);
        }
    }
}