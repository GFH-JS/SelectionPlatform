using MongoDB.Bson.IO;
using SelectionPlatform.Models.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace SelectionPlatform.Services.CentrifugeCalculate
{
    public class InitData
    {
        public ProjectInfoEntity getFakeData()
        {
           
            ProjectInfoEntity projectInfo = new ProjectInfoEntity();
            var props = projectInfo.proofData.GetType().GetProperties();
            foreach (var prop in props)
            {
                //prop.SetValue(projectInfo, Convert.ChangeType("value", prop.PropertyType));

                if (prop.PropertyType == typeof(InputParam))
                {
                    //InputParam inputParam2 = new InputParam();
                    //var paramProps = inputParam2.GetType().GetProperties();

                    //foreach (var prop2 in paramProps) 
                    //{
                    //    prop2.SetValue(inputParam2, Convert.ChangeType("1", prop2.PropertyType));
                    //}
                    InputParam inputParam2 = new InputParam();
                    inputParam2.paramName = prop.Name;
                    inputParam2.paramValue = Random.Shared.Next(1,100).ToString();
                    inputParam2.paramUnit = "";
                    //inputParam2.paramComboxValue.Add(new DropDownListData { text = "220", value = "220", selected = true });
                    //inputParam2.paramComboxValue.Add(new DropDownListData { text = "180", value = "180", selected = false });
                    prop.SetValue(projectInfo.proofData, inputParam2);
                }
            }
            projectInfo.id = new MongoDB.Bson.ObjectId("66c972c17f6f63b3e2099f20");
            projectInfo.projectId = "001";
            projectInfo.projectName = "离心机选型数据";
            projectInfo.projectVersion = "V1";
            projectInfo.metricInch = "MM";

            InputParam inputParam = new InputParam();
            inputParam.paramName = "inputPwoer";
            inputParam.paramUnit = "V";
            inputParam.paramComboxValue.Add(new DropDownListData { label = "220",value = "220",selected = true});
            inputParam.paramComboxValue.Add(new DropDownListData { label = "180", value = "180", selected = false });
            projectInfo.proofData.inputPwoer = inputParam;

            InputParam capacity = new InputParam();
            capacity.paramName = "capacity";
            capacity.paramUnit = "kW";
            capacity.paramValue = "12.5";
            projectInfo.proofData.capacity = capacity;

            InputParam partloadLoadPoint = new InputParam();
            partloadLoadPoint.paramName = "partloadLoadPoint";
            partloadLoadPoint.paramUnit = "";
            partloadLoadPoint.paramValue = "partload";
            projectInfo.proofData.partloadLoadPoint = partloadLoadPoint;

            for (int i = 0; i < 5; i++)
            {
                PartloadDatalist_group partloadDatalist_Group = new PartloadDatalist_group();
                partloadDatalist_Group.name = $"Group{i+1}";
                //projectInfo.partloadDatalists.Add(partloadDatalist_Group);
                for (int j = 0; j < 5; j++)
                {
                    PartloadDatalist partloadDatalist = new PartloadDatalist();
                    partloadDatalist.name = $"Children{i + 1}.{j + 1}";
                    //partloadDatalist.k1 = Random.Shared.NextDouble().ToString("F2");
                    //partloadDatalist.k2 = Random.Shared.NextDouble().ToString("F2");
                    //partloadDatalist.k3 = Random.Shared.NextDouble().ToString("F2");
                    //partloadDatalist.k4 = Random.Shared.NextDouble().ToString("F2");
                    //partloadDatalist.k5 = Random.Shared.NextDouble().ToString("F2");
                    partloadDatalist_Group.children.Add(partloadDatalist);

                }
            }

            return projectInfo;
        }


        /// <summary>
        /// 获取默认值
        /// </summary>
        /// <returns></returns>
        public ProjectInfoEntity InitInputData()
        {
            
            string paramlist = File.ReadAllText("ParamsInput.json");
            var projInfo = JsonSerializer.Deserialize<ProjectInfoEntity>(paramlist);
      
            return projInfo;
        }
    }
}
