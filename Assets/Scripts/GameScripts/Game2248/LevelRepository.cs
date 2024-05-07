using System;
using System.Collections.Generic;
using Core.Repositories;
using GameScripts.MergeSquares;
using GameScripts.MergeSquares.Models;
using GoogleSheetsToUnity;
using LargeNumbers;
using Unity.VisualScripting;
using UnityEngine;
using Utils;

namespace GameScripts.Game2248
{
    [CreateAssetMenu(fileName = "2248LevelRepository", menuName = "Repositories/2248LevelRepository")]
    public class LevelRepository : AbstractRepository<List<GridModel>>
    {
        public override string AssociatedWorksheet => "2248Levels";

        public bool TryGetById(int id, ref GridModel def)
        {
            var level = data.GetBy(value => value.id == id);
            if (level == null)
            {
                return false;
            }
            def = level;
            return true;
        }
        
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
                        // case "Chances":
                        //     var chances = TableParser.ParseStringList(rowCell.value);
                        //     foreach (var chance in chances)
                        //     {
                        //         var values = TableParser.ParseStringList(chance, '-');
                        //         model.nextValues.Add(new NextValue
                        //         {
                        //             value = Convert.ToInt32(values[0]),
                        //             chance = Convert.ToInt32(values[1])
                        //         });
                        //     }
                        //     break;
                        case "Reward":
                            model.reward = Convert.ToInt32(rowCell.value);
                            break;
                        case "UnitsPows":
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
                                    largeValue = new LargeNumber(Math.Pow(2, int.Parse(values[2])))
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
                                        valueLarge = new LargeNumber(int.Parse(taskParams[1]))
                                    };
                                    break;
                                case "Cell":
                                    model.taskModel = new TaskModel
                                    {
                                        type = ETaskType.GetCellWithValue,
                                        valueLarge = new LargeNumber(int.Parse(taskParams[1]))
                                    };
                                    break;
                                case "Lines":
                                    model.taskModel = new TaskModel
                                    {
                                        type = ETaskType.MakeLines,
                                        valueLarge = new LargeNumber(int.Parse(taskParams[1]))
                                    };
                                    break;
                                case "Endless":
                                    model.taskModel = new TaskModel
                                    {
                                        type = ETaskType.Endless,
                                        valueLarge = LargeNumber.zero
                                    };
                                    break;
                            }
                            break;
                        case "StartPows":
                            var startPows = TableParser.ParseIntList(rowCell.value, '-');
                            model.startPows = startPows;
                            break;
                        case "PowUpdates":
                            var PowUpdates = TableParser.ParseStringList(rowCell.value);
                            model.powUpdateConditions.Clear();
                            foreach (var powUpdate in PowUpdates)
                            {
                                var values = TableParser.ParseIntList(powUpdate, '+');
                                model.powUpdateConditions.Add(new PowUpdateCondition()
                                {
                                    updatePow = values[0],
                                    deletePow = values[1],
                                    addPow = values[2]
                                });
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